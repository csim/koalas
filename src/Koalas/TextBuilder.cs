namespace Koalas;

using System;
using System.Text;

public class TextBuilder {
    private TextBuilder(int indentSize = 4) {
        _indentSize = indentSize;
    }

    private readonly int _indentSize;
    private readonly StringBuilder _output = new();

    public int IndentLevel { get; private set; }

    public TextBuilder Add(string text) {
        if (IndentLevel > 0 && text.Contains(Environment.NewLine)) {
            string indent = new(' ', IndentLevel * _indentSize);
            text = $"{indent}{text.Replace(Environment.NewLine, $"{Environment.NewLine}{indent}")}";
        }

        _output.Append(text);

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
        AddLine(new string('‾', text.Length));

        return this;
    }

    public TextBuilder AddLine(string text) {
        if (IndentLevel > 0) {
            string indent = new(' ', IndentLevel * _indentSize);
            text = $"{indent}{text.Replace(Environment.NewLine, $"{Environment.NewLine}{indent}")}";
        }

        if (text.EndsWith(Environment.NewLine)) {
            _output.Append(text);
        }
        else {
            _output.AppendLine(text);
        }

        return this;
    }

    public TextBuilder AddSection(string heading, string body, int trailingBlankLines = 2) {
        if (body == null) {
            return this;
        }

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
        return new TextBuilder(indentSize);
    }

    public TextBuilder PopIndent() {
        if (IndentLevel == 0) {
            throw new Exception("Indent is at zero.");
        }

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
}

public class TextFieldSetBuilder {
    private TextFieldSetBuilder(int minLabelWidth = 0, int minValueWidth = 0) {
        _table = new TextTableBuilder().AddColumn(minWidth: minLabelWidth, leftPadding: 0)
                                       .AddStaticColumn(":")
                                       .AddColumn(minWidth: minValueWidth, rightPadding: 0);
    }

    private readonly TextTableBuilder _table;

    public TextFieldSetBuilder AddField(string label, bool? value) {
        return value == null
                   ? this
                   : AddField(label, (object)value);
    }

    public TextFieldSetBuilder AddField(string label, int? value, string format = "N0") {
        return value == null
                   ? this
                   : AddField(label, (object)value, format);
    }

    public TextFieldSetBuilder AddField(string label, uint? value, string format = "N0") {
        return value == null
                   ? this
                   : AddField(label, (object)value, format);
    }

    public TextFieldSetBuilder AddField(string label, double? value, string format = "N3") {
        return value == null
                   ? this
                   : AddField(label, (object)value, format);
    }

    public TextFieldSetBuilder AddField(string label, object value, string format = null) {
        if (value == null) {
            return this;
        }

        format ??= value switch {
                       int    => "N0",
                       double => "N3",
                       _      => null
                   };


        string result = format == null
                            ? value.ToString()
                            : string.Format($"{{0:{format}}}", value);

        result = result.TrimEnd();
        if (string.IsNullOrEmpty(result)) {
            return this;
        }

        _table.AddDataRow(label, result);

        return this;
    }

    public static TextFieldSetBuilder Create(int minLabelWidth = 0, int minValueWidth = 0) {
        return new TextFieldSetBuilder(minLabelWidth, minValueWidth);
    }

    public string Render() {
        return _table.Render();
    }

    public override string ToString() {
        return Render();
    }
}