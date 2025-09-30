namespace Koalas.Text;

public partial class TextBuilder : ITextBuilder
{
    public int IndentSize { get; private set; }

    private readonly List<IRender> _children = [];
    private readonly int _defaultIndentSize;
    private readonly List<int> _indentStack = [];

    private TextBuilder(int indentSize)
    {
        _defaultIndentSize = indentSize;
    }

    public TextBuilder Add(IRender model)
    {
        if (IndentSize > 0)
        {
            _children.Add(new TextRegionModel(Children: [model],
                                              IndentSize: IndentSize));

            return this;
        }

        _children.Add(model);

        return this;
    }

    public TextBuilder AddBlankLine()
        => Add(new TextLineModel(string.Empty));

    public TextBuilder AddBlankLine(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            AddBlankLine();
        }

        return this;
    }

    public TextBuilder AddFieldSet(IDictionary<string, string> items,
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
        TextFieldSetBuilder fieldSetBuilder = StartFieldSet(minLabelWidth: minLabelWidth,
                                                            minValueWidth: minValueWidth,
                                                            maxValueWidth: maxValueWidth,
                                                            fieldSeparator: fieldSeparator,
                                                            labelRightPadding: labelRightPadding,
                                                            valueLeftPadding: valueLeftPadding,
                                                            labelRightAlign: labelRightAlign,
                                                            valueRightAlign: valueRightAlign,
                                                            valueOverflowIndent: valueOverflowIndent);

        foreach (KeyValuePair<string, string> item in items)
        {
            fieldSetBuilder.AddField(item.Key, item.Value);
        }

        fieldSetBuilder.SaveFieldSet();

        return this;
    }

    public TextBuilder AddHangSection(string heading,
                                      string body,
                                      int? maxWidth = null,
                                      int trailingBlankLines = 1)
        => StartHangSection().Heading(heading)
                             .AddLine(body.ToWrapString(maxWidth))
                             .AddBlankLine(trailingBlankLines)
                             .SaveHangSection();

    public TextBuilder AddHangSection(string heading,
                                      ITextBuilder body,
                                      int? maxWidth = null,
                                      int trailingBlankLines = 1)
        => StartHangSection().Heading(heading)
                             .Add(body)
                             .AddBlankLine(trailingBlankLines)
                             .SaveHangSection();

    public TextBuilder AddHangSection(string heading,
                                      object body,
                                      int? maxWidth = null,
                                      int trailingBlankLines = 1)
        => StartHangSection().Heading(heading)
                             .AddLine(body.Render())
                             .AddBlankLine(trailingBlankLines)
                             .SaveHangSection();

    public TextBuilder AddHeading(string text, string prefix = "", string suffix = "")
    {
        AddLine($"{prefix}{text}{suffix}");
        AddLine($"{new string(' ', prefix.Length)}{new string('\u2500', text.Length)}");

        return this;
    }

    public TextBuilder AddLine(string text)
        => Add(new TextLineModel(Text: text?.TrimEnd() ?? string.Empty));

    public TextBuilder AddLine(object source)
        => AddLine(source.Render());

    public TextBuilder AddList(IEnumerable<string> items, string separator = ":")
    {
        TextListBuilder listBuilder = StartList(separator: separator);

        foreach (string item in items)
        {
            listBuilder.AddItem(item);
        }

        listBuilder.SaveList();

        return this;
    }

    public TextBuilder AddOptionalHangSection(string heading,
                                              string body,
                                              int? maxWidth = null,
                                              int trailingBlankLines = 1)
    {
        if (string.IsNullOrEmpty(body))
        {
            return this;
        }

        return AddHangSection(heading: heading,
                              body: body,
                              maxWidth: maxWidth,
                              trailingBlankLines: trailingBlankLines);
    }

    public TextBuilder AddOptionalLine(string content, int trailingBlankLines = 0)
        => string.IsNullOrEmpty(content)
               ? this
               : AddLine(content).AddBlankLine(trailingBlankLines);

    public TextBuilder AddOptionalLine(IRender source, int trailingBlankLines = 0)
        => AddOptionalLine(source.Render(), trailingBlankLines: trailingBlankLines);

    public TextBuilder AddOptionalLine(object source, int trailingBlankLines = 0)
        => AddOptionalLine(source.Render(), trailingBlankLines: trailingBlankLines);

    public TextBuilder AddOptionalSection(string heading,
                                          string body,
                                          string headingSuffix = "",
                                          int? maxWidth = null,
                                          int trailingBlankLines = 2)
    {
        if (string.IsNullOrEmpty(body))
        {
            return this;
        }

        return AddSection(heading: heading,
                          body: body,
                          headingSuffix: headingSuffix,
                          maxWidth: maxWidth,
                          trailingBlankLines: trailingBlankLines);
    }

    public TextBuilder AddSection(string heading,
                                  string body,
                                  string headingSuffix = "",
                                  int? maxWidth = null,
                                  int trailingBlankLines = 2)
        => StartSection().Heading(heading)
                         .HeadingSuffix(headingSuffix)
                         .AddLine(body.ToWrapString(maxWidth))
                         .AddBlankLine(trailingBlankLines)
                         .SaveSection();

    public TextBuilder AddSection(string heading,
                                  ITextBuilder body,
                                  string headingSuffix = "",
                                  int? maxWidth = null,
                                  int trailingBlankLines = 2)
        => StartSection().Heading(heading)
                         .HeadingSuffix(headingSuffix)
                         .Add(body)
                         .AddBlankLine(trailingBlankLines)
                         .SaveSection();

    public TextBuilder AddSection(string heading,
                                  object body,
                                  string headingSuffix = "",
                                  int? maxWidth = null,
                                  int trailingBlankLines = 2)
        => StartSection().Heading(heading)
                         .HeadingSuffix(headingSuffix)
                         .AddLine(body.Render())
                         .AddBlankLine(trailingBlankLines)
                         .SaveSection();

    public TextBuilder AddTable(IEnumerable<IEnumerable<object>> values,
                                TextTableBorder border = TextTableBorder.Inner,
                                IEnumerable<string> columnNames = null,
                                int defaultColumnPadding = 1,
                                int? defaultColumnMaxWidth = 50,
                                Func<object, string> formatCellValue = null,
                                bool includeIdentityColumn = false)
        => Add(TextTableBuilder.Create(values: values,
                                       border: border,
                                       columnNames: columnNames,
                                       defaultColumnPadding: defaultColumnPadding,
                                       defaultColumnMaxWidth: defaultColumnMaxWidth,
                                       formatCellValue: formatCellValue,
                                       includeIdentityColumn: includeIdentityColumn));

    public TextBuilder AddTitle(string text)
    {
        AddLine(text);

        int length = text.Length;
        if (text.Contains("\n"))
        {
            length = text.Lines()
                         .Max(t => t.Length);
        }

        AddLine(new string('═', length));

        return this;
    }

    public IRender Build()
        => new TextRegionModel(Children: _children,
                               IndentSize: 0);

    public TextBuilder ClearIndent()
    {
        _indentStack.Clear();
        IndentSize = 0;

        return this;
    }

    public static TextBuilder Create(int indentSize = 2)
        => new(indentSize: indentSize);

    public TextBuilder PopIndent(int count = 1)
    {
        if (count <= 0)
        {
            throw new ArgumentException("count must be greater than zero.", nameof(count));
        }

        for (int i = 1; i <= count; i++)
        {
            _indentStack.RemoveAt(_indentStack.Count - 1);
        }

        IndentSize = _indentStack.Sum();

        return this;
    }

    public TextBuilder PushIndent(int? size = null, int count = 1)
    {
        if (size <= 0)
        {
            throw new ArgumentException("size must be greater than zero.", nameof(size));
        }

        if (count <= 0)
        {
            throw new ArgumentException("count must be greater than zero.", nameof(count));
        }

        int indentSize = size ?? _defaultIndentSize;
        for (int i = 1; i <= count; i++)
        {
            _indentStack.Add(indentSize);
        }

        IndentSize = _indentStack.Sum();

        return this;
    }

    public string Render()
        => Build().Render();

    public TextFieldSetBuilder StartFieldSet(int minLabelWidth = 0,
                                             int minValueWidth = 0,
                                             int maxValueWidth = 1000,
                                             string fieldSeparator = ":",
                                             int labelRightPadding = 1,
                                             int valueLeftPadding = 1,
                                             bool labelRightAlign = false,
                                             bool valueRightAlign = false,
                                             int valueOverflowIndent = 0)
        => new TextFieldSetBuilder(this).MinLabelWidth(minLabelWidth)
                                        .MinValueWidth(minValueWidth)
                                        .MaxValueWidth(maxValueWidth)
                                        .FieldSeparator(fieldSeparator)
                                        .LabelRightPadding(labelRightPadding)
                                        .ValueLeftPadding(valueLeftPadding)
                                        .LabelRightAlign(labelRightAlign)
                                        .ValueRightAlign(valueRightAlign)
                                        .ValueOverflowIndent(valueOverflowIndent);

    public TextHangSectionBuilder StartHangSection(string heading = null)
        => new TextHangSectionBuilder(this).Heading(heading);

    public TextListBuilder StartList(string separator = ":", int defaultTrailingBlankLines = 0)
        => new TextListBuilder(this).Separator(separator)
                                    .DefaultTrailingBlankLines(defaultTrailingBlankLines);

    public TextSectionBuilder StartSection(string heading = null)
        => new TextSectionBuilder(this).Heading(heading);

    public TextTableBuilder StartTable(TextTableBorder border = TextTableBorder.Default,
                                       int defaultColumnPadding = 1,
                                       int? defaultColumnMaxWidth = null)
        => new TextTableBuilder(this).Border(border)
                                     .DefaultColumnPadding(defaultColumnPadding)
                                     .DefaultColumnMaxWidth(defaultColumnMaxWidth);

    public override string ToString()
        => Render();
}
