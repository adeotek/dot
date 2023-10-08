using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Exceptions;
using Adeotek.Extensions.Processes;

using NSubstitute;

namespace Adeotek.Extensions.Docker.Tests;

public class DockerManagerTests
{
    private const string CliCommand = "docker";
    private const string DockerGenericError =
        "error during connect: this error may indicate that the docker daemon is not running...";
    
    [Fact]
    public void ContainerExists_WithExisting_ReturnsTrue()
    {
        const string containerName = "test-container-mock";
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
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { $"/{containerName}" });

        var result = sut.ContainerExists(containerName);
        
        Assert.True(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"container inspect --format \"{{{{lower .Name}}}}\" {containerName}", args);
    }
    
    [Fact]
    public void ContainerExists_WithMissing_ReturnsFalse()
    {
        const string containerName = "test-container-mock";
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

        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            "",
            $"Error response from daemon: No such container: {containerName}"
        });
        
        var result = sut.ContainerExists(containerName);
        
        Assert.False(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"container inspect --format \"{{{{lower .Name}}}}\" {containerName}", args);
    }
    
    [Fact]
    public void ContainerExists_WithUnknownError_ThrowsException()
    {
        const string containerName = "test-container-mock";
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
        
        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });

        var action = () => { sut.ContainerExists(containerName); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"container inspect --format \"{{{{lower .Name}}}}\" {containerName}", args);
    }

    [Fact]
    public void CreateContainer_WithMissing_ReturnsTrue()
    {
        var config = DockerConfigManager.GetSampleConfig();
        var expectedArgs = GetCreateContainerArgs(config);
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

        ShellProcessMockSendStdOutput(shellProcessMock, new[] { "newly_created_container_id" });
        
        var result = sut.CreateContainer(config);
        
        Assert.True(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void CreateContainer_WithExisting_ReturnsFalse()
    {
        var config = DockerConfigManager.GetSampleConfig();
        var expectedArgs = GetCreateContainerArgs(config);
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
    
        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"docker: Error response from daemon: Conflict. The container name \"/{config.PrimaryName}\" is already in use by container"
        });
        
        var result = sut.CreateContainer(config);
        
        Assert.False(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void CreateContainer_WithUnknownError_ThrowsException()
    {
        var config = DockerConfigManager.GetSampleConfig();
        var expectedArgs = GetCreateContainerArgs(config);
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
    
        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });
        
        var action = () => { sut.CreateContainer(config); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }

    [Fact]
    public void StartContainer_WithExisting_ReturnTrue()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        string? cmd = null;
        string? args = null;
        
        sut.OnDockerCliEvent += (_, e) =>
        {
            if (e is { Type: DockerCliEventType.ExitCode, Data.Count: 3 })
            {
                cmd = e.Data.GetValueOrDefault("cmd");
                args = e.Data.GetValueOrDefault("args");
            }
        };

        ShellProcessMockSendStdOutput(shellProcessMock, new[] { containerName });
        
        var result = sut.StartContainer(containerName);

        Assert.True(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"start {containerName}", args);
    }
    
    [Fact]
    public void StartContainer_WithMissing_ReturnsFalse()
    {
        const string containerName = "test-container-mock";
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

        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"Error response from daemon: No such container: {containerName}"
        });
        
        var result = sut.StartContainer(containerName);
        
        Assert.False(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"start {containerName}", args);
        Assert.Equal($"Container '{containerName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void StartContainer_WithUnknownError_ThrowsException()
    {
        const string containerName = "test-container-mock";
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

        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });
        
        var action = () => { sut.StartContainer(containerName); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"start {containerName}", args);
    }
    
    [Fact]
    public void StopContainer_WithExisting_ExpectSuccess()
    {
        const string containerName = "test-container-mock";
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

        ShellProcessMockSendStdOutput(shellProcessMock, new[] { containerName });
        
        sut.StopContainer(containerName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"stop {containerName}", args);
        Assert.Equal("0", exitCode);
    }
    
    [Fact]
    public void StopContainer_WithMissing_LogsWarning()
    {
        const string containerName = "test-container-mock";
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

        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"Error response from daemon: No such container: {containerName}"
        });
        
        sut.StopContainer(containerName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"stop {containerName}", args);
        Assert.Equal($"Container '{containerName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void StopContainer_WithUnknownError_ThrowsException()
    {
        const string containerName = "test-container-mock";
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

        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });
        
        var action = () => sut.StopContainer(containerName);
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"stop {containerName}", args);
    }
    
    [Fact]
    public void RemoveContainer_WithExisting_ExpectSuccess()
    {
        const string containerName = "test-container-mock";
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

        ShellProcessMockSendStdOutput(shellProcessMock, new[] { containerName });
        
        sut.RemoveContainer(containerName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rm {containerName}", args);
        Assert.Equal("0", exitCode);
    }
    
    [Fact]
    public void RemoveContainer_WithMissing_LogsWarning()
    {
        const string containerName = "test-container-mock";
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

        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"Error response from daemon: No such container: {containerName}"
        });
        
        sut.RemoveContainer(containerName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rm {containerName}", args);
        Assert.Equal($"Container '{containerName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void RemoveContainer_WithUnknownError_ThrowsException()
    {
        const string containerName = "test-container-mock";
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
        
        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });
        
        var action = () => sut.RemoveContainer(containerName);
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rm {containerName}", args);
    }
    
    [Fact]
    public void RenameContainer_WithExisting_ExpectSuccess()
    {
        const string containerName = "test-container-mock";
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
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { containerName });
        
        sut.RenameContainer(containerName, containerNewName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rename {containerName} {containerNewName}", args);
        Assert.Equal("0", exitCode);
    }
    
    [Fact]
    public void RenameContainer_WithMissing_LogsWarning()
    {
        const string containerName = "test-container-mock";
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

        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"Error response from daemon: No such container: {containerName}"
        });
        
        sut.RenameContainer(containerName, containerNewName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rename {containerName} {containerNewName}", args);
        Assert.Equal($"Container '{containerName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void RenameContainer_WithUnknownError_ThrowsException()
    {
        const string containerName = "test-container-mock";
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

        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });
        
        var action = () => sut.RenameContainer(containerName, containerNewName);
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rename {containerName} {containerNewName}", args);
    }
    
    [Fact]
    public void VolumeExists_WithExisting_ReturnsTrue()
    {
        const string volumeName = "test-docker-volume-mock";
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
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[]
        {
            "DRIVER VOLUME NAME",
            $"local {volumeName}"
        });

        var result = sut.VolumeExists(volumeName);
        
        Assert.True(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume ls --filter name={volumeName}", args);
    }
    
    [Fact]
    public void VolumeExists_WithMissing_ReturnsFalse()
    {
        const string volumeName = "test-docker-volume-mock";
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

        ShellProcessMockSendStdOutput(shellProcessMock, new[]
        {
            "DRIVER VOLUME NAME"
        });
        
        var result = sut.VolumeExists(volumeName);
        
        Assert.False(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume ls --filter name={volumeName}", args);
    }
    
    [Fact]
    public void CreateVolume_WithExistingOrMissing_ExpectSuccess()
    {
        const string volumeName = "test-docker-volume-mock";
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

        ShellProcessMockSendStdOutput(shellProcessMock, new[] { volumeName });
        
        sut.CreateVolume(volumeName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume create {volumeName}", args);
    }
    
    [Fact]
    public void CreateVolume_WithUnknownError_ThrowsException()
    {
        const string volumeName = "test-docker-volume-mock";
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
    
        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });
        
        var action = () => sut.CreateVolume(volumeName);
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume create {volumeName}", args);
    }
    
    [Fact]
    public void RemoveVolume_WithExisting_ExpectSuccess()
    {
        const string volumeName = "test-docker-volume-mock";
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

        ShellProcessMockSendStdOutput(shellProcessMock, new[] { volumeName });
        
        sut.RemoveVolume(volumeName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume rm {volumeName}", args);
        Assert.Equal("0", exitCode);
    }
    
    [Fact]
    public void RemoveVolume_WithMissing_LogsWarning()
    {
        const string volumeName = "test-docker-volume-mock";
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

        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"Error response from daemon: get {volumeName}: no such volume"
        });
        
        sut.RemoveVolume(volumeName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume rm {volumeName}", args);
        Assert.Equal($"Volume '{volumeName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void RemoveVolume_WithUnknownError_ThrowsException()
    {
        const string volumeName = "test-docker-volume-mock";
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
        
        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });
        
        var action = () => sut.RemoveVolume(volumeName);
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume rm {volumeName}", args);
    }
    
    [Fact]
    public void NetworkExists_WithExisting_ReturnsTrue()
    {
        const string networkName = "test-docker-network-mock";
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
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[]
        {
            "NETWORK ID NAME DRIVER SCOPE",
            $"a1b2c3d4 {networkName} bridge local"
        });

        var result = sut.NetworkExists(networkName);
        
        Assert.True(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"network ls --filter name={networkName}", args);
    }
    
    [Fact]
    public void NetworkExists_WithMissing_ReturnsFalse()
    {
        const string networkName = "test-docker-network-mock";
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

        ShellProcessMockSendStdOutput(shellProcessMock, new[]
        {
            "NETWORK ID NAME DRIVER SCOPE"
        });
        
        var result = sut.NetworkExists(networkName);
        
        Assert.False(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"network ls --filter name={networkName}", args);
    }
    
    [Fact]
    public void CreateNetwork_WithMissing_ExpectSuccess()
    {
        var network = DockerConfigManager.GetSampleConfig().Network 
                      ?? throw new NullReferenceException("NetworkConfig");
        var expectedArgs = GetNetworkCreateArgs(network);
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

        ShellProcessMockSendStdOutput(shellProcessMock, new[]
        {
            "newly_created_docker_network_id"
        });
        
        sut.CreateNetwork(network);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void CreateNetwork_WithExisting_ExpectSuccess()
    {
        var network = DockerConfigManager.GetSampleConfig().Network 
                      ?? throw new NullReferenceException("NetworkConfig");
        var expectedArgs = GetNetworkCreateArgs(network);
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

        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"Error response from daemon: network with name {network.Name} already exists"
        });
        
        sut.CreateNetwork(network);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void CreateNetwork_WithUnknownError_ThrowsException()
    {
        var network = DockerConfigManager.GetSampleConfig().Network 
                      ?? throw new NullReferenceException("NetworkConfig");
        var expectedArgs = GetNetworkCreateArgs(network);
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
    
        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });
        
        var action = () => sut.CreateNetwork(network);
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void RemoveNetwork_WithExisting_ExpectSuccess()
    {
        const string networkName = "test-docker-network-mock";
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

        ShellProcessMockSendStdOutput(shellProcessMock, new[] { networkName });
        
        sut.RemoveNetwork(networkName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"network rm {networkName}", args);
        Assert.Equal("0", exitCode);
    }
    
    [Fact]
    public void RemoveNetwork_WithMissing_LogsWarning()
    {
        const string networkName = "test-docker-network-mock";
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

        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"Error response from daemon: network {networkName} not found"
        });
        
        sut.RemoveNetwork(networkName);
        
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"network rm {networkName}", args);
        Assert.Equal($"Network '{networkName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void RemoveNetwork_WithUnknownError_ThrowsException()
    {
        const string networkName = "test-docker-network-mock";
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
        
        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });
        
        var action = () => sut.RemoveNetwork(networkName);
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"network rm {networkName}", args);
    }
    
    [Fact]
    public void PullImage_WithMissing_ReturnsImageId()
    {
        const string imageName = "test-image-mock:latest";
        const string imageTag = "latest";
        var sut = GetDockerManager(out var shellProcessMock);
        var expectedResult = "sha256:some-docker-image-id-hash";
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
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[]
        {
            $"latest: Pulling from library/{imageName}",
            "0a123b00: Already exists",
            "a123b001: Pull complete",
            "b123b001: Pull complete",
            "c123b001: Pull complete",
            "d123b001: Pull complete",
            $"Digest: {expectedResult}",
            $"Status: Downloaded newer image for {imageName}:{imageTag}",
            $"docker.io/library/{imageName}:{imageTag}",
            ""
        });

        var result = sut.PullImage(imageName, imageTag);
        
        Assert.Equal(expectedResult, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"pull {imageName}:{imageTag}", args);
    }
    
    [Fact]
    public void PullImage_WithExisting_ReturnsImageId()
    {
        const string imageName = "test-image-mock:latest";
        const string imageTag = "latest";
        var sut = GetDockerManager(out var shellProcessMock);
        var expectedResult = "sha256:some-docker-image-id-hash";
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
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[]
        {
            $"latest: Pulling from library/{imageName}",
            $"Digest: {expectedResult}",
            $"Status: Image is up to date for {imageName}:{imageTag}",
            $"docker.io/library/{imageName}:{imageTag}",
            ""
        });

        var result = sut.PullImage(imageName, imageTag);
        
        Assert.Equal(expectedResult, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"pull {imageName}:{imageTag}", args);
    }
    
    [Fact]
    public void PullImage_WithMissing_ThrowsException()
    {
        const string imageName = "test-image-mock:latest";
        const string imageTag = "latest";
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
        
        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });

        var action = () => { sut.PullImage(imageName, imageTag); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"pull {imageName}:{imageTag}", args);
    }
    
    [Fact]
    public void GetContainerImageId_WithExisting_ReturnsId()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerManager(out var shellProcessMock);
        var expectedResult = "sha256:some-docker-container-image-id-hash";
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
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { expectedResult });

        var result = sut.GetContainerImageId(containerName);
        
        Assert.Equal(expectedResult, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"container inspect --format \"{{{{lower .Image}}}}\" {containerName}", args);
    }
    
    [Fact]
    public void GetContainerImageId_WithMissing_ThrowsException()
    {
        const string containerName = "test-container-mock";
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
        
        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"Error response from daemon: No such container: {containerName}"
        });

        var action = () => { sut.GetContainerImageId(containerName); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"container inspect --format \"{{{{lower .Image}}}}\" {containerName}", args);
    }
    
    [Fact]
    public void GetContainerImageId_WithUnknownError_ThrowsException()
    {
        const string containerName = "test-container-mock";
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
        
        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });

        var action = () => { sut.GetContainerImageId(containerName); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"container inspect --format \"{{{{lower .Image}}}}\" {containerName}", args);
    }
    
    [Fact]
    public void GetImageId_WithExisting_ReturnsId()
    {
        const string imageName = "test-image-mock:latest";
        const string imageTag = "latest";
        var sut = GetDockerManager(out var shellProcessMock);
        var expectedResult = "sha256:some-docker-image-id-hash";
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
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { expectedResult });

        var result = sut.GetImageId(imageName, imageTag);
        
        Assert.Equal(expectedResult, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"image inspect --format \"{{{{lower .Id}}}}\" {imageName}:{imageTag}", args);
    }
    
    [Fact]
    public void GetImageId_WithMissing_ThrowsException()
    {
        const string imageName = "test-image-mock";
        const string imageTag = "latest";
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
        
        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"Error response from daemon: No such image: {imageName}:{imageTag}"
        });

        var action = () => { sut.GetImageId(imageName, imageTag); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"image inspect --format \"{{{{lower .Id}}}}\" {imageName}:{imageTag}", args);
    }
    
    [Fact]
    public void GetImageId_WithUnknownError_ThrowsException()
    {
        const string imageName = "test-image-mock:latest";
        const string imageTag = "latest";
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
        
        ShellProcessMockSendErrOutput(shellProcessMock, new[] { DockerGenericError });

        var action = () => { sut.GetImageId(imageName, imageTag); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"image inspect --format \"{{{{lower .Id}}}}\" {imageName}:{imageTag}", args);
    }
    
    private static void ShellProcessMockSendStdOutput(IShellProcess shellProcessMock, IEnumerable<string> messages)
    {
        shellProcessMock
            .StartAndWaitForExit()
            .Returns(0)
            .AndDoes(_ =>
            {
                foreach (var message in messages)
                {
                    shellProcessMock.StdOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(shellProcessMock, 
                        new OutputReceivedEventArgs(message));    
                }
            });
    }

    private static void ShellProcessMockSendErrOutput(IShellProcess shellProcessMock, IEnumerable<string> errors)
    {
        shellProcessMock
            .StartAndWaitForExit()
            .Returns(1)
            .AndDoes(_ =>
            {
                foreach (var error in errors)
                {
                    shellProcessMock.ErrOutputDataReceived += Raise.Event<OutputReceivedEventHandler>(shellProcessMock, 
                        new OutputReceivedEventArgs(error, true));    
                }
            });
    }
    
    private static DockerManager GetDockerManager(out IShellProcess shellProcessMock)
    {
        var provider = TestHelpers.GetShellProcessProvider(out shellProcessMock);
        return new DockerManager(new DockerCliCommand(provider) { Command = CliCommand });
    }

    private static string GetCreateContainerArgs(ContainerConfig config) =>
        "run -d " +
        $"--name={config.NamePrefix}{config.BaseName}{config.PrimarySuffix} " +
        $"-p {config.Ports[0].Host}:{config.Ports[0].Container} " +
        $"-p {config.Ports[1].Host}:{config.Ports[1].Container} " +
        $"-v {config.Volumes[0].Source}:{config.Volumes[0].Destination} " +
        $"-v {config.Volumes[1].Source}:{config.Volumes[1].Destination} " +
        $"-e {config.EnvVars.First().Key}={config.EnvVars.First().Value} " +
        $"-e {config.EnvVars.Skip(1).First().Key}={config.EnvVars.Skip(1).First().Value} " +
        $"--network={config.Network?.Name} --ip={config.Network?.IpAddress} " +
        $"--hostname={config.Network?.Hostname} " +
        $"--network-alias={config.Network?.Alias} " +
        $"--restart={config.Restart} " +
        $"{config.Image}:{config.Tag}";

    private static string GetNetworkCreateArgs(NetworkConfig network) =>
        "network create -d bridge --attachable " +
        $"--subnet {network.Subnet} " +
        $"--ip-range {network.IpRange} " +
        $"{network.Name}";
}