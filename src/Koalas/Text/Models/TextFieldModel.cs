namespace Koalas.Text.Models;

public record class TextFieldModel(string Label,
                                   ITextModel Value,
                                   string Format) : ITextModel
{
    public string Render()
    {
        return string.Empty;
    }
}