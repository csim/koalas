namespace Koalas.Text;

public partial class TextHangSectionBuilder : ITextBuilder
{
    internal TextHangSectionBuilder(TextBuilder parent)
    {
        _parent = parent;
        _bodyBuilder = TextBuilder.Create(indentSize: parent.IndentSize);
    }

    private readonly TextBuilder _bodyBuilder;
    private string _heading = string.Empty;
    private readonly TextBuilder _parent;
    private bool _saved;

    public IRender Build()
    {
        return new TextHangSectionModel(Heading: _heading,
                                        Body: _bodyBuilder.Build());
    }

    public static TextHangSectionBuilder Create(int indentSize = 2)
    {
        return new TextHangSectionBuilder(TextBuilder.Create(indentSize: 2));
    }

    public string Render()
    {
        return Build().Render();
    }

    public TextBuilder SaveHangSection()
    {
        if (_saved) throw new Exception($"Cannot {nameof(SaveHangSection)}, {nameof(TextHangSectionBuilder)} already saved.");

        _saved = true;

        return _parent.Add(this);
    }

    public override string ToString()
    {
        return Render();
    }
}

public partial class TextHangSectionBuilder
{
    public TextHangSectionBuilder Heading(string heading)
    {
        _heading = heading;

        return this;
    }
}

public partial class TextHangSectionBuilder
{
    public TextHangSectionBuilder Add(ITextBuilder subject)
    {
        _bodyBuilder.Add(subject);

        return this;
    }

    public TextHangSectionBuilder AddBlankLine()
    {
        _bodyBuilder.AddBlankLine();

        return this;
    }

    public TextHangSectionBuilder AddBlankLine(int count)
    {
        _bodyBuilder.AddBlankLine(count);

        return this;
    }

    public TextHangSectionBuilder AddFieldSet(IDictionary<string, string> items,
                                              int minLabelWidth = 0,
                                              int minValueWidth = 0,
                                              int maxValueWidth = 1000,
                                              string fieldSeparator = ":",
                                              int labelRightPadding = 1,
                                              int valueLeftPadding = 1,
                                              bool labelRightAlign = false,
                                              bool valueRightAlign = false,
                                              int valueOverflowIndent = 0)
    {
        _bodyBuilder.StartFieldSet(minLabelWidth: minLabelWidth,
                                   minValueWidth: minValueWidth,
                                   maxValueWidth: maxValueWidth,
                                   fieldSeparator: fieldSeparator,
                                   labelRightPadding: labelRightPadding,
                                   valueLeftPadding: valueLeftPadding,
                                   labelRightAlign: labelRightAlign,
                                   valueRightAlign: valueRightAlign,
                                   valueOverflowIndent: valueOverflowIndent);

        return this;
    }

    public TextHangSectionBuilder AddHangSection(string heading,
                                                 string body,
                                                 int? maxWidth = null,
                                                 int trailingBlankLines = 1)
    {
        _bodyBuilder.AddHangSection(heading, body, maxWidth: maxWidth, trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextHangSectionBuilder AddHangSection(string heading,
                                                 ITextBuilder body,
                                                 int? maxWidth = null,
                                                 int trailingBlankLines = 1)
    {
        _bodyBuilder.AddHangSection(heading, body, maxWidth: maxWidth, trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextHangSectionBuilder AddLine(string text)
    {
        _bodyBuilder.AddLine(text);

        return this;
    }

    public TextHangSectionBuilder AddLine(object source)
    {
        return AddLine(source.Render());
    }

    public TextHangSectionBuilder AddList(IEnumerable<string> items, string separator = ":")
    {
        _bodyBuilder.AddList(items, separator);

        return this;
    }

    public TextHangSectionBuilder AddOptionalLine(string content, int trailingBlankLines = 0)
    {
        return string.IsNullOrEmpty(content)
                   ? this
                   : AddLine(content).AddBlankLine(trailingBlankLines);
    }

    public TextHangSectionBuilder AddOptionalLine(object source, int trailingBlankLines = 0)
    {
        return AddOptionalLine(source.Render(), trailingBlankLines: trailingBlankLines);
    }

    public TextHangSectionBuilder AddTable(IEnumerable<IEnumerable<object>> values,
                                           TextTableBorder border = TextTableBorder.Inner,
                                           IEnumerable<string> columnNames = null,
                                           int defaultColumnPadding = 1,
                                           int? defaultColumnMaxWidth = 50,
                                           Func<object, string> formatCellValue = null,
                                           bool includeIdentityColumn = false)
    {
        return Add(TextTableBuilder.Create(values: values,
                                           border: border,
                                           columnNames: columnNames,
                                           defaultColumnPadding: defaultColumnPadding,
                                           defaultColumnMaxWidth: defaultColumnMaxWidth,
                                           formatCellValue: formatCellValue,
                                           includeIdentityColumn: includeIdentityColumn));
    }

    public TextHangSectionBuilder ClearIndent()
    {
        _bodyBuilder.ClearIndent();

        return this;
    }

    public TextHangSectionBuilder PopIndent(int count = 1)
    {
        _bodyBuilder.PopIndent(count);

        return this;
    }

    public TextHangSectionBuilder PushIndent(int? size = null, int count = 1)
    {
        _bodyBuilder.PushIndent(size, count);

        return this;
    }
}

public partial class TextHangSectionBuilder
{ }