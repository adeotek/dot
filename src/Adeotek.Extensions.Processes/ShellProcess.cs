﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace Adeotek.Extensions.Processes;

public class ShellProcess : Process, IShellProcess
{
    public string FileName { get; set; } = default!;
    public string WorkingDirectory { get; set; } = "";
    public string? Arguments { get; set; }
    public Collection<string> ArgumentList { get; set; } = new();
    public Dictionary<string, string?> Environment { get; set; } = new();
    public bool RedirectStandardOutput { get; set; } = true;
    public bool RedirectStandardError { get; set; } = true;
    public bool IsElevated { get; set; }
    public Encoding? StandardInputEncoding { get; set; } = Encoding.UTF8;
    public Encoding? StandardOutputEncoding { get; set; } = Encoding.UTF8;
    public Encoding? StandardErrorEncoding { get; set; } = Encoding.UTF8;
    
    public event OutputReceivedEventHandler? StdOutputDataReceived;
    public event OutputReceivedEventHandler? ErrOutputDataReceived;

    public int StartAndWaitForExit()
    {
        StartInfo = GetProcessInfo();

        if (RedirectStandardOutput)
        {
            OutputDataReceived += ProcessOutputDataReceived;
        }
        
        if (RedirectStandardError)
        {
            ErrorDataReceived += ProcessErrorDataReceived;
        }
        
        Start();
        if (RedirectStandardOutput)
        {
            BeginOutputReadLine();
        }
        if (RedirectStandardError)
        {
            BeginErrorReadLine();
        }
        WaitForExit();
        
        return ExitCode;
    }
    
    public ProcessStartInfo GetProcessInfo()
    {
        ProcessStartInfo processStartInfo = new()
        {
            FileName = FileName,
            WorkingDirectory = WorkingDirectory,
            Arguments = Arguments,
            RedirectStandardOutput = RedirectStandardOutput,
            RedirectStandardError = RedirectStandardError,
            UseShellExecute = false, // false is required for output redirection
            CreateNoWindow = true,
            ErrorDialog = false,
            StandardInputEncoding = null,
            StandardOutputEncoding = RedirectStandardOutput ? StandardOutputEncoding : null,
            StandardErrorEncoding = RedirectStandardError ? StandardErrorEncoding : null
        };

        foreach (var arg in ArgumentList)
        {
            processStartInfo.ArgumentList.Add(arg);
        }
        
        foreach (var env in Environment)
        {
            processStartInfo.Environment.Add(env);
        }

        if (IsElevated)
        {
            processStartInfo.Verb = "runas";
        }

        return processStartInfo;
    }
    
    private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e) => 
        StdOutputDataReceived?.Invoke(this, new OutputReceivedEventArgs(e.Data));

    private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e) => 
        ErrOutputDataReceived?.Invoke(this, new OutputReceivedEventArgs(e.Data, true));
}