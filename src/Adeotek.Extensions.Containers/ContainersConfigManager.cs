﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Adeotek.Extensions.ConfigFiles;
using Adeotek.Extensions.ConfigFiles.Converters;
using Adeotek.Extensions.Containers.Config;
using Adeotek.Extensions.Containers.Converters;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Adeotek.Extensions.Containers;

[ExcludeFromCodeCoverage]
public class ContainersConfigManager : ConfigManager
{
    public static ContainersConfig LoadContainersConfig(string? configFile)
    {
        var configManager = new ContainersConfigManager();
        return ProcessConfig(configManager.LoadConfig<ContainersConfig>(configFile), configFile);
    }

    protected override T LoadConfigFromYamlString<T>(string data)
    {
        try
        {
            var builder = new DeserializerBuilder()
                .WithNamingConvention(YamlNamingConvention);
            return builder
                .IgnoreUnmatchedProperties()
                .WithTypeConverter(new StringArrayYamlTypeConverter(null, builder.BuildValueDeserializer()))
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
    
    private static ContainersConfig ProcessConfig(ContainersConfig config, string? configFile)
    {
        var dirName = Path.GetFileName(Path.GetDirectoryName(configFile));
        if (string.IsNullOrEmpty(dirName))
        {
            return config;
        }
        
        foreach ((string key, ServiceConfig value) in config.Services
            .Where(x => string.IsNullOrEmpty(x.Value.ContainerName)
                && string.IsNullOrEmpty(x.Value.BaseName)))
        {
            value.BaseName = key;
            value.NamePrefix = $"{dirName}-";
        }
        
        return config;
    }

    public static string GetSerializedSampleConfig(string format)
    {
        try
        {
            if (format == "json")
            {
                return JsonSerializer.Serialize(GetSampleConfig(), DefaultJsonSerializerOptions);
            }

            var builder = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance);
            return builder
                .WithQuotingNecessaryStrings()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
                .WithTypeConverter(new StringArrayYamlTypeConverter(builder.BuildValueSerializer(), null))
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
                        Privileged = true,
                        Ports =
                        [
                            new PortMapping { Published = "8000", Target = "8080", HostIp = "0.0.0.0", Protocol = "tcp" },
                            new PortMapping { Target = "443" }
                        ],
                        Volumes =
                        [
                            new VolumeConfig
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
                            new VolumeConfig
                            {
                                Type = "volume",
                                Source = "<docker-volume-name>",
                                Target = "/path/in/container/data",
                                ReadOnly = false
                            }
                        ],
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
                                    Aliases =
                                    [
                                        "my-first-service"
                                    ]
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
                        Expose = new [] { "8080-8082", "443" },
                        Restart = "<optional-restart-mode (default: unless-stopped)>",
                        Entrypoint = "<optional-override-th-image-entrypoint>",
                        Command = new []
                        {
                            "<container-startup-command>",
                            "--some-arg=123",
                            "--flag-arg"
                        },
                        Labels = new Dictionary<string, string>
                        {
                            { "com.example.description", "Some description for the service" }, 
                            { "com.example.scope", "backend" }
                        },
                        InitCliOptions = new []
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
                        Ports =
                        [
                            new PortMapping { Published = "8765", Target = "1234" }
                        ],
                        Volumes =
                        [
                            new VolumeConfig
                            {
                                Source = "<other-docker-volume-name>",
                                Target = "/path/in/container/data"
                            }
                        ],
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
                                    Aliases =
                                    [
                                        "my-second-service"
                                    ]
                                }
                            },
                        },
                        Hostname = "",
                        ExtraHosts = new Dictionary<string, string>
                        {
                            { "host.docker.internal", "host-gateway" }
                        },
                        Restart = "<optional-restart-mode (default: unless-stopped)>",
                        Labels = new Dictionary<string, string>
                        {
                            { "com.example.description", "Another description for the second service" }, 
                            { "com.example.scope", "backend" }
                        },
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
                                Subnet = "172.17.0.0/24",
                                IpRange = "172.17.0.0/26",
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