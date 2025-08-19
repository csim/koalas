namespace Koalas.Text;

public partial class TextFieldSetBuilder : ITextBuilder
{
    internal TextFieldSetBuilder(TextBuilder parent)
    {
        _parent = parent;
    }

    private readonly List<TextFieldModel> _items = [];
    private bool _labelRightAlign;
    private int _labelRightPadding;
    private int _maxValueWidth;
    private int _minLabelWidth;
    private int _minValueWidth;
    private string _nullProjection;
    private readonly TextBuilder _parent;
    private bool _saved;
    private string _separator;
    private int _valueLeftPadding = 1;
    private int _valueOverflowIndent;
    private bool _valueRightAlign;

    public int Count => _items.Count;

    public IRender Build()
    {
        return new TextFieldSetModel(Items: _items,
                                     Separator: _separator,
                                     MinLabelWidth: _minLabelWidth,
                                     LabelRightPadding: _labelRightPadding,
                                     LabelRightAlign: _labelRightAlign,
                                     MinValueWidth: _minValueWidth,
                                     MaxValueWidth: _maxValueWidth,
                                     ValueLeftPadding: _valueLeftPadding,
                                     ValueRightAlign: _valueRightAlign,
                                     ValueOverflowIndent: _valueOverflowIndent,
                                     NullProjection: _nullProjection);
    }

    public static TextFieldSetBuilder Create(int minLabelWidth = 0,
                                             int minValueWidth = 0,
                                             int maxValueWidth = 1000,
                                             string fieldSeparator = ":",
                                             int labelRightPadding = 1,
                                             int valueLeftPadding = 1,
                                             bool labelRightAlign = false,
                                             bool valueRightAlign = false,
                                             int valueOverflowIndent = 0)
    {
        return new TextFieldSetBuilder(TextBuilder.Create()).MinLabelWidth(minLabelWidth)
                                                            .MinValueWidth(minValueWidth)
                                                            .MaxValueWidth(maxValueWidth)
                                                            .FieldSeparator(fieldSeparator)
                                                            .LabelRightPadding(labelRightPadding)
                                                            .ValueLeftPadding(valueLeftPadding)
                                                            .LabelRightAlign(labelRightAlign)
                                                            .ValueRightAlign(valueRightAlign)
                                                            .ValueOverflowIndent(valueOverflowIndent);
    }

    public string Render()
    {
        return Build().Render();
    }

    public TextBuilder SaveFieldSet()
    {
        if (_saved) throw new Exception($"Cannot {nameof(SaveFieldSet)}, {nameof(TextFieldSetBuilder)} already saved.");

        _saved = true;


        return _items.Count > 0
                   ? _parent.Add(this)
                   : _parent;
    }

    public TextFieldSetItemBuilder StartField()
    {
        return new TextFieldSetItemBuilder(this);
    }

    public override string ToString()
    {
        return Render();
    }
}

public partial class TextFieldSetBuilder
{
    public TextFieldSetBuilder FieldSeparator(string fieldSeparator)
    {
        _separator = fieldSeparator;

        return this;
    }

    public TextFieldSetBuilder LabelRightAlign(bool labelRightAlign)
    {
        _labelRightAlign = labelRightAlign;

        return this;
    }

    public TextFieldSetBuilder LabelRightPadding(int labelRightPadding)
    {
        _labelRightPadding = labelRightPadding;

        return this;
    }

    public TextFieldSetBuilder MaxValueWidth(int maxValueWidth)
    {
        _maxValueWidth = maxValueWidth;

        return this;
    }

    public TextFieldSetBuilder MinLabelWidth(int minLabelWidth)
    {
        _minLabelWidth = minLabelWidth;

        return this;
    }

    public TextFieldSetBuilder MinValueWidth(int minValueWidth)
    {
        _minValueWidth = minValueWidth;

        return this;
    }

    public TextFieldSetBuilder NullProjection(string nullProjection)
    {
        _nullProjection = nullProjection;

        return this;
    }

    public TextFieldSetBuilder ValueLeftPadding(int valueLeftPadding)
    {
        _valueLeftPadding = valueLeftPadding;

        return this;
    }

    public TextFieldSetBuilder ValueOverflowIndent(int valueOverflowIndent)
    {
        _valueOverflowIndent = valueOverflowIndent;

        return this;
    }

    public TextFieldSetBuilder ValueRightAlign(bool valueRightAlign)
    {
        _valueRightAlign = valueRightAlign;

        return this;
    }
}

public partial class TextFieldSetBuilder
{
    public TextFieldSetBuilder AddField(string label, ITextBuilder value)
    {
        return StartField().Label(label)
                           .Value(value)
                           .SaveField();
    }

    public TextFieldSetBuilder AddField(string label, bool? value, int trailingBlankLines = 0)
    {
        return AddField(label, (object) value, trailingBlankLines: trailingBlankLines);
    }

    public TextFieldSetBuilder AddField(string label, int? value, string format = "N0", int trailingBlankLines = 0)
    {
        return AddField(label, (object) value, format);
    }

    public TextFieldSetBuilder AddField(string label, double? value, string format = "N3", int trailingBlankLines = 0)
    {
        return AddField(label, (object) value, format, trailingBlankLines: trailingBlankLines);
    }

    public TextFieldSetBuilder AddField(string label, decimal? value, string format = "N3", int trailingBlankLines = 0)
    {
        return AddField(label, (object) value, format, trailingBlankLines: trailingBlankLines);
    }

    public TextFieldSetBuilder AddField(string label, object value, string format = null, int trailingBlankLines = 0)
    {
        value ??= "--";
        format ??= value switch {
                       int     => "N0",
                       float   => "N3",
                       double  => "N3",
                       decimal => "N6",
                       _       => null
                   };

        string formattedValue = format == null
                                    ? value.ToString().TrimEnd()
                                    : string.Format($"{{0:{format}}}", value);

        IRender valueModel = new TextLineModel(formattedValue);

        // ReSharper disable once InvertIf
        if (trailingBlankLines > 0)
        {
            List<IRender> childValueModels = [valueModel];
            for (int i = 0; i < trailingBlankLines; i++)
            {
                childValueModels.Add(new TextLineModel(string.Empty));
            }

            valueModel = new TextRegionModel(childValueModels, IndentSize: 0);
        }

        return AddField(new TextFieldModel(Label: label,
                                           Value: valueModel,
                                           Format: format));
    }

    public TextFieldSetBuilder AddField(TextFieldModel field)
    {
        _items.Add(field);

        return this;
    }
}