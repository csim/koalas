namespace Koalas.Text.Models;

public record class TextFieldModel(string? Label, IRender Value, string? Format) : IRender
{
    public string Render()
    {
        return string.Empty;
    }
}
