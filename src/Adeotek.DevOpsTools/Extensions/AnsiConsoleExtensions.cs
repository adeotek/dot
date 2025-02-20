﻿using System.Text.Json;

using Spectre.Console;

namespace Adeotek.DevOpsTools.Extensions;

internal static class AnsiConsoleExtensions
{
    public static IAnsiConsole WriteJson(this IAnsiConsole console, JsonElement node, JsonStyle? jsonStyle = null)
    {
        ArgumentNullException.ThrowIfNull(console);

        console.WriteJson(node, jsonStyle ?? JsonStyle.Default, 0);
        return console;
    }

    private static IAnsiConsole WriteJson(this IAnsiConsole console, JsonElement node, JsonStyle jsonStyle,
        int indentionLevel)
    {
        switch (node.ValueKind)
        {
            case JsonValueKind.Undefined:
                break;

            case JsonValueKind.Object:
                var indent = new string(' ', indentionLevel * jsonStyle.IndentSize);
                var firstPropertyWritten = false;
                foreach (var property in node.EnumerateObject())
                {
                    console.Write(!firstPropertyWritten
                        ? new Text("{", jsonStyle.CurlyBracketStyle)
                        : new Text(",", jsonStyle.ValueSeparatorStyle));
                    console.WriteLine();
                    console.Write(indent + "  \"");
                    console.Write(new Text(property.Name, jsonStyle.NameStyle));
                    console.Write("\"");
                    console.Write(new Text(": ", jsonStyle.NameSeparatorStyle));
                    console.WriteJson(property.Value, jsonStyle, indentionLevel + 1);
                    firstPropertyWritten = true;
                }

                if (firstPropertyWritten)
                {
                    console.WriteLine();
                    console.Write(indent);
                    console.Write(new Text("}", jsonStyle.CurlyBracketStyle));
                }
                else
                {
                    console.Write(new Text("{}", jsonStyle.CurlyBracketStyle));
                }

                break;

            case JsonValueKind.Array:
                var indentArray = new string(' ', (indentionLevel + 1) * jsonStyle.IndentSize);
                var firstPropertyWritten2 = false;
                foreach (var child in node.EnumerateArray())
                {
                    console.Write(!firstPropertyWritten2
                        ? new Text("[", jsonStyle.SquareBracketStyle)
                        : new Text(",", jsonStyle.ValueSeparatorStyle));
                    console.WriteLine();
                    console.Write(indentArray);
                    console.WriteJson(child, jsonStyle, indentionLevel + 1);
                    firstPropertyWritten2 = true;
                }

                if (firstPropertyWritten2)
                {
                    console.WriteLine();
                    console.Write(indentArray[..^2]);
                    console.Write(new Text("]", jsonStyle.SquareBracketStyle));
                }
                else
                {
                    console.Write(new Text("[]", jsonStyle.SquareBracketStyle));
                }

                break;

            case JsonValueKind.String:
                console.Write(new Text("\"" + node.GetString() + "\"", jsonStyle.StringStyle));
                break;

            case JsonValueKind.Number:
                console.Write(new Text(node.GetRawText(), jsonStyle.NumberStyle));
                break;

            case JsonValueKind.True:
                console.Write(new Text("true", jsonStyle.BooleanStyle));
                break;

            case JsonValueKind.False:
                console.Write(new Text("false", jsonStyle.BooleanStyle));
                break;

            case JsonValueKind.Null:
                console.Write(new Text("null", jsonStyle.NullStyle));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(node.ValueKind));
        }

        if (indentionLevel == 0)
            console.WriteLine();

        return console;
    }
}

public record JsonStyle
{
    public static readonly JsonStyle Default = new();

    public int IndentSize { get; init; } = 2;

    public Style NameStyle { get; init; } = new(Color.LightSkyBlue1);
    public Style StringStyle { get; init; } = new(Color.LightPink3);
    public Style NumberStyle { get; init; } = new(Color.DarkSeaGreen2);
    public Style NullStyle { get; init; } = new(Color.SkyBlue3);
    public Style BooleanStyle { get; init; } = new(Color.SkyBlue3);
    public Style CurlyBracketStyle { get; init; } = new(Color.Grey82);
    public Style SquareBracketStyle { get; init; } = new(Color.Grey82);
    public Style NameSeparatorStyle { get; init; } = new(Color.Grey82);
    public Style ValueSeparatorStyle { get; init; } = new(Color.Grey82);
}