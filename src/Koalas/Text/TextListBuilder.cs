namespace Koalas.Text;

public partial class TextListBuilder : IRender, ITextBuilder
{
    internal TextListBuilder(TextBuilder parent)
    {
        _parent = parent;
    }

    private readonly List<TextListItemModel> _items = [];
    private readonly TextBuilder _parent;
    private bool _saved;
    private string _separator = ":";

    public ITextModel Build()
    {
        return new TextListModel(Items: _items,
                                 Separator: _separator);
    }

    public string Render()
    {
        if (!_saved) SaveList();

        return _parent.Render();
    }

    public TextBuilder SaveList()
    {
        if (_saved) throw new Exception($"Cannot {nameof(SaveList)}, {nameof(TextListBuilder)} already saved.");

        _saved = true;

        return _parent.Add(this);
    }

    public TextListItemBuilder StartItem(string id = null, string indicator = null)
    {
        return new TextListItemBuilder(this).Id(id)
                                            .Indicator(indicator);
    }

    public override string ToString()
    {
        return Render();
    }
}

public partial class TextListBuilder
{
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

    public TextListBuilder AddItem(TextSectionBuilder builder, string id = null, string indicator = null, int trailingBlankLines = 0)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .Add(builder)
                          .AddBlankLine(trailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(TextHangSectionBuilder builder, string id = null, string indicator = null, int trailingBlankLines = 0)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .Add(builder)
                          .AddBlankLine(trailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(TextListBuilder builder, string id = null, string indicator = null, int trailingBlankLines = 0)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .Add(builder)
                          .AddBlankLine(trailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(TextFieldSetBuilder builder, string id = null, string indicator = null, int trailingBlankLines = 0)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .Add(builder)
                          .AddBlankLine(trailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(TextTableBuilder builder, string id = null, string indicator = null, int trailingBlankLines = 0)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .AddLine(builder.Render())
                          .AddBlankLine(trailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(string body, string id = null, string indicator = null, int trailingBlankLines = 0)
    {
        return StartItem().Id(id)
                          .Indicator(indicator)
                          .AddLine(body)
                          .AddBlankLine(trailingBlankLines)
                          .SaveItem();
    }

    public TextListBuilder AddItem(Action<TextBuilder> bodyFactory,
                                   string id = null,
                                   string indicator = null)
    {
        TextBuilder bodyBuilder = TextBuilder.Create();
        bodyFactory(bodyBuilder);

        AddItem(new TextListItemModel(Indicator: indicator,
                                      Id: id,
                                      Body: bodyBuilder.Build()));

        return this;
    }

    public static TextListBuilder Create(string separator = ":", int indentSize = 2)
    {
        return new TextListBuilder(TextBuilder.Create(indentSize: indentSize)).Separator(separator);
    }
}