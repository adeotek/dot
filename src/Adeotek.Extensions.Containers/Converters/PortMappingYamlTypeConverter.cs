using Adeotek.Extensions.Containers.Config;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Containers.Converters;

public class PortMappingYamlTypeConverter(
    IValueSerializer? valueSerializer = null,
    IValueDeserializer? valueDeserializer = null)
    : CustomBaseYamlTypeConverter<PortMapping>(valueSerializer, valueDeserializer)
{
    protected override bool TryCustomParse(IParser parser, out PortMapping? value)
    {
        if (!parser.TryConsume<Scalar>(out var scalar))
        {
            value = null;
            return false;
        }

        var protocolParts = scalar.Value.Split('/');
        var parts = protocolParts[0].Split(':');
        if (parts.Length is < 1 or > 3)
        {
            throw new Exception("Invalid YAML format for node of type PortMapping");
        }
        
        value = new PortMapping
        {
            Protocol = protocolParts.Length > 1 ? protocolParts[1] : null
        };

        if (parts.Length == 1)
        {
            value.Published = null;
            value.Target = parts[0];
            value.HostIp = null;
        }
        else if (parts.Length == 2)
        {
            value.Published = parts[0];
            value.Target = parts[1];
            value.HostIp = null;
        }
        else
        {
            value.Published = parts[1];
            value.Target = parts[2];
            value.HostIp = parts[0];
        }

        return true;
    }
}