using System.Diagnostics.CodeAnalysis;

using Adeotek.Extensions.Docker.Config.V1;

namespace Adeotek.Extensions.Docker.Config;

[ExcludeFromCodeCoverage]
public static class MapperExtensions
{
    public static ContainersConfig ToContainersConfig(this ContainerConfigV1 config) =>
        new()
        {
            Services = config.ExtractService(),
            Networks = config.ExtractNetwork()
        };

    private static Dictionary<string, ServiceConfig> ExtractService(this ContainerConfigV1 config)
    {
        return new Dictionary<string, ServiceConfig>
        {
            {
                config.Name,
                new ServiceConfig
                {
                    Image = $"{config.Image}{(string.IsNullOrEmpty(config.Tag) ? "" : $":{config.Tag}")}",
                    PullPolicy = null,
                    ContainerName = null,
                    NamePrefix = config.NamePrefix,
                    BaseName = config.Name,
                    CurrentSuffix = config.CurrentSuffix,
                    PreviousSuffix = config.PreviousSuffix,
                    Ports = config.Ports.Select(x => x.ToV2()).ToArray(),
                    Volumes = config.Volumes.Select(x => x.ToV2()).ToArray(),
                    EnvFiles = null,
                    EnvVars = config.EnvVars,
                    Networks = config.ExtractServiceNetwork(),
                    Links = null,
                    Hostname = config.Network?.Hostname,
                    ExtraHosts = config.ExtraHosts,
                    Dns = null,
                    Restart = config.Restart,
                    Entrypoint = null,
                    Command = config.Command is null 
                        ? null
                        : (new[] { config.Command }).Union(config.CommandArgs).ToArray(),
                    Expose = null,
                    Attach = true,
                    RunCommandOptions = config.RunCommandOptions
                }
            }
        };
    }
    
    private static PortMapping ToV2(this PortMappingV1 port) =>
        new()
        {
            Target = port.Container,
            Published = port.Host.ToString(),
            HostIp = null,
            Protocol = null,
            Mode = null
        };

    private static VolumeConfig ToV2(this VolumeConfigV1 volume)
    {
        return new()
        {
            Type = volume.IsBind ? "bind" : "volume",
            Source = volume.Source,
            Target = volume.Destination,
            ReadOnly = volume.IsReadonly,
            Bind = volume.IsBind 
                ? new VolumeBindConfig
                {
                    Propagation = null,
                    CreateHostPath = volume.AutoCreate,
                    SeLinux = null
                }
                : null,
            Volume = !volume.IsBind ? new VolumeVolumeConfig { NoCopy = false } : null,
            TmpFs = null
        };
    }

    private static Dictionary<string, NetworkConfig> ExtractNetwork(this ContainerConfigV1 config)
    {
        var result = new Dictionary<string, NetworkConfig>();
        if (config.Network is null)
        {
            return result;
        }
        
        result.Add(config.Network.Name, new NetworkConfig
        {
            Name = config.Network.Name,
            Driver = "bridge",
            Ipam = config.Network.Subnet is null && config.Network.IpRange is null ? null : new NetworkIpam
            {
                Config = new NetworkIpamConfig
                {
                    Subnet = config.Network.Subnet ?? "",
                    IpRange = config.Network.IpRange ?? "",
                    Gateway = null
                }
            },
            Attachable = true,
            External = false,
            Internal = false,
            Preserve = config.Network.IsShared
        });
        return result;
    }
    
    private static Dictionary<string, ServiceNetworkConfig>? ExtractServiceNetwork(this ContainerConfigV1 config)
    {
        if (config.Network is null)
        {
            return null;
        }
        
        var result = new Dictionary<string, ServiceNetworkConfig>
        { 
            { 
                config.Network.Name, 
                new ServiceNetworkConfig
                {
                    IpV4Address = config.Network.IpAddress,
                    IpV6Address = null,
                    Aliases = string.IsNullOrEmpty(config.Network.Alias) ? null : new []{ config.Network.Alias }
                }
            } 
        };
        return result;
    }
}