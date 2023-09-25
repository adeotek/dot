using System.Text;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace Adeotek.DevOpsTools.Common;

internal sealed class CustomComposer : IRenderable
{
    private readonly StringBuilder _content = new();

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        return ((IRenderable)new Markup(_content.ToString())).Measure(options, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        return ((IRenderable)new Markup(_content.ToString())).Render(options, maxWidth);
    }

    public CustomComposer Text(string text)
    {
        _content.Append(text);
        return this;
    }
    
    public CustomComposer Text(string text, int length)
    {
        Text(text);
        if (text.Length < length)
        {
            Repeat(' ', length - text.Length);
        }
        return this;
    }

    public CustomComposer Style(string style, string text)
    {
        _content.Append('[').Append(style).Append(']');
        _content.Append(text.EscapeMarkup());
        _content.Append("[/]");
        return this;
    }

    public CustomComposer Style(string style, Action<CustomComposer> action)
    {
        _content.Append('[').Append(style).Append(']');
        action(this);
        _content.Append("[/]");
        return this;
    }
    
    public CustomComposer Style(string color, string text, int length)
    {
        Style(color, text);
        if (text.Length < length)
        {
            Repeat(' ', length - text.Length);
        }
        return this;
    }

    public CustomComposer Space()
    {
        return Spaces(1);
    }

    public CustomComposer Spaces(int count)
    {
        return Repeat(' ', count);
    }

    public CustomComposer Tab()
    {
        return Tabs(1);
    }

    public CustomComposer Tabs(int count)
    {
        return Spaces(count * 4);
    }

    public CustomComposer Repeat(char character, int count)
    {
        _content.Append(new string(character, count));
        return this;
    }
    
    public CustomComposer Repeat(string style, char character, int count)
    {
        _content.Append('[').Append(style).Append(']');
        _content.Append(new string(character, count));
        _content.Append("[/]");
        return this;
    }

    public CustomComposer LineBreak()
    {
        return LineBreaks(1);
    }

    public CustomComposer LineBreaks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _content.Append(Environment.NewLine);
        }

        return this;
    }

    public CustomComposer Join(string separator, IEnumerable<string> composers)
    {
        Space();
        Text(string.Join(separator, composers));
        return this;
    }

    public override string ToString()
    {
        return _content.ToString();
    }
}