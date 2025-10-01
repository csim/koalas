namespace Koalas.Text.Models;

public interface IBorderTextRow : ITextRow
{
    public char Dash { get; }
    public bool External { get; init; }

    public char DoubleJunction(ITextColumn col, ITextRow row);
    public char SingleJunction(ITextColumn col, ITextRow row);
}

public interface ITextRow
{
    public bool First { get; set; }
    public int? Id { get; set; }
    public int Index { get; set; }
    public bool Last { get; set; }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns);
}

public class DataTextRow(int id, IEnumerable<object?> values) : List<object?>(values), ITextRow
{
    public bool First { get; set; }
    public int? Id { get; set; } = id;
    public int Index { get; set; }
    public bool Last { get; set; }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns)
    {
        int lineIndexMax = columns.Max(c => c.Lines(this).Count);

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

public class DoubleBorderTextRow : IBorderTextRow
{
    public const char DashChar = '═';

    public char Dash => DashChar;
    public bool External { get; init; }
    public bool First { get; set; }

    public int? Id
    {
        get => null;
        set { }
    }

    public int Index { get; set; }
    public bool Last { get; set; }

    private static readonly char[,] _doubleJunctionChars =
    {
        { '╔', '╦', '╗' },
        { '╠', '╬', '╣' },
        { '╚', '╩', '╝' },
    };

    private static readonly char[,] _singleJunctionChars =
    {
        { '╒', '╤', '╕' },
        { '╞', '╪', '╡' },
        { '╘', '╧', '╛' },
    };

    public char DoubleJunction(ITextColumn col, ITextRow row)
    {
        return _doubleJunctionChars[
            row.First ? 0
            : row.Last ? 2
            : 1,
            col.First ? 0
            : col.Last ? 2
            : 1
        ];
    }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns)
    {
        foreach (ITextColumn col in columns)
        {
            col.Render(output, this, 0);
        }

        output.AppendLine();
    }

    public char SingleJunction(ITextColumn col, ITextRow row)
    {
        return _singleJunctionChars[
            row.First ? 0
            : row.Last ? 2
            : 1,
            col.First ? 0
            : col.Last ? 2
            : 1
        ];
    }
}

public class EllipsisTextRow : ITextRow
{
    public int ColumnIndex { get; set; }
    public bool First { get; set; }

    public int? Id
    {
        get => null;
        set { }
    }

    public int Index { get; set; }
    public string Indicator { get; set; } = "...";
    public bool Last { get; set; }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns)
    {
        int index = 0;

        foreach (ITextColumn col in columns)
        {
            output.Append(index == ColumnIndex ? Indicator : new string(' ', col.Width));
            index++;
        }

        output.AppendLine();
    }
}

public class HeadingTextRow : ITextRow
{
    public bool First { get; set; }

    public int? Id
    {
        get => null;
        set { }
    }

    public int Index { get; set; }
    public bool Last { get; set; }

    private readonly string[]? _headingOverrides;

    public HeadingTextRow() { }

    public HeadingTextRow(string[] headingOverrides)
    {
        _headingOverrides = headingOverrides;
    }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns)
    {
        if (
            columns.OfType<TextColumnBase>().All(static c => string.IsNullOrEmpty(c.Heading))
            && _headingOverrides?.Any() is false or null
        )
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

public class SingleBorderTextRow : IBorderTextRow
{
    public const char DashChar = '─';

    public char Dash => DashChar;
    public bool External { get; init; }
    public bool First { get; set; }

    public int? Id
    {
        get => null;
        set { }
    }

    public int Index { get; set; }
    public bool Last { get; set; }

    private static readonly char[,] _doubleJunctionChars =
    {
        { '╓', '╥', '╖' },
        { '╟', '╫', '╢' },
        { '╙', '╨', '╜' },
    };

    private static readonly char[,] _singleJunctionChars =
    {
        { '┌', '┬', '┐' },
        { '├', '┼', '┤' },
        { '└', '┴', '┘' },
    };

    public char DoubleJunction(ITextColumn col, ITextRow row)
    {
        return _doubleJunctionChars[
            row.First ? 0
            : row.Last ? 2
            : 1,
            col.First ? 0
            : col.Last ? 2
            : 1
        ];
    }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns)
    {
        foreach (ITextColumn col in columns)
        {
            col.Render(output, this, 0);
        }

        output.AppendLine();
    }

    public char SingleJunction(ITextColumn col, ITextRow row)
    {
        return _singleJunctionChars[
            row.First ? 0
            : row.Last ? 2
            : 1,
            col.First ? 0
            : col.Last ? 2
            : 1
        ];
    }
}
