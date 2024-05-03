using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;

namespace Adeotek.Extensions.ConfigFiles.Converters;

public class StringArrayYamlTypeConverter(
    IValueSerializer? _valueSerializer,
    IValueDeserializer? _valueDeserializer)
    : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(StringArray) || type == typeof(StringArray?);
    
    public object ReadYaml(IParser parser, Type type)
    {
        ArgumentNullException.ThrowIfNull(_valueDeserializer, "No value deserializer object provided");
        
        if (parser.TryConsume<Scalar>(out var scalar))
        {
            return new StringArray(scalar.Value);
        }

        var value = _valueDeserializer.DeserializeValue(parser, typeof(string[]), new SerializerState(), _valueDeserializer);    
        return value is string[] standardValue
            ? new StringArray(standardValue)
            : throw new Exception($"Invalid YAML format for node of type {nameof(StringArray)}");
    }
    
    public virtual void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        ArgumentNullException.ThrowIfNull(_valueSerializer, "No value serializer object provided");
        if (value is StringArray stringArray)
        {
            _valueSerializer.SerializeValue(emitter, stringArray.Value, typeof(string[]));
        }
        else
        {
            _valueSerializer.SerializeValue(emitter, value, type);    
        }
    }
}