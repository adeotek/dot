using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;

namespace Adeotek.Extensions.Containers.Converters;

public abstract class CustomBaseYamlTypeConverter<T>(
    IValueSerializer? valueSerializer,
    IValueDeserializer? valueDeserializer)
    : IYamlTypeConverter
    where T : notnull
{
    // Unfortunately the API does not provide these in the ReadYaml and WriteYaml methods,
    // so we are forced to set them from the constructor.
    protected readonly IValueSerializer? _valueSerializer = valueSerializer;
    protected readonly IValueDeserializer? _valueDeserializer = valueDeserializer;

    public bool Accepts(Type type) => type == typeof(T);
    
    public virtual object? ReadYaml(IParser parser, Type type)
    {
        ArgumentNullException.ThrowIfNull(_valueDeserializer, "No value deserializer object provided");
        if (TryCustomParse(parser, out var customValue))
        {
            return customValue;
        }

        var value = _valueDeserializer.DeserializeValue(parser, type, new SerializerState(), _valueDeserializer);
        return value is T standardValue
               ? standardValue
               : throw new Exception($"Invalid YAML format for node of type {typeof(T).Name}");
    }
    
    public virtual void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        ArgumentNullException.ThrowIfNull(_valueSerializer, "No value serializer object provided");
        _valueSerializer.SerializeValue(emitter, value, type);
    }

    protected virtual bool TryCustomParse(IParser parser, out T? value)
    {
        value = default;
        return false;
    }
}