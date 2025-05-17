namespace Koalas.Text.Models;

public record class TextRegionModel(IReadOnlyList<ITextModel> Children,
                                    int IndentSize) : ITextModel
{
    public string Render()
    {
        List<string> output = [];

        foreach (ITextModel child in Children)
        {
            string childOutput = child.Render();

            if (!childOutput.EndsWith(Environment.NewLine)) childOutput += Environment.NewLine;

            output.Add(childOutput.Indent(IndentSize));
        }

        return output.ToJoinString();
    }
}