namespace Koalas.Text.Models;

public record class TextSectionModel(string Heading,
                                     ITextModel Body,
                                     string HeadingSuffix) : ITextModel
{
    public string Render()
    {
        string body = Body.Render();

        return $"""
                {Heading}{HeadingSuffix}
                {new string('\u2500', Heading.Length)}
                {body}
                """;
    }
}