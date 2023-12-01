using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;

namespace Adeotek.Extensions.Docker.Converters;

public abstract class CustomBaseYamlTypeConverter<T> : IYamlTypeConverter where T : notnull
{
    // Unfortunately the API does not provide these in the ReadYaml and WriteYaml methods,
    // so we are forced to set them from the constructor.
    protected readonly IValueSerializer? _valueSerializer;
    protected readonly IValueDeserializer? _valueDeserializer;

    protected CustomBaseYamlTypeConverter(
        IValueSerializer? valueSerializer, 
        IValueDeserializer? valueDeserializer)
    {
        _valueSerializer = valueSerializer;
        _valueDeserializer = valueDeserializer;
    }
    
    public bool Accepts(Type type) => type == typeof(T);
    
    public virtual object? ReadYaml(IParser parser, Type type)
    {
        if (_valueDeserializer is null)
        {
            throw new ArgumentNullException(nameof(_valueSerializer), "No value deserializer object provided");
        }

        if (TryCustomParse(parser, out var customValue))
        {
            return customValue;
        }

        var value = _valueDeserializer.DeserializeValue(parser, type, new SerializerState(), _valueDeserializer);
        return value is T standardValue
               ? standardValue
               : throw new Exception("Invalid YAML format for node of type VolumeConfig");
    }
    
    public virtual void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (_valueSerializer is null)
        {
            throw new ArgumentNullException(nameof(_valueSerializer), "No value serializer object provided");
        }
        
        _valueSerializer.SerializeValue(emitter, value, type);
    }

    protected virtual bool TryCustomParse(IParser parser, out T? value)
    {
        value = default;
        return false;
    }
}