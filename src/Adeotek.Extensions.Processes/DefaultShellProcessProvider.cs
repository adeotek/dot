namespace Adeotek.Extensions.Processes;

public class DefaultShellProcessProvider : IShellProcessProvider
{
    public IShellProcess GetShellProcess() => new ShellProcess();

    public IShellProcess GetShellProcess(
        string fileName, 
        string? arguments = null,
        OutputReceivedEventHandler? outputDataReceived = null,
        OutputReceivedEventHandler? errorDataReceived = null,
        EventHandler? exited = null)
    {
        var process = new ShellProcess
        {
            FileName = fileName,
            Arguments = arguments
        };

        if (outputDataReceived is not null)
        {
            process.StdOutputDataReceived += outputDataReceived;
        }
        
        if (errorDataReceived is not null)
        {
            process.ErrOutputDataReceived += errorDataReceived;
        }
        
        if (exited is not null)
        {
            process.Exited += exited;
        }

        return process;
    }
}