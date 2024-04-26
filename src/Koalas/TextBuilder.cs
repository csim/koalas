namespace Koalas;
using System;
using System.Text;

public class TextBuilder {
    private TextBuilder(int indentSize = 4) {
        _indentSize = indentSize;
    }

    private const char _underline = '\u2500';
    private readonly int _indentSize;
    private readonly StringBuilder _output = new();

    public int IndentLevel { get; private set; }

    public TextBuilder Add(string text) {
        _output.Append(AddIndent(text ?? string.Empty));

        return this;
    }

    public TextBuilder AddBlankLine() {
        AddLine(string.Empty);

        return this;
    }

    public TextBuilder AddBlankLine(int count) {
        for (var i = 0; i < count; i++) {
            AddBlankLine();
        }

        return this;
    }

    public TextBuilder AddHeading(string text) {
        AddLine(text);
        AddLine(new string(_underline, text.Length));

        return this;
    }

    public TextBuilder AddLine(string text) {
        _output.AppendLine(AddIndent(text ?? string.Empty));

        return this;
    }

    public TextBuilder AddSection(string heading, string body, int trailingBlankLines = 2) {
        body ??= "<null>";

        if (body == string.Empty) body = "<empty>";

        AddHeading(heading);
        AddLine(body.TrimEnd());
        AddBlankLine(trailingBlankLines);

        return this;
    }

    public TextBuilder AddTitle(string text) {
        AddLine(text);
        AddLine(new string('═', text.Length));

        return this;
    }

    public static TextBuilder Create(int indentSize = 4) {
        return new(indentSize);
    }

    public TextBuilder PopIndent() {
        if (IndentLevel == 0) throw new Exception("Indent is at zero.");

        IndentLevel--;

        return this;
    }

    public TextBuilder PushIndent() {
        IndentLevel++;

        return this;
    }

    public string Render() {
        return _output.ToString();
    }

    public override string ToString() {
        return Render();
    }

    private string AddIndent(string text) {
        if (IndentLevel <= 0) return text;

        bool endsWithNewline = text.EndsWith(Environment.NewLine);
        if (endsWithNewline) text = text.TrimEnd('\r', '\n');

        string indent = new(' ', IndentLevel * _indentSize);
        text = $"{indent}{text.Replace(Environment.NewLine, $"{Environment.NewLine}{indent}")}";

        return text;
    }
}