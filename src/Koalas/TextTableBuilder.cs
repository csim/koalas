namespace Koalas;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koalas.Extensions;

public partial class TextTableBuilder(TextTableBorder border = TextTableBorder.Default,
                                      int? defaultColumnPadding = null,
                                      int? defaultColumnMaxWidth = null) : ITextRowBuilder
{
    private readonly bool _columnBorders = border.HasFlag(TextTableBorder.Column) || border.HasFlag(TextTableBorder.Inner);
    private readonly List<ITextColumn> _columns = [];
    private int _currentDataColumnIndex;
    private readonly int _defaultColumnMaxWidth = defaultColumnMaxWidth ?? int.MaxValue;
    private readonly int _defaultColumnPadding = defaultColumnPadding ?? 1;
    private readonly bool _headingRowBorder = border.HasFlag(TextTableBorder.Inner);
    private bool _layoutBorderComputed;
    private readonly bool _outerBorders = border.HasFlag(TextTableBorder.Outer);
    private readonly bool _rowBorders = border.HasFlag(TextTableBorder.Row);
    private readonly List<ITextRow> _rows = [];

    public static TextTableBuilder Create(TextTableBorder border = TextTableBorder.Default, int? defaultColumnPadding = null, int? defaultColumnMaxWidth = null)
    {
        return new TextTableBuilder(border, defaultColumnPadding, defaultColumnMaxWidth);
    }

    public static TextTableBuilder Create(IEnumerable<IEnumerable<object>> values,
                                          TextTableBorder border = TextTableBorder.Inner,
                                          IEnumerable<string> columnNames = null,
                                          int? defaultColumnPadding = 1,
                                          int? defaultColumnMaxWidth = 50,
                                          Func<object, string> formatValue = null)
    {
        TextTableBuilder builder = Create(border, defaultColumnPadding, defaultColumnMaxWidth);
        if (values == null) return builder;

        values = values as IReadOnlyList<IEnumerable<object>> ?? values.ToList();
        if (!values.Any()) return builder;

        columnNames = columnNames?.ToList() ?? [];

        int maxColumnCount = values.Max(row => row.Count());
        for (int index = 0; index < maxColumnCount; index++)
        {
            bool allNumeric = values.All(row => row.ElementAtOrDefault(index)?.IsNumeric() is null or true);
            string columnName = columnNames?.ElementAtOrDefault(index) ?? $"Column{index + 1}";

            builder.AddColumn(columnName, maxWidth: defaultColumnMaxWidth, alignRight: allNumeric, nullProjection: "--");
        }

        builder.AddHeadingRow();

        formatValue ??= v => v.ToLiteral();

        foreach (IEnumerable<object> row in values)
        {
            List<object> rowValues = [.. row.Select(formatValue).ToArray()];

            for (int position = rowValues.Count; position < maxColumnCount; position++)
            {
                rowValues.Add(null);
            }

            builder.AddDataRow(rowValues);
        }

        return builder;
    }
}

public partial class TextTableBuilder
{
    public TextTableBuilder AddBorderColumn()
    {
        _columns.Add(new SingleBorderTextColumn { External = true });

        return this;
    }

    public TextTableBuilder AddColumn(string heading = "",
                                      int minWidth = 0,
                                      int? maxWidth = null,
                                      bool alignRight = false,
                                      string format = null,
                                      string nullProjection = null,
                                      int? leftPadding = null,
                                      int? rightPadding = null)
    {
        _columns.Add(new TextColumn {
                                        Heading = heading,
                                        MinimumWidth = minWidth,
                                        MaximumWidth = maxWidth,
                                        DataColumnIndex = _currentDataColumnIndex++,
                                        Format = format,
                                        AlignRight = alignRight,
                                        NullProjection = nullProjection,
                                        LeftPadding = leftPadding,
                                        RightPadding = rightPadding
                                    });

        return this;
    }

    public TextTableBuilder AddDoubleBorderColumn()
    {
        _columns.Add(new DoubleBorderTextColumn { External = true });

        return this;
    }

    public TextTableBuilder AddIdentityColumn(string heading = "", int? leftPadding = null, int? rightPadding = null)
    {
        _columns.Add(new IdentityTextColumn {
                                                Heading = heading,
                                                LeftPadding = leftPadding,
                                                RightPadding = rightPadding,
                                                AlignRight = true
                                            });

        return this;
    }

    public TextTableBuilder AddNumberColumn(string heading = "",
                                            string format = "N0",
                                            int minWidth = 0,
                                            string nullProjection = "--",
                                            int? leftPadding = null,
                                            int? rightPadding = null)
    {
        return AddColumn(heading,
                         minWidth: minWidth,
                         alignRight: true,
                         format: format,
                         nullProjection: nullProjection,
                         leftPadding: leftPadding,
                         rightPadding: rightPadding);
    }

    public TextTableBuilder AddStaticColumn(string value,
                                            bool showInHeading = false,
                                            bool showInRow = true,
                                            bool showInRowExtraLines = false)
    {
        _columns.Add(new StaticTextColumn {
                                              Text = value,
                                              ShowInHeading = showInHeading,
                                              ShowInRow = showInRow,
                                              ShowInRowExtraLines = showInRowExtraLines
                                          });

        return this;
    }
}

public partial class TextTableBuilder
{
    private TextRowBuilder _rowBuilder;

    /// <inheritdoc />
    public IReadOnlyList<ITextRow> Rows => _rows;

    private ITextRowBuilder RowBuilder => _rowBuilder ??= new TextRowBuilder(this, _rows, _columns);

    public ITextRowBuilder AddBorderRow()
    {
        return RowBuilder.AddBorderRow();
    }

    public ITextRowBuilder AddDataRow(params object[] cells)
    {
        return RowBuilder.AddDataRow((IReadOnlyList<object>) cells);
    }

    public ITextRowBuilder AddDataRow(IReadOnlyList<object> row, int? rowId = null)
    {
        return RowBuilder.AddDataRow(row, rowId);
    }

    public ITextRowBuilder AddDataRows(IReadOnlyList<IReadOnlyList<object>> rows, int? startRowId = null)
    {
        return RowBuilder.AddDataRows(rows, startRowId);
    }

    public ITextRowBuilder AddDoubleBorderRow()
    {
        return RowBuilder.AddDoubleBorderRow();
    }

    public ITextRowBuilder AddEllipsisRow(string indicator = "...", int indicatorColumnIndex = 0)
    {
        return RowBuilder.AddEllipsisRow(indicator, indicatorColumnIndex);
    }

    public ITextRowBuilder AddHeadingRow()
    {
        return RowBuilder.AddHeadingRow();
    }

    public ITextRowBuilder AddHeadingRow(params string[] headingOverrides)
    {
        _rows.Add(new HeadingTextRow(headingOverrides));

        return this;
    }

    public ITextRowBuilder ClearRows()
    {
        return RowBuilder.ClearRows();
    }

    /// <inheritdoc />
    public ITextRowBuilder InsertRow(int index, ITextRow row)
    {
        return RowBuilder.InsertRow(index, row);
    }

    /// <inheritdoc />
    public ITextRowBuilder RemoveRow(int index)
    {
        return RowBuilder.RemoveRow(index);
    }
}

public partial class TextTableBuilder
{
    public string Render()
    {
        return Render(null);
    }

    public string Render(int? rowLimit)
    {
        if (!_columns.Any() || !_rows.Any()) return string.Empty;

        ComputeLayout();

        StringBuilder output = new();

        bool hasRowLimit = rowLimit != null;
        int dataRowCount = 0;
        bool limitReached = false;

        foreach (ITextRow row in _rows)
        {
            if (hasRowLimit && row is DataTextRow && ++dataRowCount > rowLimit.Value)
            {
                limitReached = true;
                break;
            }

            row.Render(output, _columns);
        }

        string ret = output.ToString().TrimEnd('\r', '\n') + Environment.NewLine;

        if (!limitReached) return ret;

        int totalDataRowCount = _rows.Count(r => r is DataTextRow);
        ret += $"[{totalDataRowCount - rowLimit.Value:N0} more]{Environment.NewLine}";

        return ret;
    }

    public void ResetLayout()
    {
        _layoutBorderComputed = false;
        _columns.RemoveAll(c => c is PaddingTextColumn or IBorderTextColumn { External: false });
        _rows.RemoveAll(c => c is IBorderTextRow { External: false });
    }

    public override string ToString()
    {
        return Render();
    }

    public int Width()
    {
        ComputeLayout();

        return _columns.Sum(c => c.Width);
    }

    private void ComputeLayout()
    {
        // Resolve inner borders
        if (!_layoutBorderComputed && _columnBorders)
        {
            ComputeMeta();
            IEnumerable<int> dataColumnIndexes = from col in _columns
                                                 where col is IDynamicWidthTextColumn
                                                       && !col.First
                                                       && _columns[col.Index - 1] is not IBorderTextColumn
                                                 orderby col.Index descending
                                                 select col.Index;
            foreach (int index in dataColumnIndexes.ToList())
            {
                _columns.Insert(index, new SingleBorderTextColumn());
            }
        }

        if (!_layoutBorderComputed && _rowBorders)
        {
            ComputeMeta();
            IEnumerable<int> dataRowIndexes = from row in _rows
                                              where row is DataTextRow
                                                    && !row.First
                                                    && _rows[row.Index - 1] is not IBorderTextRow
                                              orderby row.Index descending
                                              select row.Index;
            foreach (int index in dataRowIndexes.ToList())
            {
                _rows.Insert(index, new SingleBorderTextRow());
            }
        }

        if (!_layoutBorderComputed && _headingRowBorder)
        {
            ComputeMeta();
            IEnumerable<int> insertIndexes = from row in _rows
                                             where row is HeadingTextRow
                                             from index in row.Last
                                                               ? row.Index.Yield()
                                                               : row.First
                                                                   ? (row.Index + 1).Yield()
                                                                   : new[] {
                                                                               row.Index,
                                                                               row.Index + 1
                                                                           }
                                             orderby index descending
                                             select index;
            foreach (int index in insertIndexes.ToList())
            {
                _rows.Insert(index, new SingleBorderTextRow());
            }
        }

        // Resolve padding
        ComputeMeta();
        if (!_layoutBorderComputed)
        {
            IReadOnlyList<IDynamicWidthTextColumn> dynamicColumns = (from col in _columns
                                                                     where col is IDynamicWidthTextColumn
                                                                     let dynamicCol = (IDynamicWidthTextColumn) col
                                                                     orderby dynamicCol.Index descending
                                                                     select dynamicCol).ToList();
            foreach (IDynamicWidthTextColumn column in dynamicColumns.Where(col => col.LeftPadding == null)
                                                                     .ToList())
            {
                column.LeftPadding = column.First && !_outerBorders
                                         ? 0
                                         : _defaultColumnPadding;
            }

            foreach (IDynamicWidthTextColumn column in dynamicColumns.Where(col => col.RightPadding == null)
                                                                     .ToList())
            {
                column.RightPadding = column.Last && !_outerBorders
                                          ? 0
                                          : _defaultColumnPadding;
            }


            foreach (IDynamicWidthTextColumn dynamicColumn in dynamicColumns)
            {
                if (dynamicColumn.RightPadding is > 0) _columns.Insert(dynamicColumn.Index + 1, new PaddingTextColumn(dynamicColumn.RightPadding.Value));

                // ReSharper disable once InvertIf
                if (dynamicColumn.LeftPadding > 0) _columns.Insert(dynamicColumn.Index, new PaddingTextColumn(dynamicColumn.LeftPadding.Value));
            }
        }

        // Resolve widths
        ComputeMeta();
        List<DataTextRow> dynamicRows = _rows.OfType<DataTextRow>().ToList();
        foreach (ITextColumn col in _columns)
        {
            IReadOnlyList<int> partitionLengths = (from row in dynamicRows
                                                   from partition in col.Lines(row)
                                                   select partition.Length).ToList();
            int dataWidth = partitionLengths.Any() ? partitionLengths.Max() : 0;
            bool hasHeading = Rows.Any(r => r is HeadingTextRow);
            if (col is IDynamicWidthTextColumn dynamicCol)
            {
                dynamicCol.MaximumWidth ??= _defaultColumnMaxWidth;
                int headingWidth = hasHeading ? dynamicCol.Heading?.Length ?? 0 : 0;
                dynamicCol.Width = Math.Max(dynamicCol.MinimumWidth, Math.Max(headingWidth, dataWidth));
            }
            else
                col.Width = dataWidth;
        }

        // Resolve outer border
        ComputeMeta();
        if (!_layoutBorderComputed && _outerBorders)
        {
            if (_columns[0] is not IBorderTextColumn) _columns.Insert(0, new SingleBorderTextColumn());

            if (_columns[_columns.Count - 1] is not IBorderTextColumn) _columns.Add(new SingleBorderTextColumn());

            if (_rows[0] is not IBorderTextRow) _rows.Insert(0, new SingleBorderTextRow());

            if (_rows[_rows.Count - 1] is not IBorderTextRow) _rows.Add(new SingleBorderTextRow());
        }

        ComputeMeta();

        _layoutBorderComputed = true;
    }

    private void ComputeMeta()
    {
        int index = 0;
        foreach (ITextColumn col in _columns)
        {
            col.First = index == 0;
            col.Last = index == _columns.Count - 1;
            col.Index = index++;
        }

        index = 0;
        foreach (ITextRow row in _rows)
        {
            row.First = index == 0;
            row.Last = index == _rows.Count - 1;
            row.Index = index++;
        }
    }
}

public class TextRowBuilder : ITextRowBuilder
{
    public TextRowBuilder(TextTableBuilder table, List<ITextRow> rows, IReadOnlyList<ITextColumn> columns)
    {
        _table = table;
        _rows = rows;

        List<TextColumn> textColumns = columns.OfType<TextColumn>().ToList();
        _dataColumnCount = textColumns.Any() ? textColumns.Max(i => i.DataColumnIndex) + 1 : 0;
    }

    private readonly int _dataColumnCount;
    private int _rowId = 1;
    private readonly List<ITextRow> _rows;
    private readonly TextTableBuilder _table;

    public IReadOnlyList<ITextRow> Rows => _rows;

    public ITextRowBuilder AddBorderRow()
    {
        _rows.Add(new SingleBorderTextRow { External = true });

        return this;
    }

    public ITextRowBuilder AddDataRow(params object[] cells)
    {
        return AddDataRow((IReadOnlyList<object>) cells);
    }

    public ITextRowBuilder AddDataRow(IReadOnlyList<object> row, int? rowId = null)
    {
        if (row.Count != _dataColumnCount) throw new Exception($"row columns ({row.Count}) does not match scheme columns ({_dataColumnCount})");

        _rows.Add(new DataTextRow(rowId ?? _rowId++, row));

        return this;
    }

    public ITextRowBuilder AddDataRows(IReadOnlyList<IReadOnlyList<object>> rows, int? startRowId = null)
    {
        foreach (IReadOnlyList<object> row in rows)
        {
            AddDataRow(row, startRowId == null ? null : startRowId++);
        }

        return this;
    }

    public ITextRowBuilder AddDoubleBorderRow()
    {
        _rows.Add(new DoubleBorderTextRow { External = true });

        return this;
    }

    public ITextRowBuilder AddEllipsisRow(string indicator = "...", int indicatorColumnIndex = 0)
    {
        _rows.Add(new EllipsisTextRow {
                                          Indicator = indicator,
                                          ColumnIndex = indicatorColumnIndex
                                      });

        return this;
    }

    public ITextRowBuilder AddHeadingRow()
    {
        _rows.Add(new HeadingTextRow());

        return this;
    }

    public ITextRowBuilder AddHeadingRow(params string[] headingOverrides)
    {
        _table.AddHeadingRow(headingOverrides);

        return this;
    }

    public ITextRowBuilder ClearRows()
    {
        _rows.RemoveAll(r => r is DataTextRow);
        ResetLayout();

        return this;
    }

    public ITextRowBuilder InsertRow(int index, ITextRow row)
    {
        _rows.Insert(index, row);
        ResetLayout();

        return this;
    }

    public ITextRowBuilder RemoveRow(int index)
    {
        _rows.RemoveAt(index);
        ResetLayout();

        return this;
    }

    public string Render()
    {
        return _table.Render();
    }

    public override string ToString()
    {
        return _table.ToString();
    }

    private void ResetLayout()
    {
        _table.ResetLayout();
        _rowId = 1;
        _rows.Where(r => r is DataTextRow)
             .ForEach(r => r.Id = _rowId++);
    }
}

public interface ITextRowBuilder
{
    public IReadOnlyList<ITextRow> Rows { get; }

    ITextRowBuilder AddBorderRow();

    ITextRowBuilder AddDataRow(params object[] cells);

    ITextRowBuilder AddDataRow(IReadOnlyList<object> row, int? rowId = null);

    ITextRowBuilder AddDataRows(IReadOnlyList<IReadOnlyList<object>> rows, int? startRowId = null);

    ITextRowBuilder AddDoubleBorderRow();

    ITextRowBuilder AddEllipsisRow(string indicator = "...", int indicatorColumnIndex = 0);

    ITextRowBuilder AddHeadingRow();

    ITextRowBuilder AddHeadingRow(params string[] headingOverrides);

    ITextRowBuilder ClearRows();

    public ITextRowBuilder InsertRow(int index, ITextRow row);

    public ITextRowBuilder RemoveRow(int index);

    string Render();
}

/*************************************************************
 * Column Types
 *************************************************************/

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

public interface IDynamicWidthTextColumn : ITextColumn
{
    string Heading { get; }

    int? LeftPadding { get; set; }

    int? MaximumWidth { get; set; }

    int MinimumWidth { get; }

    int? RightPadding { get; set; }
}

public interface IBorderTextColumn : ITextColumn
{
    public bool External { get; }
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

    public string FormatValue(ITextRow row)
    {
        if (row is not DataTextRow dataRow) return string.Empty;

        object rawData = dataRow[DataColumnIndex] ?? NullProjection;
        return Format == null
                   ? rawData?.ToString() ?? string.Empty
                   : string.Format($"{{0:{Format}}}", rawData);
    }

    public override IReadOnlyList<string> Lines(ITextRow row)
    {
        if (_lineCache.TryGetValue(row, out IReadOnlyList<string> cachedLines)) return cachedLines;

        string formattedContent = FormatValue(row);

        string[] rawLines = formattedContent.Split([Environment.NewLine, "\n"], StringSplitOptions.None);

        if (MaximumWidth == null) return _lineCache[row] = rawLines;

        if (rawLines.Length == 1 && rawLines[0].Length < MaximumWidth) return _lineCache[row] = rawLines;

        return _lineCache[row] = rawLines.SelectMany(line => line.Wrap(MaximumWidth.Value)).ToList();
    }
}

public class SingleBorderTextColumn : TextColumnBase, IBorderTextColumn
{
    private const char _verticalBar = '│';
    private IReadOnlyList<string> _lines;

    public bool External { get; init; }

    public string FormatValue(ITextRow row)
    {
        return _verticalBar.ToString();
    }

    public override IReadOnlyList<string> Lines(ITextRow row)
    {
        return _lines ??= new[] { FormatValue(row) };
    }

    public override void Render(StringBuilder output, ITextRow row, int lineIndex)
    {
        if (row is IBorderTextRow borderRow)
        {
            output.Append(borderRow.SingleJunction(this, row).ToString());

            return;
        }

        output.Append(_verticalBar);
    }

    public override void RenderHeading(StringBuilder output, string headingOverride = null)
    {
        output.Append(_verticalBar);
    }
}

public class DoubleBorderTextColumn : TextColumnBase, IBorderTextColumn
{
    private const char _verticalBar = '║';
    private IReadOnlyList<string> _lines;

    public bool External { get; init; }

    public virtual string FormatValue(ITextRow row)
    {
        return _verticalBar.ToString();
    }

    public override IReadOnlyList<string> Lines(ITextRow row)
    {
        return _lines ??= new[] { FormatValue(row) };
    }

    public override void Render(StringBuilder output, ITextRow row, int lineIndex)
    {
        if (row is IBorderTextRow borderRow)
        {
            output.Append(borderRow.DoubleJunction(this, row).ToString());

            return;
        }

        output.Append(_verticalBar);
    }

    public override void RenderHeading(StringBuilder output, string headingOverride = null)
    {
        output.Append(_verticalBar);
    }
}

public class IdentityTextColumn : TextColumnBase, IDynamicWidthTextColumn
{
    public int? LeftPadding { get; set; }

    public int? MaximumWidth { get; set; }

    public int MinimumWidth => 1;

    public int? RightPadding { get; set; }

    public virtual string FormatValue(ITextRow row)
    {
        return (row.Id ?? 0).ToString("N0").PadLeft(Width);
    }

    public override IReadOnlyList<string> Lines(ITextRow row)
    {
        return new[] { FormatValue(row) };
    }
}

public class PaddingTextColumn(int width) : TextColumnBase
{
    public override IReadOnlyList<string> Lines(ITextRow row)
    {
        return new[] { new string(Space, width) };
    }

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
    {
        return Text;
    }

    public override IReadOnlyList<string> Lines(ITextRow row)
    {
        return _lines ??= new[] { Text };
    }

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

/*************************************************************
 * Rows Types
 *************************************************************/

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
    public HeadingTextRow()
    { }

    public HeadingTextRow(string[] headingOverrides)
    {
        _headingOverrides = headingOverrides;
    }

    private readonly string[] _headingOverrides;

    public bool First { get; set; }

    public int? Id {
        get => null;
        set { }
    }

    public int Index { get; set; }

    public bool Last { get; set; }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns)
    {
        if (columns.OfType<TextColumnBase>().All(c => string.IsNullOrEmpty(c.Heading))
            && !_headingOverrides.Any())
            return;

        int index = 0;
        foreach (ITextColumn col in columns)
        {
            if (col is IDynamicWidthTextColumn)
            {
                col.RenderHeading(output, _headingOverrides?.ElementAtOrDefault(index));
                index++;
            }
            else
                col.RenderHeading(output);
        }

        output.AppendLine();
    }
}

public class EllipsisTextRow : ITextRow
{
    public int ColumnIndex { get; set; }

    public bool First { get; set; }

    public int? Id {
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
    private static readonly char[,] _doubleJunctionChars = {
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
    private static readonly char[,] _singleJunctionChars = {
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

    public int? Id {
        get => null;
        set { }
    }

    public int Index { get; set; }

    public bool Last { get; set; }

    public char DoubleJunction(ITextColumn col, ITextRow row)
    {
        return _doubleJunctionChars[row.First
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
        return _singleJunctionChars[row.First
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
}

public class DoubleBorderTextRow : IBorderTextRow
{
    public const char DashChar = '═';
    private static readonly char[,] _doubleJunctionChars = {
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
    private static readonly char[,] _singleJunctionChars = {
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

    public int? Id {
        get => null;
        set { }
    }

    public int Index { get; set; }

    public bool Last { get; set; }

    public char DoubleJunction(ITextColumn col, ITextRow row)
    {
        return _doubleJunctionChars[row.First
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
        return _singleJunctionChars[row.First
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
}

[Flags]
public enum TextTableBorder
{
    None = 0,
    Column = 1 << 2,
    Row = 1 << 3,
    Outer = 1 << 4,
    InnerFull = Row | Column,
    Inner = 1 << 5,
    Default = None,
    All = int.MaxValue
}