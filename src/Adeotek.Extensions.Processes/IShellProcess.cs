using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace Adeotek.Extensions.Processes;

public interface IShellProcess : IDisposable
{
    // ProcessStartInfo
    string FileName { get; set; }
    string WorkingDirectory { get; set; }
    string? Arguments { get; set; }
    Collection<string>? ArgumentList { get; set; }
    Dictionary<string,string?>? Environment { get; set; }
    bool RedirectStandardOutput { get; set; }
    bool RedirectStandardError { get; set; }
    bool IsElevated { get; set; }
    Encoding? StandardInputEncoding { get; set; }
    Encoding? StandardOutputEncoding { get; set; }
    Encoding? StandardErrorEncoding { get; set; }
    
    // Process
    int Id { get; }
    string ProcessName { get; }
    bool HasExited { get; }
    int ExitCode { get; }
    DateTime StartTime { get; }
    DateTime ExitTime { get; }
    StreamWriter StandardInput { get; }
    StreamReader StandardError { get; }
    StreamReader StandardOutput { get; }
    
    event DataReceivedEventHandler? OutputDataReceived;
    event DataReceivedEventHandler? ErrorDataReceived;
    event EventHandler Exited;
    
    bool Start();
    void Refresh();
    void BeginOutputReadLine();
    void BeginErrorReadLine();
    void CancelOutputRead();
    void CancelErrorRead();
    void WaitForExit();
    bool WaitForExit(TimeSpan timeout);
    Task WaitForExitAsync (CancellationToken cancellationToken = default);
    bool WaitForInputIdle();
    void Close();
    void Kill (bool entireProcessTree);
    
    // Custom
    ProcessStartInfo GetProcessInfo();
    int StartAndWaitForExit();
}