using Adeotek.Extensions.Docker.Exceptions;
using Adeotek.Extensions.Processes;

using NSubstitute;

namespace Adeotek.Extensions.Docker.Tests;

public class DockerManagerTests
{
    private const string CliCommand = "docker";
    
    [Fact]
    public void ContainerExists_WithExisting_ReturnTrue()
    {
        var containerName = "test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.Command, Data.Count: 2 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(0)
            .AndDoes(_ =>
            {
                shellProcessMock.StdOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs("CONTAINER ID IMAGE COMMAND CREATED STATUS PORTS NAMES"));
                shellProcessMock.StdOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs($"12345abcde some-docker-image:latest \"/entrypoint.sh\" x days ago Up x hours {containerName}"));
            });
        
        var result = sut.ContainerExists(containerName);
        
        Assert.True(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"container ls --all --filter name={containerName}", args);
    }
    
    [Fact]
    public void ContainerExists_WithMissing_ReturnFalse()
    {
        var containerName = "test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.Command, Data.Count: 2 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(0)
            .AndDoes(_ =>
            {
                shellProcessMock.StdOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs("CONTAINER ID IMAGE COMMAND CREATED STATUS PORTS NAMES"));
                shellProcessMock.StdOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs("12345abcde some-docker-image:latest \"/entrypoint.sh\" x days ago Up x hours other-container-mock"));
            });
        
        var result = sut.ContainerExists(containerName);
        
        Assert.False(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"container ls --all --filter name={containerName}", args);
    }
    
    [Fact]
    public void ContainerExists_WithNoContainers_ReturnFalse()
    {
        var containerName = "test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.Command, Data.Count: 2 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(0)
            .AndDoes(_ =>
            {
                shellProcessMock.StdOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs("CONTAINER ID IMAGE COMMAND CREATED STATUS PORTS NAMES"));
            });
        
        var result = sut.ContainerExists(containerName);
        
        Assert.False(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"container ls --all --filter name={containerName}", args);
    }
    
    // TODO: CreateContainer

    [Fact]
    public void StopContainer_WithExisting_ExpectSuccess()
    {
        var containerName = "test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        string? exitCode = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.ExitCode, Data.Count: 3 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
                exitCode = e.Data.GetValueOrDefault("exit");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(0)
            .AndDoes(_ =>
            {
                shellProcessMock.StdOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs(containerName));
            });
        
        sut.StopContainer(containerName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"stop {containerName}", args);
        Assert.Equal("0", exitCode);
    }
    
    [Fact]
    public void StopContainer_WithMissing_LogWarning()
    {
        var containerName = "test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        string? message = null;
        string? level = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.Command, Data.Count: 2 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
            }
            else if (e is { Type: DockerCliEventType.Message, Data.Count: 2 })
            {
                message = e.Data.GetValueOrDefault("message");
                level = e.Data.GetValueOrDefault("level");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(1)
            .AndDoes(_ =>
            {
                shellProcessMock.ErrOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs($"Error response from daemon: No such container: {containerName}", true));
            });
        
        sut.StopContainer(containerName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"stop {containerName}", args);
        Assert.Equal($"Container '{containerName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void StopContainer_WithUnknownError_ThrowException()
    {
        var containerName = "test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.Command, Data.Count: 2 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(1)
            .AndDoes(_ =>
            {
                shellProcessMock.ErrOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs("Error response from daemon: some unknown error!", true));
            });
        
        var action = () => sut.StopContainer(containerName);
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"stop {containerName}", args);
    }
    
    
    [Fact]
    public void RemoveContainer_WithExisting_ExpectSuccess()
    {
        var containerName = "test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        string? exitCode = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.ExitCode, Data.Count: 3 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
                exitCode = e.Data.GetValueOrDefault("exit");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(0)
            .AndDoes(_ =>
            {
                shellProcessMock.StdOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs(containerName));
            });
        
        sut.RemoveContainer(containerName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rm {containerName}", args);
        Assert.Equal("0", exitCode);
    }
    
    [Fact]
    public void RemoveContainer_WithMissing_LogWarning()
    {
        var containerName = "test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        string? message = null;
        string? level = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.Command, Data.Count: 2 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
            }
            else if (e is { Type: DockerCliEventType.Message, Data.Count: 2 })
            {
                message = e.Data.GetValueOrDefault("message");
                level = e.Data.GetValueOrDefault("level");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(1)
            .AndDoes(_ =>
            {
                shellProcessMock.ErrOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs($"Error response from daemon: No such container: {containerName}", true));
            });
        
        sut.RemoveContainer(containerName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rm {containerName}", args);
        Assert.Equal($"Container '{containerName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void RemoveContainer_WithUnknownError_ThrowException()
    {
        var containerName = "test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.Command, Data.Count: 2 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(1)
            .AndDoes(_ =>
            {
                shellProcessMock.ErrOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs("Error response from daemon: some unknown error!", true));
            });
        
        var action = () => sut.RemoveContainer(containerName);
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rm {containerName}", args);
    }
    
    [Fact]
    public void RenameContainer_WithExisting_ExpectSuccess()
    {
        var containerName = "test-container-mock";
        var containerNewName = "new-test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        string? exitCode = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.ExitCode, Data.Count: 3 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
                exitCode = e.Data.GetValueOrDefault("exit");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(0)
            .AndDoes(_ =>
            {
                shellProcessMock.StdOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs(containerName));
            });
        
        sut.RenameContainer(containerName, containerNewName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rename {containerName} {containerNewName}", args);
        Assert.Equal("0", exitCode);
    }
    
    [Fact]
    public void RenameContainer_WithMissing_LogWarning()
    {
        var containerName = "test-container-mock";
        var containerNewName = "new-test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        string? message = null;
        string? level = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.Command, Data.Count: 2 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
            }
            else if (e is { Type: DockerCliEventType.Message, Data.Count: 2 })
            {
                message = e.Data.GetValueOrDefault("message");
                level = e.Data.GetValueOrDefault("level");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(1)
            .AndDoes(_ =>
            {
                shellProcessMock.ErrOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs($"Error response from daemon: No such container: {containerName}", true));
            });
        
        sut.RenameContainer(containerName, containerNewName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rename {containerName} {containerNewName}", args);
        Assert.Equal($"Container '{containerName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void RenameContainer_WithUnknownError_ThrowException()
    {
        var containerName = "test-container-mock";
        var containerNewName = "new-test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.Command, Data.Count: 2 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
            }
        };

        shellProcessMock
            .StartAndWaitForExit()
            .Returns(1)
            .AndDoes(_ =>
            {
                shellProcessMock.ErrOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(this, 
                    new OutputReceivedEventArgs("Error response from daemon: some unknown error!", true));
            });
        
        var action = () => sut.RenameContainer(containerName, containerNewName);
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rename {containerName} {containerNewName}", args);
    }
    
    private DockerManager GetDockerManager(out IShellProcess shellProcessMock)
    {
        var provider = TestHelpers.GetShellProcessProvider(out shellProcessMock);
        return new DockerManager(new DockerCliCommand(provider) { Command = CliCommand });
    }
}