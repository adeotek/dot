using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Processes;

using NSubstitute;

namespace Adeotek.Extensions.Docker.Tests;

public class DockerManagerTests
{
    private const string CliCommand = "docker";
    
    [Fact]
    public void PurgeVolumes_WithSharedVolumes_DoNotRemoveAny()
    {
        var targetServiceName = "target-service";
        var bindPath = "/path/on/host/data";
        var volumeName = "purge-target-volume";
        ContainersConfig config = GetConfigForVolumes(targetServiceName, bindPath, volumeName);
        List<ServiceConfig> targetServices = new()
        {
            config.Services[targetServiceName].SetServiceName(targetServiceName)
        };
        var sut = GetDockerManager(out var shellProcessMock);
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { "" });

        var result = sut.PurgeVolumes(targetServices, config, false);
        
        Assert.Equal(0, result);
        shellProcessMock.ReceivedWithAnyArgs(0).StartAndWaitForExit();
    }
    
    [Fact]
    public void PurgeVolumes_WithDedicatedVolume_RemoveOne()
    {
        var targetServiceName = "target-service";
        var bindPath = "/path/on/host/data";
        var volumeName = "purge-target-volume";
        ContainersConfig config = GetConfigForVolumes(targetServiceName, bindPath, volumeName, 
            otherVolumeName: "other-docker-volume");
        List<ServiceConfig> targetServices = new()
        {
            config.Services[targetServiceName].SetServiceName(targetServiceName)
        };
        var sut = GetDockerManager(out var shellProcessMock);
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { volumeName });

        var result = sut.PurgeVolumes(targetServices, config, false);
        
        Assert.Equal(1, result);
        shellProcessMock.ReceivedWithAnyArgs(1).StartAndWaitForExit();
    }
    
    [Fact]
    public void PurgeVolumes_WithDedicatedVolumes_RemoveTwo()
    {
        var targetServiceName = "target-service";
        var volumeName = "purge-target-volume";
        ContainersConfig config = GetConfigForVolumes(targetServiceName, volumeName, volumeName, 
            "/other/bind/path", "other-docker-volume");
        List<ServiceConfig> targetServices = new()
        {
            config.Services[targetServiceName].SetServiceName(targetServiceName)
        };
        var sut = GetDockerManager(out var shellProcessMock);
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { volumeName });

        var result = sut.PurgeVolumes(targetServices, config, false);
        
        // warning: bind volumes will not be purged
        Assert.Equal(1, result);
        shellProcessMock.ReceivedWithAnyArgs(1).StartAndWaitForExit();
    }
    
    [Fact]
    public void PurgeNetworks_WithOneSharedNetworkAndOtherExternal_DoNotRemoveAny()
    {
        var targetServiceName = "target-service";
        var firstNetworkName = "target-docker-network";
        var firstNetworkKey = "first-network";
        var secondNetworkKey = "second-network";
        ContainersConfig config = GetConfigForNetworks(targetServiceName, firstNetworkName,
            firstNetworkKey, secondNetworkKey, 1, true);
        List<ServiceConfig> targetServices = new()
        {
            config.Services[targetServiceName].SetServiceName(targetServiceName)
        };
        var sut = GetDockerManager(out var shellProcessMock);
        
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { "" });

        var result = sut.PurgeNetworks(targetServices, config, false);
        
        Assert.Equal(0, result);
        shellProcessMock.ReceivedWithAnyArgs(0).StartAndWaitForExit();
    }
    
    [Fact]
    public void PurgeNetworks_WithOneDedicatedNetworkAndOneExternal_RemoveOne()
    {
        var targetServiceName = "target-service";
        var firstNetworkName = "target-docker-network";
        var firstNetworkKey = "first-network";
        var secondNetworkKey = "second-network";
        ContainersConfig config = GetConfigForNetworks(targetServiceName, firstNetworkName, 
            firstNetworkKey, secondNetworkKey, 2, true);
        List<ServiceConfig> targetServices = new()
        {
            config.Services[targetServiceName].SetServiceName(targetServiceName)
        };
        var sut = GetDockerManager(out var shellProcessMock);
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { firstNetworkName });

        var result = sut.PurgeNetworks(targetServices, config, false);
        
        Assert.Equal(1, result);
        shellProcessMock.ReceivedWithAnyArgs(1).StartAndWaitForExit();
    }
    
    [Fact]
    public void PurgeNetworks_WithDedicatedNetworks_RemoveTwo()
    {
        var targetServiceName = "target-service";
        var firstNetworkName = "target-docker-network";
        var firstNetworkKey = "first-network";
        var secondNetworkKey = "second-network";
        ContainersConfig config = GetConfigForNetworks(targetServiceName, firstNetworkName,
            firstNetworkKey, secondNetworkKey, 0, false);
        List<ServiceConfig> targetServices = new()
        {
            config.Services[targetServiceName].SetServiceName(targetServiceName)
        };
        var sut = GetDockerManager(out var shellProcessMock);
        ShellProcessMockSendStdOutput(shellProcessMock, new[] { firstNetworkName });

        var result = sut.PurgeNetworks(targetServices, config, false);
        
        Assert.Equal(2, result);
        shellProcessMock.ReceivedWithAnyArgs(2).StartAndWaitForExit();
    }
    
    private static DockerManager GetDockerManager(out IShellProcess shellProcessMock)
    {
        var provider = TestHelpers.GetShellProcessProvider(out shellProcessMock);
        return new DockerManager(new DockerCliCommand(provider) { Command = CliCommand });
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
    
    private static ContainersConfig GetConfigForVolumes(string targetServiceName,
        string bindPath, string volumeName, 
        string? otherBindPath = null, string? otherVolumeName = null) =>
        new()
        {
            Services = new Dictionary<string, ServiceConfig>
            {
                { 
                    targetServiceName, 
                    new ServiceConfig
                    {
                        
                        Volumes = new VolumeConfig[]
                        {
                            new()
                            {
                                Type = "bind",
                                Source = bindPath,
                                Target = "/path/in/container/data",
                                ReadOnly = true,
                                Bind = new VolumeBindConfig
                                {
                                    CreateHostPath = true
                                }
                            },
                            new()
                            {
                                Type = "volume",
                                Source = volumeName,
                                Target = "/path/in/container/data",
                                ReadOnly = false
                            }
                        }
                    }
                },
                { 
                    "other-service", 
                    new ServiceConfig
                    {
                        Volumes = new VolumeConfig[]
                        {
                            new()
                            {
                                Type = "bind",
                                Source = otherBindPath ?? bindPath,
                                Target = "/path/in/container/data",
                                ReadOnly = true,
                                Bind = new VolumeBindConfig
                                {
                                    CreateHostPath = true
                                }
                            },
                            new()
                            {
                                Type = "volume",
                                Source = otherVolumeName ?? volumeName,
                                Target = "/path/in/container/data"
                            }
                        }
                    }
                }
            }
        };

    private static ContainersConfig GetConfigForNetworks(string targetServiceName, string firstNetworkName,
        string firstNetworkKey, string secondNetworkKey,
        int sharedNetworks, bool secondNetworkExternal) =>
        new()
        {
            Services = new Dictionary<string, ServiceConfig>
            {
                { 
                    targetServiceName, 
                    new ServiceConfig
                    {
                        Networks = new Dictionary<string, ServiceNetworkConfig?>
                        {
                            {
                                firstNetworkKey,
                                new ServiceNetworkConfig
                                {
                                    IpV4Address = "172.17.0.10",
                                    Aliases = new []
                                    {
                                        "my-first-service"
                                    }
                                }
                            },
                            {
                                secondNetworkKey, 
                                new ServiceNetworkConfig()
                            }
                        }
                    }
                },
                { 
                    "other-service", 
                    new ServiceConfig
                    {
                        Networks = new Dictionary<string, ServiceNetworkConfig?>
                        {
                            {
                                sharedNetworks switch
                                {
                                    1 => firstNetworkKey,
                                    2 => secondNetworkKey,
                                    _ => "host-network"
                                },
                                new ServiceNetworkConfig
                                {
                                    IpV4Address = "172.17.0.11",
                                    Aliases = new []
                                    {
                                        "my-second-service"
                                    }
                                }
                            },
                        }
                    }
                }
            },
            Networks = new Dictionary<string, NetworkConfig>
            {
                { 
                    firstNetworkKey, 
                    new NetworkConfig
                    {
                        Name = firstNetworkName,
                        Driver = "bridge",
                        Attachable = true,
                        Internal = false,
                        External = false,
                        Ipam = new NetworkIpam
                        {
                            Driver = "default",
                            Config = new NetworkIpamConfig
                            {
                                Subnet = "172.17.0.1/24",
                                IpRange = "172.17.0.1/26",
                                Gateway = "172.17.0.1"
                            }
                        }
                    }
                },
                { 
                    secondNetworkKey, 
                    new NetworkConfig
                    {
                        Name = sharedNetworks == 0 ? firstNetworkName : "other-docker-network",
                        Driver = "bridge",
                        External = secondNetworkExternal
                    }
                },
                { 
                    "host-network", 
                    new NetworkConfig
                    {
                        Name = "docker-host-network",
                        Driver = "host",
                        External = true
                    }
                }
            }
        };
}