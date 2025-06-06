﻿namespace Koalas.Text;

public partial class TextListItemBuilder : IRender
{
    internal TextListItemBuilder(TextListBuilder parent)
    {
        _parent = parent;
    }

    private readonly TextBuilder _bodyBuilder = TextBuilder.Create();
    private string _id;
    private string _indicator;
    private readonly TextListBuilder _parent;
    private bool _saved;

    public TextListItemBuilder Add(ITextBuilder subject)
    {
        _bodyBuilder.Add(subject);

        return this;
    }

    public string Render()
    {
        if (!_saved) SaveItem();

        return _parent.Render();
    }

    public TextListBuilder SaveItem()
    {
        if (_saved) throw new Exception($"Cannot {nameof(SaveItem)}, {nameof(TextListItemBuilder)} already saved.");

        _saved = true;

        return _parent.AddItem(Build());
    }

    public override string ToString()
    {
        return Render();
    }

    private TextListItemModel Build()
    {
        return new TextListItemModel(Body: _bodyBuilder.Build(),
                                     Id: _id,
                                     Indicator: _indicator);
    }
}

public partial class TextListItemBuilder
{
    public TextListItemBuilder Id(string id)
    {
        _id = id;

        return this;
    }

    public TextListItemBuilder Indicator(string indicator)
    {
        _indicator = indicator;

        return this;
    }
}

public partial class TextListItemBuilder
{
    public TextListItemBuilder AddBlankLine()
    {
        _bodyBuilder.AddBlankLine();
        return this;
    }

    public TextListItemBuilder AddBlankLine(int count)
    {
        _bodyBuilder.AddBlankLine(count);

        return this;
    }

    public TextListItemBuilder AddFieldSet(IDictionary<string, string> items,
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

    public TextListItemBuilder AddHangSection(string heading,
                                              string body,
                                              int? maxWidth = null,
                                              int trailingBlankLines = 1)
    {
        _bodyBuilder.AddHangSection(heading, body, maxWidth: maxWidth, trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextListItemBuilder AddHangSection(string heading,
                                              ITextBuilder body,
                                              int? maxWidth = null,
                                              int trailingBlankLines = 1)
    {
        _bodyBuilder.AddHangSection(heading, body, maxWidth: maxWidth, trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextListItemBuilder AddLine(string text)
    {
        _bodyBuilder.AddLine(text: text);

        return this;
    }

    public TextListItemBuilder AddLine(Action<TextBuilder> bodyFactory)
    {
        _bodyBuilder.AddLine(bodyFactory);

        return this;
    }

    public TextListItemBuilder AddList(IEnumerable<string> items, string separator = ":")
    {
        _bodyBuilder.AddList(items, separator);

        return this;
    }

    public TextListItemBuilder AddSection(string heading,
                                          string body,
                                          string headingSuffix = "",
                                          int? maxWidth = null,
                                          int trailingBlankLines = 2)
    {
        _bodyBuilder.AddSection(heading: heading,
                                body: body,
                                headingSuffix: headingSuffix,
                                maxWidth: maxWidth,
                                trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextListItemBuilder AddSection(string heading,
                                          ITextBuilder body,
                                          string headingSuffix = "",
                                          int? maxWidth = null,
                                          int trailingBlankLines = 2)
    {
        _bodyBuilder.AddSection(heading: heading,
                                body: body,
                                headingSuffix: headingSuffix,
                                maxWidth: maxWidth,
                                trailingBlankLines: trailingBlankLines);

        return this;
    }

    public TextListItemBuilder AddTable(IEnumerable<IEnumerable<object>> values,
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

    public TextListItemBuilder ClearIndent()
    {
        _bodyBuilder.ClearIndent();

        return this;
    }

    public TextListItemBuilder PopIndent(int count = 1)
    {
        _bodyBuilder.PopIndent(count);

        return this;
    }

    public TextListItemBuilder PushIndent(int? size = null, int count = 1)
    {
        _bodyBuilder.PushIndent(size, count);

        return this;
    }
}