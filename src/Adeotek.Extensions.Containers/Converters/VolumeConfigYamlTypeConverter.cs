using System.Text.RegularExpressions;

using Adeotek.Extensions.Containers.Config;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Containers.Converters;

public class VolumeConfigYamlTypeConverter : CustomBaseYamlTypeConverter<VolumeConfig>
{
    public VolumeConfigYamlTypeConverter(
        IValueSerializer? valueSerializer = null, 
        IValueDeserializer? valueDeserializer = null)
        : base(valueSerializer, valueDeserializer)
    { }
    
    protected override bool TryCustomParse(IParser parser, out VolumeConfig? value)
    {
        if (!parser.TryConsume<Scalar>(out var scalar))
        {
            value = null;
            return false;
        }

        string[] parts;
        var regEx = new Regex(@"^[a-z]{1}:\\", RegexOptions.IgnoreCase);
        var regExMatches = regEx.Matches(scalar.Value);
        if (regExMatches.Count == 1)
        {
            parts = scalar.Value[regExMatches[0].Length..].Split(':');
            parts[0] = $"{regExMatches[0].Value}{parts[0]}";
        }
        else
        {
            parts = scalar.Value.Split(':');
        }
        
        if (parts.Length is < 2 or > 3)
        {
            throw new Exception("Invalid YAML format for node of type VolumeConfig");
        }

        value = new VolumeConfig
        {
            Source = parts[0],
            Target = parts[1]    
        };
        
        if (parts[0].Contains('/') || parts[0].Contains('\\'))
        {
            value.Type = "bind";
            value.Bind = new VolumeBindConfig();
        }
        else
        {
            value.Type = "volume";
            value.Volume = new VolumeVolumeConfig();
        }

        return true;
    }
}