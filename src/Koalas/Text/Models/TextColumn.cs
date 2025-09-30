using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koalas.Text.Models;

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

public abstract class TextColumnBase : ITextColumn
{
    public static char Space = ' ';

    public bool AlignRight { get; init; }

    public bool First { get; set; }

    public string Heading { get; set; } = string.Empty;

    public int Index { get; set; }

    public bool Last { get; set; }

    public int Width { get; set; }

    public abstract IReadOnlyList<string> Lines(ITextRow row);

    public virtual void Render(StringBuilder output, ITextRow row, int lineIndex)
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
        string line = 0 <= lineIndex && lineIndex < lines.Count
                          ? lines[lineIndex] ?? string.Empty
                          : string.Empty;

        output.Append(AlignRight
                          ? line.PadLeft(Width)
                          : Last
                              ? line
                              : line.PadRight(Width));
    }

    public virtual void RenderHeading(StringBuilder output, string headingOverride = null)
    {
        string heading = headingOverride ?? Heading ?? string.Empty;

        output.Append(AlignRight ? heading.PadLeft(Width) : heading.PadRight(Width));
    }
}

public class TextColumn : TextColumnBase, IDynamicWidthTextColumn
{
    private readonly Dictionary<ITextRow, IReadOnlyList<string>> _lineCache = new();

    public int DataColumnIndex { get; init; }

    public string Format { get; init; }

    public int? LeftPadding { get; set; }

    public int? MaximumWidth { get; set; }

    public int MinimumWidth { get; init; }

    public string NullProjection { get; init; }

    public int? RightPadding { get; set; }

    public int WrapOverflowIndent { get; set; }

    public string FormatValue(ITextRow row)
    {
        if (row is not DataTextRow dataRow)
        {
            return string.Empty;
        }

        object rawData = dataRow[DataColumnIndex] ?? NullProjection;
        return Format == null
                   ? rawData?.ToString() ?? string.Empty
                   : string.Format($"{{0:{Format}}}", rawData);
    }

    public override IReadOnlyList<string> Lines(ITextRow row)
    {
        if (_lineCache.TryGetValue(row, out IReadOnlyList<string> cachedLines))
        {
            return cachedLines;
        }

        string formattedContent = FormatValue(row);

        IReadOnlyList<string> rawLines = formattedContent.Lines()
                                                         .ToList();

        if (MaximumWidth == null || (rawLines.Count == 1 && rawLines[0].Length < MaximumWidth))
        {
            return _lineCache[row] = rawLines;
        }

        return _lineCache[row] = rawLines.SelectMany(line => line.Wrap(MaximumWidth.Value, overflowIndentSize: WrapOverflowIndent))
                                         .ToList();
    }
}

public interface IBorderTextColumn : ITextColumn
{
    public bool External { get; }
}

public class SingleBorderTextColumn : TextColumnBase, IBorderTextColumn
{
    private const char VerticalBar = '│';
    private IReadOnlyList<string> _lines;

    public bool External { get; init; }

    public string FormatValue(ITextRow row)
        => VerticalBar.ToString();

    public override IReadOnlyList<string> Lines(ITextRow row)
        => _lines ??= [FormatValue(row)];

    public override void Render(StringBuilder output, ITextRow row, int lineIndex)
    {
        if (row is IBorderTextRow borderRow)
        {
            output.Append(borderRow.SingleJunction(this, row).ToString());

            return;
        }

        output.Append(VerticalBar);
    }

    public override void RenderHeading(StringBuilder output, string headingOverride = null)
        => output.Append(VerticalBar);
}

public class DoubleBorderTextColumn : TextColumnBase, IBorderTextColumn
{
    private const char VerticalBar = '║';
    private IReadOnlyList<string> _lines;

    public bool External { get; init; }

    public virtual string FormatValue(ITextRow row)
        => VerticalBar.ToString();

    public override IReadOnlyList<string> Lines(ITextRow row)
        => _lines ??= [FormatValue(row)];

    public override void Render(StringBuilder output, ITextRow row, int lineIndex)
    {
        if (row is IBorderTextRow borderRow)
        {
            output.Append(borderRow.DoubleJunction(this, row).ToString());

            return;
        }

        output.Append(VerticalBar);
    }

    public override void RenderHeading(StringBuilder output, string headingOverride = null)
        => output.Append(VerticalBar);
}

public class IdentityTextColumn : TextColumnBase, IDynamicWidthTextColumn
{
    public int? LeftPadding { get; set; }

    public int? MaximumWidth { get; set; }

    public int MinimumWidth => 1;

    public int? RightPadding { get; set; }

    public virtual string FormatValue(ITextRow row)
        => (row.Id ?? 0).ToString("N0").PadLeft(Width);

    public override IReadOnlyList<string> Lines(ITextRow row)
        => [FormatValue(row)];
}

public class PaddingTextColumn(int width) : TextColumnBase
{
    public override IReadOnlyList<string> Lines(ITextRow row)
        => [new(Space, width)];

    public override void Render(StringBuilder output, ITextRow row, int lineIndex)
    {
        if (row is IBorderTextRow borderRow)
        {
            output.Append(borderRow.Dash, Width);

            return;
        }

        output.Append(Space, width);
    }
}

public class StaticTextColumn : TextColumnBase
{
    private IReadOnlyList<string> _lines;

    public bool ShowInHeading { get; init; }

    public bool ShowInRow { get; init; }

    public bool ShowInRowExtraLines { get; init; }

    public string Text { get; init; }

    public virtual string FormatValue(ITextRow row)
        => Text;

    public override IReadOnlyList<string> Lines(ITextRow row)
        => _lines ??= [Text];

    public override void Render(StringBuilder output, ITextRow row, int lineIndex)
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

        output.Append(lineIndex > 0 && !ShowInRowExtraLines
                          ? blank
                          : ShowInRow
                              ? fdata
                              : blank);
    }
}