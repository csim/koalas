namespace Koalas.Text;

public partial class TextFieldSetItemBuilder : IRender
{
    internal TextFieldSetItemBuilder(TextFieldSetBuilder parent)
    {
        _parent = parent;
    }

    private string _format;
    private string _label;
    private readonly TextFieldSetBuilder _parent;
    private bool _saved;
    private TextBuilder _valueBuilder = TextBuilder.Create();

    public TextBuilder Add(IRender subject)
        => _valueBuilder.Add(subject);

    public string Render()
    {
        if (!_saved)
        {
            SaveField();
        }

        return _parent.Render();
    }

    public TextFieldSetBuilder SaveField()
    {
        if (_saved)
        {
            throw new Exception($"Cannot {nameof(SaveField)}, {nameof(TextFieldSetItemBuilder)} already saved.");
        }

        _saved = true;

        return _parent.AddField(Build());
    }

    public override string ToString()
        => Render();

    private TextFieldModel Build()
        => new(Label: _label,
               Value: _valueBuilder.Build(),
               Format: _format);
}

public partial class TextFieldSetItemBuilder
{
    public TextFieldSetItemBuilder Format(string format)
    {
        _format = format;

        return this;
    }

    public TextFieldSetItemBuilder Label(string label)
    {
        _label = label;

        return this;
    }

    public TextFieldSetItemBuilder Value(string value)
    {
        _valueBuilder = TextBuilder.Create()
                                   .AddLine(value);

        return this;
    }

    public TextFieldSetItemBuilder Value(ITextBuilder builder)
    {
        _valueBuilder = TextBuilder.Create()
                                   .Add(builder);

        return this;
    }
}

public partial class TextFieldSetItemBuilder
{
    public TextFieldSetItemBuilder AddBlankLine()
    {
        _valueBuilder.AddBlankLine();

        return this;
    }

    public TextFieldSetItemBuilder AddBlankLine(int count)
    {
        _valueBuilder.AddBlankLine(count);

        return this;
    }

    public TextFieldSetItemBuilder AddFieldSet(IDictionary<string, string> items,
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
        _valueBuilder.StartFieldSet(minLabelWidth: minLabelWidth,
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

    public TextFieldSetItemBuilder AddHangSection(string heading,
                                                  string body,
                                                  int? maxWidth = null,
                                                  int trailingBlankLines = 1)
    {
        _valueBuilder.AddHangSection(heading, body, maxWidth: maxWidth, trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextFieldSetItemBuilder AddHangSection(string heading,
                                                  ITextBuilder body,
                                                  int? maxWidth = null,
                                                  int trailingBlankLines = 1)
    {
        _valueBuilder.AddHangSection(heading, body, maxWidth: maxWidth, trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextFieldSetItemBuilder AddHangSection(string heading,
                                                  object body,
                                                  int? maxWidth = null,
                                                  int trailingBlankLines = 1)
    {
        _valueBuilder.AddHangSection(heading, body, maxWidth: maxWidth, trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextFieldSetItemBuilder AddLine(string text)
    {
        _valueBuilder.AddLine(text);

        return this;
    }

    public TextFieldSetItemBuilder AddLine(object source)
        => AddLine(source.Render());

    public TextFieldSetItemBuilder AddList(IEnumerable<string> items, string separator = ":")
    {
        _valueBuilder.AddList(items, separator);

        return this;
    }

    public TextFieldSetItemBuilder AddSection(string heading,
                                              string body,
                                              string headingSuffix = "",
                                              int? maxWidth = null,
                                              int trailingBlankLines = 2)
    {
        _valueBuilder.AddSection(heading: heading,
                                 body: body,
                                 headingSuffix: headingSuffix,
                                 maxWidth: maxWidth,
                                 trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextFieldSetItemBuilder AddSection(string heading,
                                              object body,
                                              string headingSuffix = "",
                                              int? maxWidth = null,
                                              int trailingBlankLines = 2)
    {
        _valueBuilder.AddSection(heading: heading,
                                 body: body,
                                 headingSuffix: headingSuffix,
                                 maxWidth: maxWidth,
                                 trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextFieldSetItemBuilder AddSection(string heading,
                                              ITextBuilder body,
                                              string headingSuffix = "",
                                              int? maxWidth = null,
                                              int trailingBlankLines = 2)
    {
        _valueBuilder.AddSection(heading: heading,
                                 body: body,
                                 headingSuffix: headingSuffix,
                                 maxWidth: maxWidth,
                                 trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextFieldSetItemBuilder AddTable(IEnumerable<IEnumerable<object>> values,
                                            TextTableBorder border = TextTableBorder.Inner,
                                            IEnumerable<string> columnNames = null,
                                            int defaultColumnPadding = 1,
                                            int? defaultColumnMaxWidth = 50,
                                            Func<object, string> formatCellValue = null,
                                            bool includeIdentityColumn = false)
    {
        _valueBuilder.Add(TextTableBuilder.Create(values: values,
                                                  border: border,
                                                  columnNames: columnNames,
                                                  defaultColumnPadding: defaultColumnPadding,
                                                  defaultColumnMaxWidth: defaultColumnMaxWidth,
                                                  formatCellValue: formatCellValue,
                                                  includeIdentityColumn: includeIdentityColumn));
        return this;
    }

    public TextFieldSetItemBuilder ClearIndent()
    {
        _valueBuilder.ClearIndent();

        return this;
    }

    public TextFieldSetItemBuilder PopIndent(int count = 1)
    {
        _valueBuilder.PopIndent(count);

        return this;
    }

    public TextFieldSetItemBuilder PushIndent(int? size = null, int count = 1)
    {
        _valueBuilder.PushIndent(size, count);

        return this;
    }
}