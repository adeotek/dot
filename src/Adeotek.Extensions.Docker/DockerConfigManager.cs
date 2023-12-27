using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Adeotek.Extensions.ConfigFiles;
using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Config.V1;
using Adeotek.Extensions.Docker.Converters;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Adeotek.Extensions.Docker;

[ExcludeFromCodeCoverage]
public class DockerConfigManager : ConfigManager
{
    public static ContainersConfig LoadContainersConfig(string? configFile, string? version = null)
    {
        var configManager = new DockerConfigManager();
        if (version != "v1")
        {
            return configManager.LoadConfig<ContainersConfig>(configFile); 
        }

        configManager.YamlNamingConvention = PascalCaseNamingConvention.Instance;
        var configV1 = configManager.LoadConfig<ContainerConfigV1>(configFile);
        return configV1.ToContainersConfig();
    }

    protected override T LoadConfigFromYamlString<T>(string data)
    {
        try
        {
            var builder = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance);
            return builder
                       .WithTypeConverter(new PortMappingYamlTypeConverter(null, builder.BuildValueDeserializer()))
                       .WithTypeConverter(new VolumeConfigYamlTypeConverter(null, builder.BuildValueDeserializer()))
                       .Build()
                       .Deserialize<T>(data)
                   ?? throw new ConfigFileException("Unable to deserialize YAML config data");
        }
        catch (Exception e)
        {
            throw new ConfigFileException("Config data is not in a valid YAML format", null, e);
        }
    }

    public static string GetSerializedSampleConfig(string format)
    {
        try
        {
            if (format == "json")
            {
                return JsonSerializer.Serialize(GetSampleConfig(), DefaultJsonSerializerOptions);
            }
            
            return new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .WithQuotingNecessaryStrings()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
                .EnsureRoundtrip()
                .Build()
                .Serialize(GetSampleConfig());
        }
        catch (Exception e)
        {
            throw new ConfigFileException("Config data is not in a valid YAML format", null, e);
        }
    }

    public static ContainersConfig GetSampleConfig() => new()
        {
            Services = new Dictionary<string, ServiceConfig>
            {
                { 
                    "first-service", 
                    new ServiceConfig
                    {
                        Image = "[<registry>/][<project>/]<image>[:<tag>|@<digest>]",
                        PullPolicy = "<optional-pull-policy (default: missing)>",
                        NamePrefix = "<optional-container-name-prefix>",
                        BaseName = "<base-container-name>",
                        CurrentSuffix = "<optional-container-name-suffix>",
                        PreviousSuffix = "<optional-demoted-container-name-suffix>",
                        Ports = new PortMapping[]
                        {
                            new() { Published = "8000", Target = "8080", HostIp = "0.0.0.0", Protocol = "tcp" },
                            new() { Target = "443" }
                        },
                        Volumes = new VolumeConfig[]
                        {
                            new()
                            {
                                Type = "bind",
                                Source = "/path/on/host/data",
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
                                Source = "<docker-volume-name>",
                                Target = "/path/in/container/data",
                                ReadOnly = false
                            }
                        },
                        EnvFiles = new [] { "/<host-path>/<env-vars-file>" },
                        EnvVars = new Dictionary<string, string>
                        {
                            { "TZ", "UTC" }, 
                            { "DEBUG_MODE", "true" }
                        },
                        Networks = new Dictionary<string, ServiceNetworkConfig?>
                        {
                            {
                                "some-network",
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
                                "other-network", 
                                new ServiceNetworkConfig()
                            }
                        },
                        Links = new []
                        {
                            "second-service",
                            "second-service:<alias>"
                        },
                        Hostname = "optional-hostname-or-null-for-autogenerated",
                        ExtraHosts = new Dictionary<string, string>
                        {
                            { "host.docker.internal", "host-gateway" }, 
                            { "some.other.docker.container", "172.17.0.11" }
                        },
                        Dns = new [] { "8.8.8.8", "8.8.4.4" },
                        Restart = "<optional-restart-mode (default: unless-stopped)>",
                        Entrypoint = "<optional-override-th-image-entrypoint>",
                        Command = new []
                        {
                            "<container-startup-command>",
                            "--some-arg=123",
                            "--flag-arg"
                        },
                        // Expose = new [] { "8080", "443" },
                        // Attach = true,
                        RunCommandOptions = new []
                        {
                            "-it",
                            "<any-docker-run-option>"
                        }
                    }
                },
                { 
                    "second-service", 
                    new ServiceConfig
                    {
                        Image = "[<registry>/][<project>/]<image>[:<tag>|@<digest>]",
                        PullPolicy = "<optional-pull-policy (default: missing)>",
                        ContainerName = "<full-container-name>",
                        Ports = new PortMapping[]
                        {
                            new() { Published = "8765", Target = "1234" } 
                        },
                        Volumes = new VolumeConfig[]
                        {
                            new()
                            {
                                Source = "<other-docker-volume-name>",
                                Target = "/path/in/container/data"
                            }
                        },
                        EnvVars = new Dictionary<string, string>
                        {
                            { "TZ", "UTC" } 
                        },
                        Networks = new Dictionary<string, ServiceNetworkConfig?>
                        {
                            {
                                "some-network",
                                new ServiceNetworkConfig
                                {
                                    IpV4Address = "172.17.0.11",
                                    Aliases = new []
                                    {
                                        "my-second-service"
                                    }
                                }
                            },
                        },
                        Hostname = "",
                        ExtraHosts = new Dictionary<string, string>
                        {
                            { "host.docker.internal", "host-gateway" }
                        },
                        Restart = "<optional-restart-mode (default: unless-stopped)>"
                    }
                }
            },
            Networks = new Dictionary<string, NetworkConfig>
            {
                { 
                    "some-network", 
                    new NetworkConfig
                    {
                        Name = "<some-docker-network-name>",
                        Driver = "bridge",
                        Attachable = true,
                        External = false,
                        Internal = false,
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
                    "other-network", 
                    new NetworkConfig
                    {
                        Name = "<other-docker-network-name>",
                        Driver = "host",
                        External = true
                    }
                }
            }
        };
}