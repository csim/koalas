namespace Koalas.Text.Models;

public interface IBorderTextColumn : ITextColumn
{
    bool External { get; }
}

public interface ITextColumn
{
    bool First { get; set; }
    int Index { get; set; }
    bool Last { get; set; }
    int Width { get; set; }

    IReadOnlyList<string> Lines(ITextRow row);
    void Render(StringBuilder output, ITextRow row, int partitionIndex);
    void RenderHeading(StringBuilder output, string headingOverride = null);
}

public class DoubleBorderTextColumn : TextColumnBase, IBorderTextColumn
{
    private const char _verticalBar = '║';

    public bool External { get; init; }

    private IReadOnlyList<string> _lines;

    public virtual string FormatValue(ITextRow row) => _verticalBar.ToString();

    public override IReadOnlyList<string> Lines(ITextRow row) => _lines ??= [FormatValue(row)];

    public override void Render(StringBuilder output, ITextRow row, int partitionIndex)
    {
        if (row is IBorderTextRow borderRow)
        {
            output.Append(borderRow.DoubleJunction(this, row));

            return;
        }

        output.Append(_verticalBar);
    }

    public override void RenderHeading(StringBuilder output, string headingOverride = null) =>
        output.Append(_verticalBar);
}

public class IdentityTextColumn : TextColumnBase, IDynamicWidthTextColumn
{
    public int? LeftPadding { get; set; }
    public int? MaximumWidth { get; set; }
    public int MinimumWidth => 1;
    public int? RightPadding { get; set; }

    public virtual string FormatValue(ITextRow row) => (row.Id ?? 0).ToString("N0").PadLeft(Width);

    public override IReadOnlyList<string> Lines(ITextRow row) => [FormatValue(row)];
}

public class PaddingTextColumn(int width) : TextColumnBase
{
    public override IReadOnlyList<string> Lines(ITextRow row) => [new(Space, width)];

    public override void Render(StringBuilder output, ITextRow row, int partitionIndex)
    {
        if (row is IBorderTextRow borderRow)
        {
            output.Append(borderRow.Dash, Width);

            return;
        }

        output.Append(Space, width);
    }
}

public class SingleBorderTextColumn : TextColumnBase, IBorderTextColumn
{
    private const char _verticalBar = '│';

    public bool External { get; init; }

    private IReadOnlyList<string> _lines;

    public static string FormatValue(ITextRow row) => _verticalBar.ToString();

    public override IReadOnlyList<string> Lines(ITextRow row) => _lines ??= [FormatValue(row)];

    public override void Render(StringBuilder output, ITextRow row, int partitionIndex)
    {
        if (row is IBorderTextRow borderRow)
        {
            output.Append(borderRow.SingleJunction(this, row));

            return;
        }

        output.Append(_verticalBar);
    }

    public override void RenderHeading(StringBuilder output, string headingOverride = null) =>
        output.Append(_verticalBar);
}

public class StaticTextColumn : TextColumnBase
{
    public bool ShowInHeading { get; init; }
    public bool ShowInRow { get; init; }
    public bool ShowInRowExtraLines { get; init; }
    public string Text { get; init; }

    private IReadOnlyList<string> _lines;

    public virtual string FormatValue(ITextRow row) => Text;

    public override IReadOnlyList<string> Lines(ITextRow row) => _lines ??= [Text];

    public override void Render(StringBuilder output, ITextRow row, int partitionIndex)
    {
        string blank = new(Space, Width);
        string fdata = FormatValue(row);

        if (row is HeadingTextRow)
        {
            output.Append(ShowInHeading ? fdata : blank);

            return;
        }

        if (row is IBorderTextRow borderRow)
        {
            output.Append(borderRow.Dash, Width);

            return;
        }

        output.Append(
            partitionIndex > 0 && !ShowInRowExtraLines ? blank
            : ShowInRow ? fdata
            : blank
        );
    }
}

public class TextColumn : TextColumnBase, IDynamicWidthTextColumn
{
    public int DataColumnIndex { get; init; }
    public string Format { get; init; }
    public int? LeftPadding { get; set; }
    public int? MaximumWidth { get; set; }
    public int MinimumWidth { get; init; }
    public string NullProjection { get; init; }
    public int? RightPadding { get; set; }
    public int WrapOverflowIndent { get; set; }

    private readonly Dictionary<ITextRow, IReadOnlyList<string>> _lineCache = [];

    public string FormatValue(ITextRow row)
    {
        if (row is not DataTextRow dataRow)
        {
            return string.Empty;
        }

        object rawData = dataRow[DataColumnIndex] ?? NullProjection;
        return Format == null ? rawData?.ToString() ?? string.Empty : string.Format($"{{0:{Format}}}", rawData);
    }

    public override IReadOnlyList<string> Lines(ITextRow row)
    {
        if (_lineCache.TryGetValue(row, out IReadOnlyList<string> cachedLines))
        {
            return cachedLines;
        }

        string formattedContent = FormatValue(row);

        List<string> rawLines = [.. formattedContent.Lines()];

        return MaximumWidth == null || (rawLines.Count == 1 && rawLines[0].Length < MaximumWidth)
            ? (_lineCache[row] = rawLines)
            : (
                _lineCache[row] = [
                    .. rawLines.SelectMany(line =>
                        line.Wrap(MaximumWidth.Value, overflowIndentSize: WrapOverflowIndent)
                    ),
                ]
            );
    }
}

public abstract class TextColumnBase : ITextColumn
{
    public bool AlignRight { get; init; }
    public bool First { get; set; }
    public string Heading { get; set; } = string.Empty;
    public int Index { get; set; }
    public bool Last { get; set; }
    public static char Space { get; set; } = ' ';
    public int Width { get; set; }

    public abstract IReadOnlyList<string> Lines(ITextRow row);

    public virtual void Render(StringBuilder output, ITextRow row, int partitionIndex)
    {
        if (row is HeadingTextRow)
        {
            output.Append(AlignRight ? Heading.PadLeft(Width) : Heading.PadRight(Width));

            return;
        }

        if (row is IBorderTextRow borderRow)
        {
            output.Append(borderRow.Dash, Width);

            return;
        }

        IReadOnlyList<string> lines = Lines(row);
        string line =
            0 <= partitionIndex && partitionIndex < lines.Count ? lines[partitionIndex] ?? string.Empty : string.Empty;

        output.Append(
            AlignRight ? line.PadLeft(Width)
            : Last ? line
            : line.PadRight(Width)
        );
    }

    public virtual void RenderHeading(StringBuilder output, string? headingOverride = null)
    {
        string heading = headingOverride ?? Heading ?? string.Empty;

        output.Append(AlignRight ? heading.PadLeft(Width) : heading.PadRight(Width));
    }
}
