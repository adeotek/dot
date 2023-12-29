using System.Reflection;
using System.Text;

using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Exceptions;
using Adeotek.Extensions.Processes;

using NSubstitute;

namespace Adeotek.Extensions.Docker.Tests;

public class DockerCliTests
{
    private const string CliCommand = "docker";
    private const string DockerGenericError =
        "error during connect: this error may indicate that the docker daemon is not running...";
    
    [Fact]
    public void ContainerExists_WithExisting_ReturnsTrue()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        var sut = GetDockerCli(out var shellProcessMock);
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
        var sut = GetDockerCli(out var shellProcessMock);
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

    [Theory]
    [InlineData(true, "first-service")]
    [InlineData(true, "second-service")]
    [InlineData(false, "second-service")]
    public void CreateContainer_WithMissing_ReturnsOne(bool autoStart, string serviceName)
    {
        var config = DockerConfigManager.GetSampleConfig();
        var serviceConfig = config.Services[serviceName];
        var networks = config.Networks.ToNetworksEnumerable().ToList();
        var expectedArgs = GetCreateContainerArgs(serviceConfig, networks, autoStart);
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.CreateContainer(serviceConfig, networks, autoStart);
        
        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Theory]
    [InlineData(true, "first-service")]
    [InlineData(true, "second-service")]
    [InlineData(false, "second-service")]
    public void CreateContainer_WithMissingAndNoStartupCommand_ReturnsOne(bool autoStart, string serviceName)
    {
        var config = DockerConfigManager.GetSampleConfig();
        var serviceConfig = config.Services[serviceName];
        var networks = config.Networks.ToNetworksEnumerable().ToList();
        serviceConfig.Command = null;
        var expectedArgs = GetCreateContainerArgs(serviceConfig, networks, autoStart);
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.CreateContainer(serviceConfig, networks, autoStart);
        
        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void CreateContainer_WithExisting_ReturnsZero()
    {
        var config = DockerConfigManager.GetSampleConfig();
        var serviceConfig = config.Services.First().Value;
        var networks = config.Networks.ToNetworksEnumerable().ToList();
        var expectedArgs = GetCreateContainerArgs(serviceConfig, networks, true);
        var sut = GetDockerCli(out var shellProcessMock);
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
            $"docker: Error response from daemon: Conflict. The container name \"/{serviceConfig.CurrentName}\" is already in use by container"
        });
        
        var result = sut.CreateContainer(serviceConfig, networks, true);
        
        Assert.Equal(0, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void CreateContainer_WithUnknownError_ThrowsException()
    {
        var config = DockerConfigManager.GetSampleConfig();
        var serviceConfig = config.Services.First().Value;
        var networks = config.Networks.ToNetworksEnumerable().ToList();
        var expectedArgs = GetCreateContainerArgs(serviceConfig, networks, true);
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var action = () =>
        {
            sut.CreateContainer(serviceConfig, networks, true);
        };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void AttachContainerToNetwork_WithAllArgs_ReturnOne()
    {
        const string containerName = "test-container-mock";
        const string networkName = "other-network-mock";
        ServiceNetworkConfig serviceNetwork = new()
        {
            IpV4Address = "1.2.3.4",
            Aliases = new [] { "first-net-alias", "second-net-alias" }
        };
        var expectedArgs = $"network connect --ip={serviceNetwork.IpV4Address} " +
                           $"--network-alias={serviceNetwork.Aliases[0]} " +
                           $"--network-alias={serviceNetwork.Aliases[1]} " +
                           $"{networkName} {containerName}";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { "" });
        
        var result = sut.AttachContainerToNetwork(containerName, networkName, serviceNetwork);
        
        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void AttachContainerToNetwork_WithExistingContainerAndNetwork_ReturnOne()
    {
        const string containerName = "test-container-mock";
        const string networkName = "other-network-mock";
        ServiceNetworkConfig? serviceNetwork = null;
        var expectedArgs = $"network connect {networkName} {containerName}";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { "" });
        
        var result = sut.AttachContainerToNetwork(containerName, networkName, serviceNetwork);
        
        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void AttachContainerToNetwork_WhenAlreadyAttached_ReturnZero()
    {
        const string containerName = "test-container-mock";
        const string networkName = "other-network-mock";
        ServiceNetworkConfig? serviceNetwork = null;
        var expectedArgs = $"network connect {networkName} {containerName}";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"Error response from daemon: endpoint with name {containerName} already exists in network {networkName}"
        });
        
        var result = sut.AttachContainerToNetwork(containerName, networkName, serviceNetwork);
        
        Assert.Equal(0, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void AttachContainerToNetwork_WithNonExistingContainer_ThrowsException()
    {
        const string containerName = "test-container-mock";
        const string networkName = "other-network-mock";
        ServiceNetworkConfig? serviceNetwork = null;
        var expectedArgs = $"network connect {networkName} {containerName}";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        ShellProcessMockSendErrOutput(shellProcessMock, new[]
        {
            $"Error response from daemon: No such container: {containerName}"
        });
        
        var action = () => { sut.AttachContainerToNetwork(containerName, networkName, serviceNetwork); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
 
    [Fact]
    public void StartContainer_WithExisting_ReturnOne()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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

        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"start {containerName}", args);
    }
    
    [Fact]
    public void StartContainer_WithMissing_ReturnsZero()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        Assert.Equal(0, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"start {containerName}", args);
        Assert.Equal($"Container '{containerName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void StartContainer_WithUnknownError_ThrowsException()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
    public void StopContainer_WithExisting_ReturnsOne()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.StopContainer(containerName);
        
        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"stop {containerName}", args);
    }
    
    [Fact]
    public void StopContainer_WithMissing_ReturnsZero()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.StopContainer(containerName);
        
        Assert.Equal(0, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"stop {containerName}", args);
        Assert.Equal($"Container '{containerName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void StopContainer_WithUnknownError_ThrowsException()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var action = () => { sut.StopContainer(containerName); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"stop {containerName}", args);
    }
    
    [Fact]
    public void RemoveContainer_WithExisting_ReturnsOne()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.RemoveContainer(containerName);
        
        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rm {containerName}", args);
    }
    
    [Fact]
    public void RemoveContainer_WithMissing_ReturnsZero()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.RemoveContainer(containerName);
        
        Assert.Equal(0, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rm {containerName}", args);
        Assert.Equal($"Container '{containerName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void RemoveContainer_WithUnknownError_ThrowsException()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var action = () => { sut.RemoveContainer(containerName); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rm {containerName}", args);
    }
    
    [Fact]
    public void RenameContainer_WithExisting_ReturnsOne()
    {
        const string containerName = "test-container-mock";
        var containerNewName = "new-test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.RenameContainer(containerName, containerNewName);
        
        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rename {containerName} {containerNewName}", args);
    }
    
    [Fact]
    public void RenameContainer_WithMissing_ReturnsZero()
    {
        const string containerName = "test-container-mock";
        var containerNewName = "new-test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.RenameContainer(containerName, containerNewName);
        
        Assert.Equal(0, result);
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
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var action = () => { sut.RenameContainer(containerName, containerNewName); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"rename {containerName} {containerNewName}", args);
    }
    
    [Fact]
    public void VolumeExists_WithExisting_ReturnsTrue()
    {
        const string volumeName = "test-docker-volume-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        var sut = GetDockerCli(out var shellProcessMock);
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
    public void CreateVolume_WithExistingOrMissing_ReturnsOne()
    {
        const string volumeName = "test-docker-volume-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.CreateVolume(volumeName);
        
        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume create {volumeName}", args);
    }
    
    [Fact]
    public void CreateVolume_WithUnknownError_ThrowsException()
    {
        const string volumeName = "test-docker-volume-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var action = () => { sut.CreateVolume(volumeName); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume create {volumeName}", args);
    }
    
    [Fact]
    public void RemoveVolume_WithExisting_ReturnsOne()
    {
        const string volumeName = "test-docker-volume-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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

        ShellProcessMockSendStdOutput(shellProcessMock, new[] { volumeName });
        
        var result = sut.RemoveVolume(volumeName);
        
        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume rm {volumeName}", args);
    }
    
    [Fact]
    public void RemoveVolume_WithMissing_ReturnsZero()
    {
        const string volumeName = "test-docker-volume-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.RemoveVolume(volumeName);
        
        Assert.Equal(0, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume rm {volumeName}", args);
        Assert.Equal($"Volume '{volumeName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void RemoveVolume_WithUnknownError_ThrowsException()
    {
        const string volumeName = "test-docker-volume-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var action = () => { sut.RemoveVolume(volumeName); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"volume rm {volumeName}", args);
    }
    
    [Fact]
    public void NetworkExists_WithExisting_ReturnsTrue()
    {
        const string networkName = "test-docker-network-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        var sut = GetDockerCli(out var shellProcessMock);
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
    public void CreateNetwork_WithMissing_ReturnsOne()
    {
        var network = DockerConfigManager.GetSampleConfig().Networks.First().Value
                      ?? throw new NullReferenceException("NetworkConfig");
        var expectedArgs = GetNetworkCreateArgs(network);
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.CreateNetwork(network);
        
        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void CreateNetwork_WithExisting_ReturnsZero()
    {
        var network = DockerConfigManager.GetSampleConfig().Networks.First().Value 
                      ?? throw new NullReferenceException("NetworkConfig");
        var expectedArgs = GetNetworkCreateArgs(network);
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.CreateNetwork(network);
        
        Assert.Equal(0, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void CreateNetwork_WithUnknownError_ThrowsException()
    {
        var network = DockerConfigManager.GetSampleConfig().Networks.First().Value 
                      ?? throw new NullReferenceException("NetworkConfig");
        var expectedArgs = GetNetworkCreateArgs(network);
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var action = () => { sut.CreateNetwork(network); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void RemoveNetwork_WithExisting_ReturnsOne()
    {
        const string networkName = "test-docker-network-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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

        ShellProcessMockSendStdOutput(shellProcessMock, new[] { networkName });
        
        var result = sut.RemoveNetwork(networkName);
        
        Assert.Equal(1, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"network rm {networkName}", args);
    }
    
    [Fact]
    public void RemoveNetwork_WithMissing_ReturnsZero()
    {
        const string networkName = "test-docker-network-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var result = sut.RemoveNetwork(networkName);
        
        Assert.Equal(0, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"network rm {networkName}", args);
        Assert.Equal($"Network '{networkName}' not found!", message);
        Assert.Equal("warn", level);
    }
    
    [Fact]
    public void RemoveNetwork_WithUnknownError_ThrowsException()
    {
        const string networkName = "test-docker-network-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        var action = () => { sut.RemoveNetwork(networkName); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"network rm {networkName}", args);
    }
    
    [Fact]
    public void PullImage_WithMissing_ReturnsTrue()
    {
        const string imageName = "test-image-mock";
        const string imageTag = "latest";
        var sut = GetDockerCli(out var shellProcessMock);
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
            "Digest: sha256:some-docker-image-id-hash",
            $"Status: Downloaded newer image for {imageName}:{imageTag}",
            $"docker.io/library/{imageName}:{imageTag}",
            ""
        });

        var result = sut.PullImage($"{imageName}:{imageTag}");
        
        Assert.True(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"pull {imageName}:{imageTag}", args);
    }
    
    [Fact]
    public void PullImage_WithExisting_ReturnsFalse()
    {
        const string imageName = "test-image-mock";
        const string imageTag = "latest";
        var sut = GetDockerCli(out var shellProcessMock);
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
            "Digest: sha256:some-docker-image-id-hash",
            $"Status: Image is up to date for {imageName}:{imageTag}",
            $"docker.io/library/{imageName}:{imageTag}",
            ""
        });

        var result = sut.PullImage($"{imageName}:{imageTag}");
        
        Assert.False(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"pull {imageName}:{imageTag}", args);
    }
    
    [Fact]
    public void PullImage_WithMissing_ThrowsException()
    {
        const string imageName = "test-image-mock";
        const string imageTag = "latest";
        var sut = GetDockerCli(out var shellProcessMock);
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

        var action = () => { sut.PullImage($"{imageName}:{imageTag}"); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"pull {imageName}:{imageTag}", args);
    }
    
    [Fact]
    public void GetContainerImageId_WithExisting_ReturnsId()
    {
        const string containerName = "test-container-mock";
        var sut = GetDockerCli(out var shellProcessMock);
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
        var sut = GetDockerCli(out var shellProcessMock);
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
        var sut = GetDockerCli(out var shellProcessMock);
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
        const string imageName = "test-image-mock";
        const string imageTag = "latest";
        var sut = GetDockerCli(out var shellProcessMock);
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

        var result = sut.GetImageId($"{imageName}:{imageTag}");
        
        Assert.Equal(expectedResult, result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"image inspect --format \"{{{{lower .Id}}}}\" {imageName}:{imageTag}", args);
    }
    
    [Fact]
    public void GetImageId_WithMissing_ThrowsException()
    {
        const string imageName = "test-image-mock";
        const string imageTag = "latest";
        var sut = GetDockerCli(out var shellProcessMock);
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

        var action = () => { sut.GetImageId($"{imageName}:{imageTag}"); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"image inspect --format \"{{{{lower .Id}}}}\" {imageName}:{imageTag}", args);
    }
    
    [Fact]
    public void GetImageId_WithUnknownError_ThrowsException()
    {
        const string imageName = "test-image-mock";
        const string imageTag = "latest";
        var sut = GetDockerCli(out var shellProcessMock);
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

        var action = () => { sut.GetImageId($"{imageName}:{imageTag}"); };
        
        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal($"image inspect --format \"{{{{lower .Id}}}}\" {imageName}:{imageTag}", args);
    }

    [Fact]
    public void ArchiveDirectory_WithExistingDir_CreatesArchive()
    {
        var tmpDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "tmp");
        if (Directory.Exists(tmpDirectory))
        {
            Directory.Delete(tmpDirectory, true);
        }
        
        try
        {
            var archiveFile = Path.Combine(tmpDirectory, "test_archive.tar.gz");
            var targetDirectory = Path.Combine(tmpDirectory, "archive_target_dir");
            GenerateTempTestFiles(targetDirectory, 5);
            GenerateTempTestFiles(Path.Combine(targetDirectory, "sub_dir"), 3);
            
            var sut = new DockerManager();

            var result = sut.ArchiveDirectory(targetDirectory, archiveFile, dryRun: false);

            Assert.True(result);
            Assert.True(File.Exists(archiveFile));
        }
        finally
        {
            Directory.Delete(tmpDirectory, true);
        }
    }
    
    [Fact]
    public void ArchiveDirectory_WithDryRun_ReturnsFalse()
    {
        var tmpDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "tmp");
        if (Directory.Exists(tmpDirectory))
        {
            Directory.Delete(tmpDirectory, true);
        }
        
        try
        {
            var archiveFile = Path.Combine(tmpDirectory, "test_archive.tar.gz");
            var targetDirectory = Path.Combine(tmpDirectory, "archive_target_dir");
            GenerateTempTestFiles(targetDirectory, 5);
            GenerateTempTestFiles(Path.Combine(targetDirectory, "sub_dir"), 3);
            
            var sut = new DockerManager();

            var result = sut.ArchiveDirectory(targetDirectory, archiveFile, dryRun: true);

            Assert.False(result);
            Assert.False(File.Exists(archiveFile));
        }
        finally
        {
            Directory.Delete(tmpDirectory, true);
        }
    }
    
    [Fact]
    public void ArchiveDirectory_WithInvalidTarget_ThrowsException()
    {
        var tmpDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "tmp");
        var archiveFile = Path.Combine(tmpDirectory, "test_archive.tar.gz");
        var targetDirectory = "na://archive_target_dir";
        
        var sut = new DockerManager();

        var action = () => { sut.ArchiveDirectory(targetDirectory, archiveFile, dryRun: false); };
        
        Assert.Throws<ShellCommandException>(action);
    }
    
    [Fact]
    public void ArchiveDirectory_WithNonExistentTarget_ThrowsException()
    {
        var tmpDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "tmp");
        if (Directory.Exists(tmpDirectory))
        {
            Directory.Delete(tmpDirectory, true);
        }
        
        var archiveFile = Path.Combine(tmpDirectory, "test_archive.tar.gz");
        var targetDirectory = Path.Combine(tmpDirectory, "archive_target_dir");
        
        var sut = new DockerManager();

        var action = () => { sut.ArchiveDirectory(targetDirectory, archiveFile, dryRun: false); };
        
        Assert.Throws<ShellCommandException>(action);
    }
    
    [Fact]
    public void ArchiveVolume_WithExistingDir_CreatesArchive()
    {
        var archiveFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var archiveFileName = "test_volume_archive.tar.gz";
        var archiveFile = Path.Combine(archiveFilePath, archiveFileName);
        var volumeName = "sys--nginx-ssl";
        var expectedArgs = "run --rm " +
                           $"-v {volumeName}:/source-volume:ro " +
                           $"-v {archiveFilePath}:/backup " +
                           "debian:12 " +
                           "tar " +
                           "-C /source-volume " +
                           $"-pczf /backup/{archiveFileName} " +
                           ".";
        var sut = GetDockerCli(out var shellProcessMock);
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

        shellProcessMock.StartAndWaitForExit().Returns(0);
        
        var result = sut.ArchiveVolume(volumeName, archiveFile, dryRun: false);
        
        Assert.True(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void ArchiveVolume_WithDryRun_ReturnsFalse()
    {
        var archiveFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var archiveFileName = "test_volume_archive.tar.gz";
        var archiveFile = Path.Combine(archiveFilePath, archiveFileName);
        var volumeName = "sys--nginx-ssl";
        var expectedArgs = "run --rm " +
                           $"-v {volumeName}:/source-volume:ro " +
                           $"-v {archiveFilePath}:/backup " +
                           "debian:12 " +
                           "tar " +
                           "-C /source-volume " +
                           $"-pczf /backup/{archiveFileName} " +
                           ".";
        var sut = GetDockerCli(out var shellProcessMock);
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

        var result = sut.ArchiveVolume(volumeName, archiveFile, dryRun: true);

        shellProcessMock.Received(0).StartAndWaitForExit();
        Assert.False(result);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
    }
    
    [Fact]
    public void ArchiveVolume_WithNonZeroCliResponse_ThrowsException()
    {
        var archiveFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var archiveFileName = "test_volume_archive.tar.gz";
        var archiveFile = Path.Combine(archiveFilePath, archiveFileName);
        var volumeName = "sys--nginx-ssl";
        var expectedArgs = "run --rm " +
                           $"-v {volumeName}:/source-volume:ro " +
                           $"-v {archiveFilePath}:/backup " +
                           "debian:12 " +
                           "tar " +
                           "-C /source-volume " +
                           $"-pczf /backup/{archiveFileName} " +
                           ".";
        var sut = GetDockerCli(out var shellProcessMock);
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
        
        shellProcessMock.StartAndWaitForExit().Returns(1);

        var action = (() => { sut.ArchiveVolume(volumeName, archiveFile, dryRun: false); });

        Assert.Throws<DockerCliException>(action);
        Assert.Equal(CliCommand, cmd);
        Assert.Equal(expectedArgs, args);
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
    
    private static DockerCli GetDockerCli(out IShellProcess shellProcessMock)
    {
        var provider = TestHelpers.GetShellProcessProvider(out shellProcessMock);
        return new DockerCli(new DockerCliCommand(provider) { Command = CliCommand });
    }

    private static string GetCreateContainerArgs(ServiceConfig config, List<NetworkConfig>? networks, bool autoStart)
    {
        StringBuilder sb = new();
        var isRun = autoStart && (config.Networks?.Count ?? 0) < 2;
        sb.Append(isRun ? "run " : "create ");
        var dockerCommandOptions = config.DockerCommandOptions is not null && config.DockerCommandOptions.Length > 0
            ? config.DockerCommandOptions
            : new[] { "-d" };
        if (!isRun)
        {
            dockerCommandOptions = dockerCommandOptions.Where(x => x != "-d").ToArray();
        }

        if (dockerCommandOptions.Length > 0)
        {
            sb.Append(string.Join(' ', dockerCommandOptions).Trim()).Append(' ');
        }

        sb.Append($"--name={config.CurrentName} ");
        sb.AppendForEach(config.Ports, x => new StringBuilder()
            .Append("-p ")
            .AppendIfNotNullOrEmpty($"{x.HostIp}:", x.HostIp)
            .AppendIfNotNullOrEmpty($"{x.Published}:", x.Published)
            .Append($"{x.Target}")
            .AppendIfNotNullOrEmpty($"/{x.Protocol}", x.Protocol)
            .Append(' ')
            .ToString());
        sb.AppendForEach(config.Volumes, x => $"-v {x.Source}:{x.Target}{(x.ReadOnly ? ":ro" : "")} ");
        sb.AppendForEach(config.EnvFiles, x => $"--env-file {x} ");
        sb.AppendForEach(config.EnvVars, x => $"-e {x.Key}={x.Value} ");
        var defaultNetwork = config.Networks?.FirstOrDefault();
        if (defaultNetwork is not null)
        {
            sb.Append($"--network={GetServiceNetworkName(defaultNetwork.Value.Key, networks)} ");
            sb.Append((config.Hostname == "" ? "" : $"--hostname={config.Hostname ?? config.CurrentName} "));
            sb.AppendIfNotNullOrEmpty($"--ip={defaultNetwork.Value.Value?.IpV4Address} ", defaultNetwork.Value.Value?.IpV4Address);
            sb.AppendIfNotNullOrEmpty($"--ip6={defaultNetwork.Value.Value?.IpV6Address} ", defaultNetwork.Value.Value?.IpV6Address);
            sb.AppendForEach(defaultNetwork.Value.Value?.Aliases, x => $"--network-alias={x} ");
        }
        sb.AppendForEach(config.Links, x => $"--link {x} ");
        sb.AppendForEach(config.ExtraHosts, x => $"--add-host {x.Key}:{x.Value} ");
        sb.AppendForEach(config.Dns, x => $"--dns {x} ");
        sb.AppendForEach(config.Expose, x => $"--expose {x} ");
        sb.Append($"--restart={config.Restart ?? "unless-stopped"} ");
        sb.Append($"--pull={config.PullPolicy ?? "missing"} ");
        sb.Append($"{config.Image}");
        sb.AppendIfNotNullOrEmpty($" --entrypoint {config.Entrypoint}", config.Entrypoint);
        if (config.Command is not null && config.Command.Length > 0)
        {
            sb.Append($" {string.Join(' ', config.Command).Trim()} ");
        }

        return sb.ToString().Trim();
    }

    private static string? GetServiceNetworkName(string network, List<NetworkConfig>? networks) => 
        networks?.FirstOrDefault(x => x.NetworkName == network)?.Name;

    private static string GetNetworkCreateArgs(NetworkConfig network) =>
        $"network create --driver {network.Driver} --attachable " +
        $"--ipam-driver {network.Ipam?.Driver} " +
        $"--subnet {network.Ipam?.Config.Subnet} " +
        $"--ip-range {network.Ipam?.Config.IpRange} " +
        $"--gateway {network.Ipam?.Config.Gateway} " +
        $"{network.Name}";

    private static void GenerateTempTestFiles(string targetDirectory, int count = 1)
    {
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }
        
        for (int i = 0; i < count; i++)
        {
            var id = Guid.NewGuid().ToString();
            File.WriteAllText(Path.Combine(targetDirectory, $"{id.ToLower()}.txt"), 
                $"File: {i}{Environment.NewLine}{id.ToUpper()}");
        }
    }
}