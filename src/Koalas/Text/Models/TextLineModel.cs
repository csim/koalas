namespace Koalas.Text.Models;

public record class TextLineModel(string Text) : ITextModel
{
    public string Render()
    {
        return Text + Environment.NewLine;
    }
}