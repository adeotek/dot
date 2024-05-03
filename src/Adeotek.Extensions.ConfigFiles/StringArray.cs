using System.Text.Json.Serialization;

using Adeotek.Extensions.ConfigFiles.Converters;

namespace Adeotek.Extensions.ConfigFiles;

[JsonConverter(typeof(StringArrayJsonConverter))]
public readonly struct StringArray
{
    public string[] Value { get; }
    
    public StringArray()
    {
        Value = [];
    }
    
    public StringArray(string value)
    {
        Value = [value];
    }

    public StringArray(string[] value)
    {
        Value = value;
    }

    public override string ToString() => string.Join(' ', Value);
    public static implicit operator StringArray(string value) => new(value);
    public static implicit operator StringArray(string[] value) => new(value);
    public static implicit operator string[](StringArray value) => value.Value;
    public static implicit operator string(StringArray value) => value.ToString();
}