using System.Text;
using System.Text.Json;

using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Exceptions;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Adeotek.Extensions.Docker;

public class DockerConfigManager
{
    public static ContainerConfig LoadConfig(string? configFile)
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
            return LoadJsonConfig(configContent);
        }

        if ((new [] {".yaml", ".yml"}).Contains(Path.GetExtension(configFile).ToLower()))
        {
            return LoadYamlConfig(configContent);
        }

        throw new DockerConfigException("The config file is not in a valid format", configFile);
    }

    public static ContainerConfig LoadJsonConfig(string data)
    {
        try
        {
            return JsonSerializer.Deserialize<ContainerConfig>(data)
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
    
    public static ContainerConfig LoadYamlConfig(string data)
    {
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance) 
                .Build();
            return deserializer.Deserialize<ContainerConfig>(data)
                   ?? throw new DockerConfigException("Unable to deserialize YAML config data");
        }
        catch (DockerConfigException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new DockerConfigException("Config data is not in a valid YAML format", null, e);
        }
    }
}