using System.Diagnostics;

namespace Adeotek.Extensions.Processes;

public interface IShellProcessProvider
{
    IShellProcess GetShellProcess();
    
    IShellProcess GetShellProcess(
        string fileName, 
        string? arguments = null,
        DataReceivedEventHandler? outputDataReceived = null,
        DataReceivedEventHandler? errorDataReceived = null,
        EventHandler? exited = null);
}