using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koalas.Text.Models;

public interface ITextRow
{
    bool First { get; set; }

    int? Id { get; set; }

    int Index { get; set; }

    bool Last { get; set; }

    void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns);
}

public interface IBorderTextRow : ITextRow
{
    char Dash { get; }

    public bool External { get; init; }

    public char DoubleJunction(ITextColumn col, ITextRow row);

    public char SingleJunction(ITextColumn col, ITextRow row);
}

public class DataTextRow(int id,
                         IEnumerable<object> values) : List<object>(values), ITextRow
{
    public bool First { get; set; }

    public int? Id { get; set; } = id;

    public int Index { get; set; }

    public bool Last { get; set; }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns)
    {
        int lineIndexMax = columns.Select(c => c.Lines(this).Count).Max();

        for (int lineIndex = 0; lineIndex < lineIndexMax; lineIndex++)
        {
            foreach (ITextColumn col in columns)
            {
                col.Render(output, this, lineIndex);
            }

            output.AppendLine();
        }
    }
}

public class HeadingTextRow : ITextRow
{
    public HeadingTextRow() { }

    public HeadingTextRow(string[] headingOverrides)
    {
        _headingOverrides = headingOverrides;
    }

    private readonly string[] _headingOverrides;

    public bool First { get; set; }

    public int? Id { get => null; set { } }

    public int Index { get; set; }

    public bool Last { get; set; }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns)
    {
        if (columns.OfType<TextColumnBase>().All(c => string.IsNullOrEmpty(c.Heading))
            && _headingOverrides?.Any() is false or null)
        {
            return;
        }

        int index = 0;
        foreach (ITextColumn col in columns)
        {
            if (col is IDynamicWidthTextColumn)
            {
                col.RenderHeading(output, _headingOverrides?.ElementAtOrDefault(index));
                index++;
            }
            else
            {
                col.RenderHeading(output);
            }
        }

        output.AppendLine();
    }
}

public class EllipsisTextRow : ITextRow
{
    public int ColumnIndex { get; set; }

    public bool First { get; set; }

    public int? Id { get => null; set { } }

    public int Index { get; set; }

    public string Indicator { get; set; } = "...";

    public bool Last { get; set; }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns)
    {
        int index = 0;

        foreach (ITextColumn col in columns)
        {
            output.Append(index == ColumnIndex
                              ? Indicator
                              : new string(' ', col.Width));
            index++;
        }

        output.AppendLine();
    }
}

public class SingleBorderTextRow : IBorderTextRow
{
    public const char DashChar = '─';
    private static readonly char[,] DoubleJunctionChars = {
                                                              {
                                                                  '╓',
                                                                  '╥',
                                                                  '╖'
                                                              }, {
                                                                  '╟',
                                                                  '╫',
                                                                  '╢'
                                                              }, {
                                                                  '╙',
                                                                  '╨',
                                                                  '╜'
                                                              }
                                                          };
    private static readonly char[,] SingleJunctionChars = {
                                                              {
                                                                  '┌',
                                                                  '┬',
                                                                  '┐'
                                                              }, {
                                                                  '├',
                                                                  '┼',
                                                                  '┤'
                                                              }, {
                                                                  '└',
                                                                  '┴',
                                                                  '┘'
                                                              }
                                                          };

    public char Dash => DashChar;

    public bool External { get; init; }

    public bool First { get; set; }

    public int? Id { get => null; set { } }

    public int Index { get; set; }

    public bool Last { get; set; }

    public char DoubleJunction(ITextColumn col, ITextRow row)
        => DoubleJunctionChars[row.First
                                   ? 0
                                   : row.Last
                                       ? 2
                                       : 1,
                               col.First
                                   ? 0
                                   : col.Last
                                       ? 2
                                       : 1];

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns)
    {
        foreach (ITextColumn col in columns)
        {
            col.Render(output, this, 0);
        }

        output.AppendLine();
    }

    public char SingleJunction(ITextColumn col, ITextRow row)
        => SingleJunctionChars[row.First
                                   ? 0
                                   : row.Last
                                       ? 2
                                       : 1,
                               col.First
                                   ? 0
                                   : col.Last
                                       ? 2
                                       : 1];
}

public class DoubleBorderTextRow : IBorderTextRow
{
    public const char DashChar = '═';
    private static readonly char[,] DoubleJunctionChars = {
                                                              {
                                                                  '╔',
                                                                  '╦',
                                                                  '╗'
                                                              }, {
                                                                  '╠',
                                                                  '╬',
                                                                  '╣'
                                                              }, {
                                                                  '╚',
                                                                  '╩',
                                                                  '╝'
                                                              }
                                                          };
    private static readonly char[,] SingleJunctionChars = {
                                                              {
                                                                  '╒',
                                                                  '╤',
                                                                  '╕'
                                                              }, {
                                                                  '╞',
                                                                  '╪',
                                                                  '╡'
                                                              }, {
                                                                  '╘',
                                                                  '╧',
                                                                  '╛'
                                                              }
                                                          };

    public char Dash => DashChar;

    public bool External { get; init; }

    public bool First { get; set; }

    public int? Id { get => null; set { } }

    public int Index { get; set; }

    public bool Last { get; set; }

    public char DoubleJunction(ITextColumn col, ITextRow row)
        => DoubleJunctionChars[row.First
                                   ? 0
                                   : row.Last
                                       ? 2
                                       : 1,
                               col.First
                                   ? 0
                                   : col.Last
                                       ? 2
                                       : 1];

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns)
    {
        foreach (ITextColumn col in columns)
        {
            col.Render(output, this, 0);
        }

        output.AppendLine();
    }

    public char SingleJunction(ITextColumn col, ITextRow row)
        => SingleJunctionChars[row.First
                                   ? 0
                                   : row.Last
                                       ? 2
                                       : 1,
                               col.First
                                   ? 0
                                   : col.Last
                                       ? 2
                                       : 1];
}