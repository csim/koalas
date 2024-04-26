namespace Koalas;

public class TextFieldSetBuilder {
    private TextFieldSetBuilder(int minLabelWidth,
                                int minValueWidth,
                                string fieldSeparator,
                                bool omitNullValues,
                                string nullProjection) {
        _omitNullValues = omitNullValues;
        _nullProjection = nullProjection;
        _table = new TextTableBuilder().AddColumn(minWidth: minLabelWidth, leftPadding: 0)
                                       .AddStaticColumn(fieldSeparator)
                                       .AddColumn(minWidth: minValueWidth, rightPadding: 0);
    }

    private readonly string _nullProjection;
    private readonly bool _omitNullValues;
    private readonly TextTableBuilder _table;

    public TextFieldSetBuilder AddField(string label, bool? value) {
        return AddField(label, (object)value);
    }

    public TextFieldSetBuilder AddField(string label, int? value, string format = "N0") {
        return AddField(label, (object)value, format);
    }

    public TextFieldSetBuilder AddField(string label, uint? value, string format = "N0") {
        return AddField(label, (object)value, format);
    }

    public TextFieldSetBuilder AddField(string label, double? value, string format = "N3") {
        return AddField(label, (object)value, format);
    }

    public TextFieldSetBuilder AddField(string label, object value, string format = null) {
        if (value == null) {
            if (_omitNullValues) {
                return this;
            }

            _table.AddDataRow(label, _nullProjection);

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

        _table.AddDataRow(label, result);

        return this;
    }

    public static TextFieldSetBuilder Create(int minLabelWidth = 0,
                                             int minValueWidth = 0,
                                             string fieldSeparator = ":",
                                             bool omitNullValues = false,
                                             string nullProjection = "--") {
        return new(minLabelWidth, minValueWidth, fieldSeparator, omitNullValues, nullProjection);
    }

    public string Render() {
        return _table.Render();
    }

    public override string ToString() {
        return Render();
    }
}