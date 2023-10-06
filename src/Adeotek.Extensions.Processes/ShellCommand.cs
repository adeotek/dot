using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Adeotek.Extensions.Processes;

public class ShellCommand
{
    public static ShellCommand GetShellCommandInstance(
        string? shell = null, 
        string? command = null, 
        bool isScript = false,
        OutputReceivedEventHandler? onStdOutput = null,
        OutputReceivedEventHandler? onErrOutput = null)
    {
        var instance = new ShellCommand(new DefaultShellProcessProvider())
        {
            Shell = shell ?? NoShell, Command = command ?? "", IsScript = isScript
        };
        if (onStdOutput is not null)
        {
            instance.OnStdOutput += onStdOutput;
        }
        if (onErrOutput is not null)
        {
            instance.OnErrOutput += onErrOutput;
        }
        return instance;
    }

    public const string NoShell = "";
    public const string BashShell = "/bin/bash";
    public const string ShShell = "/bin/sh";
    public const string PsShell = "pwsh";
    public const string PowerShell = "powershell";
    public const string CommandPromptShell = "cmd";

    public static readonly bool IsWindowsPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    
    public delegate void OutputReceivedEventHandler(object sender, OutputReceivedEventArgs e);
    public event OutputReceivedEventHandler? OnStdOutput;
    public event OutputReceivedEventHandler? OnErrOutput;
    
    protected bool _prepared;
    protected string _shell = NoShell;
    protected string _command = "";
    protected List<string> _arguments = new();
    protected bool _isScript;
    protected readonly IShellProcessProvider _shellProcessProvider;

    public ShellCommand(IShellProcessProvider shellProcessProvider)
    {
        _shellProcessProvider = shellProcessProvider;
    }

    public string Shell
    {
        get => _shell;
        set
        {
            if (_shell != value) _prepared = false;
            _shell = value;
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
    public string[] Args => _arguments.ToArray();
    public string ProcessFile { get; private set; } = "";
    public string ProcessArguments { get; private set; } = "";
    public int ExitCode { get; private set; } = -1;
    public List<string> StdOutput { get; } = new();
    public List<string> ErrOutput { get; } = new();
    
    public bool IsSuccess(string? message = null, bool checkFirstLineOnly = false)
    {
        if (string.IsNullOrEmpty(message))
        {
            return ExitCode == 0;
        }

        if (ExitCode != 0)
        {
            return false;
        }
        
        return checkFirstLineOnly
            ? (StdOutput.FirstOrDefault()?.Contains(message) ?? false)
            : StdOutput.Exists(e => e.Contains(message));
    }
    
    public bool IsError(string? message = null, bool checkFirstLineOnly = false)
    {
        if (string.IsNullOrEmpty(message))
        {
            return ExitCode != 0;
        }

        if (ExitCode == 0)
        {
            return false;
        }
        
        return checkFirstLineOnly
            ? (ErrOutput.FirstOrDefault()?.Contains(message) ?? false)
            : ErrOutput.Exists(e => e.Contains(message));
    }

    public ShellCommand AddArg(string value)
    {
        _prepared = false;
        _arguments.Add(value);
        return this;
    }
    
    public ShellCommand AddArg(IEnumerable<string> range)
    {
        _prepared = false;
        _arguments.AddRange(range);
        return this;
    }

    public ShellCommand SetArgAt(int index, string value)
    {
        _prepared = false;
        _arguments[index] = value;
        return this;
    }
    
    public ShellCommand ReplaceArg(string currentValue, string newValue)
    {
        _prepared = false;
        _arguments[_arguments.IndexOf(currentValue)] = newValue;
        return this;
    }
    
    public ShellCommand RemoveArg(string item)
    {
        _prepared = false;
        _arguments.Remove(item);
        return this;
    }
    
    public ShellCommand RemoveArgAt(int index)
    {
        _prepared = false;
        _arguments.RemoveAt(index);
        return this;
    }
    
    public ShellCommand ClearArgs()
    {
        _prepared = false;
        _arguments.Clear();
        return this;
    }

    public ShellCommand ClearArgsAndReset()
    {
        Reset();
        return ClearArgs();
    }
    
    public ShellCommand Prepare()
    {
        if (_prepared)
        {
            return this;
        }
        
        ProcessFile = string.IsNullOrWhiteSpace(Shell) ? Command : Shell;
        ProcessArguments = GetShellArguments(Command, Args.ToArray(), Shell, IsScript);
        return this;
    }

    public int Execute(string command, string[]? args = null, string? shellName = null, bool isScript = false,
        bool isElevated = false)
    {
        Command = command;
        _arguments = args?.ToList() ?? new List<string>();
        Shell = shellName ?? "";
        IsScript = isScript;
        IsElevated = isElevated;

        return Execute();
    }

    public int Execute()
    {
        Reset();
        Prepare();
        
        using IShellProcess process = _shellProcessProvider.GetShellProcess(
            ProcessFile,
            ProcessArguments,
            ProcessStdOutput, 
            ProcessErrOutput);
        
        try
        {
            ExitCode = process.StartAndWaitForExit();
            
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
            ExitCode = -1;
        }

        return ExitCode;
    }

    protected virtual void ProcessStdOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is null)
        {
            return;
        }
        OnStdOutput?.Invoke(this, new OutputReceivedEventArgs(e.Data));
        StdOutput.Add(e.Data);
    }

    protected virtual void ProcessErrOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is null)
        {
            return;
        }
        OnErrOutput?.Invoke(this, new OutputReceivedEventArgs(e.Data, true));
        ErrOutput.Add(e.Data);
    }

    protected virtual void Reset()
    {
        ExitCode = -1;
        StdOutput.Clear();
        ErrOutput.Clear();
    }

    protected static string GetShellArguments(string command, string[] args, string shellName, bool isScript = false)
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

    protected static string GetArgsPrepend(string shellName, bool isScript = false) =>
        shellName switch
        {
            BashShell or ShShell or PowerShell => isScript ? "" : "-c ",
            PsShell => $"{(IsWindowsPlatform ? "-NoProfile " : "")}{(isScript ? "" : "-c ")}",
            CommandPromptShell => isScript ? "" : "/c ",
            _ => string.Empty
        };
}