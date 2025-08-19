namespace Koalas.Text;

public partial class TextSectionBuilder : ITextBuilder
{
    internal TextSectionBuilder(TextBuilder parent)
    {
        _parent = parent;
        _bodyBuilder = TextBuilder.Create(indentSize: parent.IndentSize);
    }

    private readonly TextBuilder _bodyBuilder;
    private string _heading = string.Empty;
    private string _headingSuffix = string.Empty;
    private readonly TextBuilder _parent;
    private bool _saved;

    public TextSectionBuilder Add(ITextBuilder subject)
    {
        _bodyBuilder.Add(subject);

        return this;
    }

    public IRender Build()
    {
        return new TextSectionModel(Heading: _heading,
                                    Body: _bodyBuilder.Build(),
                                    HeadingSuffix: _headingSuffix);
    }

    public static TextSectionBuilder Create(int indentSize = 2)
    {
        return new TextSectionBuilder(TextBuilder.Create(indentSize: 2));
    }

    public string Render()
    {
        return Build().Render();
    }

    public TextBuilder SaveSection()
    {
        if (_saved) throw new Exception($"Cannot {nameof(SaveSection)}, {nameof(TextSectionBuilder)} already saved.");

        _saved = true;

        return _parent.Add(this);
    }

    public override string ToString()
    {
        return Render();
    }
}

public partial class TextSectionBuilder
{
    public TextSectionBuilder Heading(string heading)
    {
        _heading = heading;

        return this;
    }

    public TextSectionBuilder HeadingSuffix(string headingSuffix)
    {
        _headingSuffix = headingSuffix;

        return this;
    }
}

public partial class TextSectionBuilder
{
    public TextSectionBuilder AddBlankLine()
    {
        _bodyBuilder.AddBlankLine();
        return this;
    }

    public TextSectionBuilder AddBlankLine(int count)
    {
        _bodyBuilder.AddBlankLine(count);
        return this;
    }

    public TextSectionBuilder AddFieldSet(IDictionary<string, string> items,
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

    public TextSectionBuilder AddHangSection(string heading,
                                             string body,
                                             int? maxWidth = null,
                                             int trailingBlankLines = 1)
    {
        _bodyBuilder.AddHangSection(heading, body, maxWidth: maxWidth, trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextSectionBuilder AddHangSection(string heading,
                                             ITextBuilder body,
                                             int? maxWidth = null,
                                             int trailingBlankLines = 1)
    {
        _bodyBuilder.AddHangSection(heading, body, maxWidth: maxWidth, trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextSectionBuilder AddLine(string text)
    {
        _bodyBuilder.AddLine(text);

        return this;
    }

    public TextSectionBuilder AddLine(object source)
    {
        return AddLine(source.Render());
    }

    public TextSectionBuilder AddList(IEnumerable<string> items, string separator = ":")
    {
        _bodyBuilder.AddList(items, separator);

        return this;
    }

    public TextSectionBuilder AddTable(IEnumerable<IEnumerable<object>> values,
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

    public TextSectionBuilder ClearIndent()
    {
        _bodyBuilder.ClearIndent();

        return this;
    }

    public TextSectionBuilder PopIndent(int count = 1)
    {
        _bodyBuilder.PopIndent(count);

        return this;
    }

    public TextSectionBuilder PushIndent(int? size = null, int count = 1)
    {
        _bodyBuilder.PushIndent(size, count);

        return this;
    }
}