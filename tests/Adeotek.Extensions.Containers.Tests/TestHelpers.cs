using Adeotek.Extensions.Processes;

using NSubstitute;

namespace Adeotek.Extensions.Containers.Tests;

public static class TestHelpers
{
    public static IShellProcessProvider GetShellProcessProvider(out IShellProcess process)
    {
        var processMock = Substitute.For<IShellProcess>();
        
        var provider = Substitute.For<IShellProcessProvider>();
        provider.GetShellProcess().Returns(processMock);
        provider.GetShellProcess(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<OutputReceivedEventHandler?>(),
                Arg.Any<OutputReceivedEventHandler?>())
            .Returns(x =>
            {
                processMock.StdOutputDataReceived += (OutputReceivedEventHandler?) x.Args()[2];
                processMock.ErrOutputDataReceived += (OutputReceivedEventHandler?) x.Args()[3];
                return processMock;
            });
        
        process = processMock;
        return provider;
    }
}