namespace Koalas.Text.Models;

public record class TextHangSectionModel(string Heading,
                                         ITextModel Body) : ITextModel
{
    public string Render()
    {
        string body = Body.Render();

        return $"""
                {Heading}:
                {body.Indent(2)}
                """;
    }
}