using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Exceptions;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Adeotek.Extensions.Docker;

[ExcludeFromCodeCoverage]
public class DockerConfigManager
{
    public static ContainerConfig LoadContainerConfig(string? configFile)
    {
        if (string.IsNullOrEmpty(configFile))
        {
            throw new DockerConfigException("Null or empty config file name", configFile);
        }

        if (!File.Exists(configFile))
        {
            throw new DockerConfigException("The config file does not exist", configFile);
        }
        
        var configContent = File.ReadAllText(configFile, Encoding.UTF8);
        if (string.IsNullOrEmpty(configContent))
        {
            throw new DockerConfigException("The config file is empty!", configFile);
        }

        if (Path.GetExtension(configFile).ToLower() == ".json")
        {
            return LoadContainerConfigFromJsonString(configContent);
        }

        if ((new [] {".yaml", ".yml"}).Contains(Path.GetExtension(configFile).ToLower()))
        {
            return LoadContainerConfigFromYamlString(configContent);
        }

        throw new DockerConfigException("The config file is not in a valid format", configFile);
    }

    public static ContainerConfig LoadContainerConfigFromJsonString(string data)
    {
        try
        {
            return JsonSerializer.Deserialize<ContainerConfig>(data, 
                    new JsonSerializerOptions { AllowTrailingCommas = true })
                ?? throw new DockerConfigException("Unable to deserialize JSON config data");
        }
        catch (DockerConfigException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new DockerConfigException("Config data is not in a valid JSON format", null, e);
        }
    }
    
    public static ContainerConfig LoadContainerConfigFromYamlString(string data)
    {
        try
        {
            return new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance) 
                .Build()
                .Deserialize<ContainerConfig>(data)
                   ?? throw new DockerConfigException("Unable to deserialize YAML config data");
        }
        catch (Exception e)
        {
            throw new DockerConfigException("Config data is not in a valid YAML format", null, e);
        }
    }
    
    public static string GetSerializedSampleConfig(string format)
    {
        try
        {
            if (format == "json")
            {
                return JsonSerializer.Serialize(GetSampleConfig(), new JsonSerializerOptions
                    {
                        WriteIndented = true, 
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
            }
            
            return new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .WithQuotingNecessaryStrings()
                .EnsureRoundtrip()
                .Build()
                .Serialize(GetSampleConfig());
        }
        catch (Exception e)
        {
            throw new DockerConfigException("Config data is not in a valid YAML format", null, e);
        }
    }

    public static ContainerConfig GetSampleConfig() => new()
        {
            Image = "<image-name>",
            Tag = "<image-tag>",
            NamePrefix = "<optional-container-name-prefix>",
            BaseName = "<base-container-name>",
            PrimarySuffix = "<optional-container-name-suffix>",
            BackupSuffix = "<optional-demoted-container-name-suffix>",
            Ports = new PortMapping[]
            {
                new() { Host = 8080, Container = 80 }, 
                new() { Host = 8443, Container = 443 }
            },
            Volumes = new VolumeConfig[]
            {
                new()
                {
                    Source = "/path/on/host/data",
                    Destination = "/path/in/container/data",
                    IsBind = true,
                    AutoCreate = false
                },
                new()
                {
                    Source = "<docker-volume-name>",
                    Destination = "/path/in/container/data",
                    IsBind = false,
                    AutoCreate = true
                }
            },
            EnvVars = new()
            {
                { "TZ", "UTC" }, 
                { "DEBUG_MODE", "true" }
            },
            Network = new()
            {
                Name = "<docker-network-name>",
                Subnet = "172.17.0.1/24",
                IpRange = "172.17.0.1/26",
                IpAddress = "172.17.0.2",
                Hostname = "optional-hostname-or-null-for-autogenerated",
                Alias = "optional-hostname-alias-or-null-for-autogenerated",
                IsShared = false
            },
            Restart = "<optional-restart-mode (default: unless-stopped)>"
        };
}