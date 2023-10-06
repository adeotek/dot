using System.Diagnostics;

using Adeotek.Extensions.Processes;

using NSubstitute;

namespace Adeotek.Extensions.Docker.Tests;

public class ShellProcessMockProvider : IShellProcessProvider
{
    public IShellProcess GetShellProcess()
    {
        return Substitute.For<IShellProcess>();
    }

    public IShellProcess GetShellProcess(string fileName, string? arguments = null,
        DataReceivedEventHandler? outputDataReceived = null, DataReceivedEventHandler? errorDataReceived = null,
        EventHandler? exited = null)
    {
        return Substitute.For<IShellProcess>();
    }
}