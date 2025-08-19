namespace Koalas.Text.Models;

public record class TextSectionModel(string Heading,
                                     IRender Body,
                                     string HeadingSuffix) : IRender
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