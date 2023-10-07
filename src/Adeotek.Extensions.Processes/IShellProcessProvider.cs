namespace Adeotek.Extensions.Processes;

public interface IShellProcessProvider
{
    IShellProcess GetShellProcess();
    
    IShellProcess GetShellProcess(
        string fileName, 
        string? arguments = null,
        OutputReceivedEventHandler? outputDataReceived = null,
        OutputReceivedEventHandler? errorDataReceived = null,
        EventHandler? exited = null);
}