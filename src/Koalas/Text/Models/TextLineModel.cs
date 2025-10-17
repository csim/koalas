namespace Koalas.Text.Models;

public record class TextLineModel(string? Text) : IRender
{
    public string Render()
    {
        return string.Concat(Text, Environment.NewLine);
    }
}
