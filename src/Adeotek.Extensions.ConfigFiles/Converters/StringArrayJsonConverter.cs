using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adeotek.Extensions.ConfigFiles.Converters;

public class StringArrayJsonConverter : JsonConverter<StringArray>
{
    public override StringArray Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => 
                new StringArray(reader.GetString() 
                    ?? throw new JsonException($"Invalid JSON format for node of type {nameof(StringArray)}")),
            JsonTokenType.StartArray =>
                new StringArray(JsonSerializer.Deserialize<string[]>(ref reader, options) 
                    ?? throw new JsonException($"Invalid JSON format for node of type {nameof(StringArray)}")),
            _ => throw new JsonException($"Unexpected token type {reader.TokenType} for node of type {nameof(StringArray)}")
        };
    }

    public override void Write(Utf8JsonWriter writer, StringArray value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}