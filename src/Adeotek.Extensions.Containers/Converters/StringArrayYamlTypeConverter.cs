using Adeotek.Extensions.ConfigFiles;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Containers.Converters;

public class StringArrayYamlTypeConverter(
    IValueSerializer? valueSerializer = null,
    IValueDeserializer? valueDeserializer = null)
    : CustomBaseYamlTypeConverter<StringArray>(valueSerializer, valueDeserializer)
{
    protected override bool TryCustomParse(IParser parser, out StringArray value)
    {
        if (!parser.TryConsume<Scalar>(out var scalar))
        {
            value = default;
            return false;
        }

        var protocolParts = scalar.Value.Split('/');
        var parts = protocolParts[0].Split(':');
        if (parts.Length is < 1 or > 3)
        {
            throw new Exception("Invalid YAML format for node of type PortMapping");
        }

        value = new StringArray();
        return true;
    }
}