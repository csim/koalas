namespace Koalas.Text;

public partial class TextListBuilder : ITextBuilder
{
    internal TextListBuilder(TextBuilder parent)
    {
        _parent = parent;
    }

    private readonly List<TextListItemModel> _items = [];
    private readonly TextBuilder _parent;
    private bool _saved;

    public IRender Build()
    {
        return new TextListModel(Items: _items);
    }

    public static TextListBuilder Create(string separator = ":", int indentSize = 2, int defaultTrailingBlankLines = 0)
    {
        return new TextListBuilder(TextBuilder.Create(indentSize: indentSize)).DefaultTrailingBlankLines(defaultTrailingBlankLines)
                                                                              .Separator(separator);
    }

    public string Render()
    {
        return Build().Render();
    }

    public TextBuilder SaveList()
    {
        if (_saved) throw new Exception($"Cannot {nameof(SaveList)}, {nameof(TextListBuilder)} already saved.");

        _saved = true;

        return _items.Count > 0
                   ? _parent.Add(this)
                   : _parent;
    }

    public TextListItemBuilder StartItem(string id = null, string indicator = null, string separator = ":")
    {
        return new TextListItemBuilder(this).Id(id)
                                            .Indicator(indicator)
                                            .Separator(separator);
    }

    public override string ToString()
    {
        return Render();
    }
}

public partial class TextListBuilder
{
    private int _defaultTrailingBlankLines;
    private string _separator;

    public TextListBuilder DefaultTrailingBlankLines(int value)
    {
        _defaultTrailingBlankLines = value;

        return this;
    }

    public TextListBuilder Separator(string separator)
    {
        _separator = separator;

        return this;
    }
}

public partial class TextListBuilder
{
    public TextListBuilder AddItem(TextListItemModel model)
    {
        _items.Add(model);

        return this;
    }

    public TextListBuilder AddItem(TextSectionBuilder builder,
                                   string id = null,
                                   string indicator = null,
                                   string separator = null,
                                   int? trailingBlankLines = null)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .Separator(separator ?? _separator)
                          .Add(builder)
                          .AddBlankLine(trailingBlankLines ?? _defaultTrailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(TextHangSectionBuilder builder,
                                   string id = null,
                                   string indicator = null,
                                   string separator = null,
                                   int? trailingBlankLines = null)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .Separator(separator ?? _separator)
                          .Add(builder)
                          .AddBlankLine(trailingBlankLines ?? _defaultTrailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(TextListBuilder builder, string id = null, string indicator = null, string separator = null, int? trailingBlankLines = null)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .Add(builder)
                          .AddBlankLine(trailingBlankLines ?? _defaultTrailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(TextFieldSetBuilder builder,
                                   string id = null,
                                   string indicator = null,
                                   string separator = null,
                                   int? trailingBlankLines = null)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .Separator(separator ?? _separator)
                          .Add(builder)
                          .AddBlankLine(trailingBlankLines ?? _defaultTrailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(TextTableBuilder builder, string id = null, string indicator = null, string separator = null, int? trailingBlankLines = null)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .Separator(separator ?? _separator)
                          .AddLine(builder.Render())
                          .AddBlankLine(trailingBlankLines ?? _defaultTrailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(string body, string id = null, string indicator = null, string separator = null, int? trailingBlankLines = null)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .Separator(separator ?? _separator)
                          .AddLine(body)
                          .AddBlankLine(trailingBlankLines ?? _defaultTrailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(ITextBuilder body, string id = null, string indicator = null, string separator = null, int? trailingBlankLines = null)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .Separator(separator ?? _separator)
                          .Add(body)
                          .AddBlankLine(trailingBlankLines ?? _defaultTrailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(Action<TextBuilder> bodyFactory,
                                   string id = null,
                                   string indicator = null,
                                   string separator = null)
    {
        TextBuilder bodyBuilder = TextBuilder.Create();
        bodyFactory(bodyBuilder);

        AddItem(new TextListItemModel(Indicator: indicator,
                                      Id: id,
                                      Separator: separator,
                                      Body: bodyBuilder.Build()));

        return this;
    }
}