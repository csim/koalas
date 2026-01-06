namespace Outback.Text.Models;

public record class TextFieldSetModel(
    IReadOnlyList<TextFieldModel> Items,
    string Separator,
    int MinLabelWidth,
    int LabelRightPadding,
    bool LabelRightAlign,
    int MinValueWidth,
    int MaxValueWidth,
    int ValueLeftPadding,
    bool ValueRightAlign,
    int ValueOverflowIndent,
    string? NullProjection
) : IRender
{
    public string Render()
    {
        TextTableBuilder table = TextTableBuilder
            .Create()
            .AddColumn(
                minWidth: MinLabelWidth,
                leftPadding: 0,
                rightPadding: LabelRightPadding,
                alignRight: LabelRightAlign,
                nullProjection: NullProjection
            )
            .AddStaticColumn(Separator)
            .AddColumn(
                minWidth: MinValueWidth,
                maxWidth: MaxValueWidth,
                wrapOverflowIndent: ValueOverflowIndent,
                leftPadding: ValueLeftPadding,
                rightPadding: 0,
                alignRight: ValueRightAlign
            );

        foreach (TextFieldModel item in Items)
        {
            string value = item.Value.Render();

            table.AddDataRow(item.Label, value);
        }

        return table.Render();
    }
}
