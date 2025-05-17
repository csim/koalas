namespace Koalas.Text.Models;

public record class TextListItemModel(string Indicator,
                                      string Id,
                                      ITextModel Body) : ITextModel
{
    public string Render()
    {
        return string.Empty;
    }
}