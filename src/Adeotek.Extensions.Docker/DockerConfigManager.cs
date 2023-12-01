using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
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
            throw new ConfigFileException("Config data is not in a valid YAML format", null, e);
        }
    }

    public static ContainersConfig GetSampleConfig() => new()
        {
            Services = new Dictionary<string, ServiceConfig>(),
            Networks = new Dictionary<string, NetworkConfig>()
        };
}