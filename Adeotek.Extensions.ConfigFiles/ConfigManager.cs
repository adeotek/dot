using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Adeotek.Extensions.ConfigFiles;

[ExcludeFromCodeCoverage]
public class ConfigManager
{
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
    {
        AllowTrailingCommas = true, 
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false
    };

    public INamingConvention YamlNamingConvention { get; set; } = UnderscoredNamingConvention.Instance;
    
    public T LoadConfig<T>(string? configFile) where T : class
    {
        if (string.IsNullOrEmpty(configFile))
        {
            throw new ConfigFileException("Null or empty config file name", configFile);
        }

        if (!File.Exists(configFile))
        {
            throw new ConfigFileException("The config file does not exist", configFile);
        }
        
        var configContent = File.ReadAllText(configFile, Encoding.UTF8);
        if (string.IsNullOrEmpty(configContent))
        {
            throw new ConfigFileException("The config file is empty!", configFile);
        }

        if (Path.GetExtension(configFile).ToLower() == ".json")
        {
            return LoadConfigFromJsonString<T>(configContent);
        }

        if ((new [] {".yaml", ".yml"}).Contains(Path.GetExtension(configFile).ToLower()))
        {
            return LoadConfigFromYamlString<T>(configContent);
        }

        throw new ConfigFileException("The config file is not in a valid format", configFile);
    }

    protected virtual T LoadConfigFromJsonString<T>(string data) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(data, JsonSerializerOptions)
                   ?? throw new ConfigFileException("Unable to deserialize JSON config data");
        }
        catch (ConfigFileException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ConfigFileException("Config data is not in a valid JSON format", null, e);
        }
    }
    
    protected virtual T LoadConfigFromYamlString<T>(string data) where T : class
    {
        try
        {
            return new DeserializerBuilder()
                .WithNamingConvention(YamlNamingConvention) 
                .Build()
                .Deserialize<T>(data)
                   ?? throw new ConfigFileException("Unable to deserialize YAML config data");
        }
        catch (Exception e)
        {
            throw new ConfigFileException("Config data is not in a valid YAML format", null, e);
        }
    }
}