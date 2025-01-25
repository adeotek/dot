using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;

namespace Adeotek.Extensions.ConfigFiles.Converters;

public class StringArrayYamlTypeConverter(
    IValueSerializer? valueSerializer,
    IValueDeserializer? valueDeserializer)
    : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(StringArray) || type == typeof(StringArray?);
    
    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        ArgumentNullException.ThrowIfNull(valueDeserializer, "No value deserializer object provided");
        
        if (parser.TryConsume<Scalar>(out var scalar))
        {
            return new StringArray(scalar.Value);
        }
        
        var value = valueDeserializer.DeserializeValue(parser, typeof(string[]), new SerializerState(), valueDeserializer);    
        return value is string[] standardValue
            ? new StringArray(standardValue)
            : throw new Exception($"Invalid YAML format for node of type {nameof(StringArray)}");
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(valueSerializer, "No value serializer object provided");
        if (value is StringArray stringArray)
        {
            valueSerializer.SerializeValue(emitter, stringArray.Value, typeof(string[]));
        }
        else
        {
            valueSerializer.SerializeValue(emitter, value, type);    
        }
    }
}