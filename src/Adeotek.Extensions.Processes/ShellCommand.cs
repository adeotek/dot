using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Adeotek.Extensions.Processes;

public class ShellCommand
{
    public delegate void OutputReceivedEventHandler(object sender, OutputReceivedEventArgs e);

    public const string BashShell = "/bin/bash";
    public const string ShShell = "/bin/sh";
    public const string PsShell = "pwsh";
    public const string PowerShell = "powershell";
    public const string CommandPromptShell = "cmd";

    public static readonly bool IsWindowsPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public string ShellName
    {
        get => _shellName;
        set
        {
            if (_shellName != value) _prepared = false;
            _shellName = value;
        }
    }

    public string Command
    {
        get => _command;
        set
        {
            if (_command != value) _prepared = false;
            _command = value;
        }
    }

    public string[] Arguments => _arguments.ToArray();

    public bool IsScript
    {
        get => _isScript;
        set
        {
            if (_isScript != value) _prepared = false;
            _isScript = value;
        }
    }

    public bool IsElevated { get; set; }
    public string ProcessFile { get; private set; } = "";
    public string ProcessArguments { get; private set; } = "";
    public int StatusCode { get; private set; } = -1;
    public List<string> StdOutput { get; } = new();
    public List<string> ErrOutput { get; } = new();
    public event OutputReceivedEventHandler? OnStdOutput;
    public event OutputReceivedEventHandler? OnErrOutput;

    private bool _prepared;
    private string _shellName = "";
    private string _command = "";
    private List<string> _arguments = new();
    private bool _isScript;
    
    public bool IsSuccess(string resource, bool checkFirstLineOnly = false)
    {
        if (string.IsNullOrEmpty(resource))
        {
            return StatusCode == 0;
        }

        if (StatusCode != 0)
        {
            return false;
        }
        
        return checkFirstLineOnly
            ? (StdOutput.FirstOrDefault()?.Contains(resource) ?? false)
            : StdOutput.Exists(e => e.Contains(resource));
    }

    public ShellCommand AddArgument(string value)
    {
        _prepared = false;
        _arguments.Add(value);
        return this;
    }
    
    public ShellCommand AddArguments(IEnumerable<string> range)
    {
        _prepared = false;
        _arguments.AddRange(range);
        return this;
    }

    public ShellCommand SetArgumentAt(int index, string value)
    {
        _prepared = false;
        _arguments[index] = value;
        return this;
    }
    
    public ShellCommand ReplaceArgument(string currentValue, string newValue)
    {
        _prepared = false;
        _arguments[_arguments.IndexOf(currentValue)] = newValue;
        return this;
    }
    
    public ShellCommand RemoveArgument(string item)
    {
        _prepared = false;
        _arguments.Remove(item);
        return this;
    }
    
    public ShellCommand RemoveArgumentAt(int index)
    {
        _prepared = false;
        _arguments.RemoveAt(index);
        return this;
    }
    
    public ShellCommand ClearArguments()
    {
        _prepared = false;
        _arguments.Clear();
        return this;
    }

    public int Execute(string command, string[]? args = null, string? shellName = null, bool isScript = false,
        bool isElevated = false)
    {
        Command = command;
        _arguments = args?.ToList() ?? new List<string>();
        ShellName = shellName ?? "";
        IsScript = isScript;
        IsElevated = isElevated;

        return Execute();
    }

    public int Execute()
    {
        Reset();
        Prepare();

        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = ProcessFile,
            Arguments = ProcessArguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (IsElevated)
        {
            processStartInfo.Verb = "runas";
        }

        using Process process = new Process();
        process.StartInfo = processStartInfo;
        process.OutputDataReceived += ProcessStdOutput;
        process.ErrorDataReceived += ProcessErrOutput;
        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            StatusCode = process.ExitCode;
        }
        catch (Exception e)
        {
            string eventArgsData = e.Message;
            ErrOutput.Add(e.Message);
            if (e.StackTrace is not null)
            {
                eventArgsData = $"{eventArgsData}{Environment.NewLine}{e.StackTrace}";
                ErrOutput.Add(e.StackTrace);
            }

            OnErrOutput?.Invoke(this, new OutputReceivedEventArgs(eventArgsData, true));
            StatusCode = -1;
        }

        return StatusCode;
    }

    public ShellCommand Prepare()
    {
        if (_prepared)
        {
            return this;
        }
        
        ProcessFile = string.IsNullOrWhiteSpace(ShellName) ? Command : ShellName;
        ProcessArguments = GetShellArguments(Command, Arguments.ToArray(), ShellName, IsScript);
        return this;
    }

    private void ProcessStdOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is null)
        {
            return;
        }

        OnStdOutput?.Invoke(this, new OutputReceivedEventArgs(e.Data));
        StdOutput.Add(e.Data);
    }

    private void ProcessErrOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is null)
        {
            return;
        }

        OnErrOutput?.Invoke(this, new OutputReceivedEventArgs(e.Data, true));
        ErrOutput.Add(e.Data);
    }

    private void Reset()
    {
        StatusCode = -1;
        StdOutput.Clear();
        ErrOutput.Clear();
    }

    private static string GetShellArguments(string command, string[] args, string shellName, bool isScript = false)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new Exception("Command cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(shellName))
        {
            if (isScript)
            {
                throw new Exception("ShellName is required for executing scripts (IsScript is true)");
            }

            return args.Length > 0 ? $"{string.Join(' ', args)}" : "";
        }

        return isScript
            ? $"{GetArgsPrepend(shellName, isScript)}{command}{(args.Length > 0 ? $" {string.Join(' ', args)}" : "")}"
            : $"{GetArgsPrepend(shellName, isScript)}\"{command}{(args.Length > 0 ? $" {string.Join(' ', args)}" : "")}\"";
    }

    private static string GetArgsPrepend(string shellName, bool isScript = false)
    {
        return shellName switch
        {
            BashShell or ShShell or PowerShell => isScript ? "" : "-c ",
            PsShell => $"{(IsWindowsPlatform ? "-NoProfile " : "")}{(isScript ? "" : "-c ")}",
            CommandPromptShell => isScript ? "" : "/c ",
            _ => string.Empty
        };
    }
}