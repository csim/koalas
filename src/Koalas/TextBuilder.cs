namespace Koalas;

using System;
using System.Text;
using System.Text.RegularExpressions;
using Koalas.Extensions;

public class TextBuilder
{
    private TextBuilder(int indentSize = 4)
    {
        _indentSize = indentSize;
    }

    private const char _underline = '\u2500';
    private static readonly Regex _indentRegex = new(@"\r\n|\n|\r",
                                                     RegexOptions.Compiled
                                                     | RegexOptions.CultureInvariant
                                                     | RegexOptions.Singleline
                                                     | RegexOptions.ExplicitCapture);
    private readonly int _indentSize;
    private readonly StringBuilder _output = new();

    public int IndentLevel { get; private set; }

    public TextBuilder Add(string text)
    {
        _output.Append(Indent(text ?? string.Empty));

        return this;
    }

    public TextBuilder AddBlankLine()
    {
        AddLine(string.Empty);

        return this;
    }

    public TextBuilder AddBlankLine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddBlankLine();
        }

        return this;
    }

    public TextBuilder AddHeading(string text, string prefix = "")
    {
        if (string.IsNullOrEmpty(prefix))
        {
            AddLine(text);
            AddLine(new string(_underline, text.Length));

            return this;
        }

        AddLine(prefix + text);
        AddLine(new string(' ', prefix.Length) + new string(_underline, text.Length));

        return this;
    }

    public TextBuilder AddLine(string text)
    {
        _output.AppendLine(Indent(text ?? string.Empty));

        return this;
    }

    public TextBuilder AddSection(string heading,
                                  string body,
                                  string headingPrefix = "",
                                  int trailingBlankLines = 2,
                                  int? maxWidth = null,
                                  bool showWrapGlyph = false)
    {
        body ??= "<null>";

        if (body == string.Empty) body = "<empty>";

        if (maxWidth != null && body.Length > maxWidth.Value) body = string.Join(Environment.NewLine, body.Wrap(maxWidth.Value, showWrapGlyph));

        AddHeading(heading, headingPrefix);
        AddLine(body.TrimEnd());
        AddBlankLine(trailingBlankLines);

        return this;
    }

    public TextBuilder AddTitle(string text)
    {
        AddLine(text);
        AddLine(new string('═', text.Length));

        return this;
    }

    public static TextBuilder Create(int indentSize = 4)
    {
        return new TextBuilder(indentSize);
    }

    public TextBuilder PopIndent(int count = 1)
    {
        if (IndentLevel == 0) throw new Exception("Indent is at zero.");

        IndentLevel -= count;
        if (IndentLevel < 0) throw new Exception("Indent below zero.");

        return this;
    }

    public TextBuilder PushIndent(int count = 1)
    {
        IndentLevel += count;

        return this;
    }

    public string Render()
    {
        return _output.ToString();
    }

    public override string ToString()
    {
        return Render();
    }

    private string Indent(string subject)
    {
        if (string.IsNullOrEmpty(subject)) return subject ?? string.Empty;

        string indent = new(' ', IndentLevel * _indentSize);
        string indentedSubject = _indentRegex.Replace(subject, $"{Environment.NewLine}{indent}");

        return $"{indent}{indentedSubject}";
    }
}