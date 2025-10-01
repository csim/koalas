namespace Koalas.Text.Models;

public record class TextRegionModel(IReadOnlyList<IRender> Children, int IndentSize) : IRender
{
    public string Render()
    {
        List<string> output = [];

        foreach (IRender child in Children)
        {
            string childOutput = child.Render();

            if (!childOutput.EndsWith(Environment.NewLine))
            {
                childOutput += Environment.NewLine;
            }

            output.Add(childOutput.Indent(IndentSize));
        }

        return output.ToJoinString();
    }
}
