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

        var parts = scalar.Value.Split(':');
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