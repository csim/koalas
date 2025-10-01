namespace Koalas.Text.Models;

public record class TextListItemModel(
    string? Indicator,
    string? Id,
    string? Separator,
    IRender Body
) : IRender
{
    public string Render()
    {
        return string.Empty;
    }
}
