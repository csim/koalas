namespace Koalas;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class TextTableBuilder : ITextRowBuilder {
    public TextTableBuilder(TextTableBorder border = TextTableBorder.Default, int? defaultColumnPadding = null, int? defaultColumnMaxWidth = null) {
        _outerBorders = border.HasFlag(TextTableBorder.Outer);
        _columnBorders = border.HasFlag(TextTableBorder.Column);
        _rowBorders = border.HasFlag(TextTableBorder.Row);
        _defaultColumnPadding = defaultColumnPadding ?? 1;
        _defaultColumnMaxWidth = defaultColumnMaxWidth ?? int.MaxValue;
    }

    private readonly bool _columnBorders;
    private readonly List<ITextColumn> _columns = new();
    private int _currentDataColumnIndex;
    private readonly int _defaultColumnMaxWidth;
    private readonly int _defaultColumnPadding;
    private readonly bool _outerBorders;
    private readonly bool _rowBorders;
    private readonly List<ITextRow> _rows = new();

    public static TextTableBuilder Create(TextTableBorder border = TextTableBorder.Default,
                                          int? defaultColumnPadding = null,
                                          int? defaultColumnMaxWidth = null) {
        return new TextTableBuilder(border, defaultColumnPadding, defaultColumnMaxWidth);
    }
}

public partial class TextTableBuilder {
    public TextTableBuilder AddBorderColumn() {
        _columns.Add(new SingleBorderTextColumn());

        return this;
    }

    public TextTableBuilder AddColumn(string heading = "",
                                      int minWidth = 0,
                                      int? maxWidth = null,
                                      bool alignRight = false,
                                      string format = null,
                                      string nullProjection = null,
                                      int? leftPadding = null,
                                      int? rightPadding = null) {
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

    public TextTableBuilder AddDoubleBorderColumn() {
        _columns.Add(new DoubleBorderTextColumn());

        return this;
    }

    public TextTableBuilder AddIdentityColumn(string heading = "", int? leftPadding = null, int? rightPadding = null) {
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
                                            int? rightPadding = null) {
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
                                            bool showInRowExtraLines = false) {
        _columns.Add(new StaticTextColumn {
                                              Text = value,
                                              ShowInHeading = showInHeading,
                                              ShowInRow = showInRow,
                                              ShowInRowExtraLines = showInRowExtraLines
                                          });

        return this;
    }
}

public partial class TextTableBuilder {
    private TextRowBuilder _rowBuilder;

    public TextRowBuilder AddBorderRow() {
        return RowBuilder().AddBorderRow();
    }

    public TextRowBuilder AddDataRow(params object[] cells) {
        return RowBuilder().AddDataRow((IReadOnlyList<object>)cells);
    }

    public TextRowBuilder AddDataRow(IReadOnlyList<object> row, int? rowId = null) {
        return RowBuilder().AddDataRow(row, rowId);
    }

    public TextRowBuilder AddDataRows(IReadOnlyList<IReadOnlyList<object>> rows, int? startRowId = null) {
        return RowBuilder().AddDataRows(rows, startRowId);
    }

    public TextRowBuilder AddDoubleBorderRow() {
        return RowBuilder().AddDoubleBorderRow();
    }

    public TextRowBuilder AddEllipsisRow(string indicator = "...", int indicatorColumnIndex = 0) {
        return RowBuilder().AddEllipsisRow(indicator, indicatorColumnIndex);
    }

    public TextRowBuilder AddHeadingRow() {
        return RowBuilder().AddHeadingRow();
    }

    private TextRowBuilder RowBuilder() {
        return _rowBuilder ??= new TextRowBuilder(this, _rows, _columns);
    }
}

public partial class TextTableBuilder {
    public string Render() {
        if (!_columns.Any() || !_rows.Any()) {
            return string.Empty;
        }

        ComputeLayout();

        StringBuilder output = new();

        foreach (ITextRow row in _rows) {
            row.Render(output, _columns);
        }

        return output.ToString();
    }

    public override string ToString() {
        return Render();
    }

    public int Width() {
        ComputeLayout();

        return _columns.Select(c => c.Width).Sum();
    }

    private void ComputeLayout() {
        // Resolve inner borders
        if (_columnBorders) {
            ComputeMeta();
            IEnumerable<int> dataColumnIndexes = from col in _columns
                                                 where col is IDynamicWidthTextColumn
                                                       && !col.First
                                                       && _columns[col.Index - 1] is not IBorderTextColumn
                                                 orderby col.Index descending
                                                 select col.Index;
            foreach (int index in dataColumnIndexes.ToList()) {
                _columns.Insert(index, new SingleBorderTextColumn());
            }
        }

        if (_rowBorders) {
            ComputeMeta();
            IEnumerable<int> dataRowIndexes = from row in _rows
                                              where row is DataTextRow
                                                    && !row.First
                                                    && _rows[row.Index - 1] is not IBorderTextRow
                                              orderby row.Index descending
                                              select row.Index;
            foreach (int index in dataRowIndexes.ToList()) {
                _rows.Insert(index, new SingleBorderTextRow());
            }
        }

        // Resolve padding
        ComputeMeta();
        IReadOnlyList<IDynamicWidthTextColumn> dynamicColumns = (from col in _columns
                                                                 where col is IDynamicWidthTextColumn
                                                                 let dynamicCol = (IDynamicWidthTextColumn)col
                                                                 orderby dynamicCol.Index descending
                                                                 select dynamicCol).ToList();
        foreach (IDynamicWidthTextColumn column in dynamicColumns.Where(col => col.LeftPadding == null)
                                                                 .ToList()) {
            column.LeftPadding = column.First && !_outerBorders
                                     ? 0
                                     : _defaultColumnPadding;
        }

        foreach (IDynamicWidthTextColumn column in dynamicColumns.Where(col => col.RightPadding == null)
                                                                 .ToList()) {
            column.RightPadding = column.Last && !_outerBorders
                                      ? 0
                                      : _defaultColumnPadding;
        }

        foreach (IDynamicWidthTextColumn dynamicColumn in dynamicColumns) {
            if (dynamicColumn.RightPadding is > 0) {
                _columns.Insert(dynamicColumn.Index + 1, new PaddingTextColumn(dynamicColumn.RightPadding.Value));
            }

            if (dynamicColumn.LeftPadding > 0) {
                _columns.Insert(dynamicColumn.Index, new PaddingTextColumn(dynamicColumn.LeftPadding.Value));
            }
        }

        // Resolve widths
        ComputeMeta();
        var dynamicRows = _rows.OfType<DataTextRow>().ToList();
        foreach (ITextColumn col in _columns) {
            IReadOnlyList<int> partitionLengths = (from row in dynamicRows
                                                   from partition in col.Lines(row)
                                                   select partition.Length).ToList();
            int dataWidth = partitionLengths.Any() ? partitionLengths.Max() : 0;
            if (col is IDynamicWidthTextColumn dynamicCol) {
                dynamicCol.MaximumWidth ??= _defaultColumnMaxWidth;
                dynamicCol.Width = Math.Max(dynamicCol.MinimumWidth, Math.Max(dynamicCol.Heading?.Length ?? 0, dataWidth));
            }
            else {
                col.Width = dataWidth;
            }
        }

        // Resolve outer border
        ComputeMeta();
        if (_outerBorders) {
            if (_columns[0] is not IBorderTextColumn) {
                _columns.Insert(0, new SingleBorderTextColumn());
            }

            if (_columns[^1] is not IBorderTextColumn) {
                _columns.Add(new SingleBorderTextColumn());
            }

            if (_rows[0] is not IBorderTextRow) {
                _rows.Insert(0, new SingleBorderTextRow());
            }

            if (_rows[^1] is not IBorderTextRow) {
                _rows.Add(new SingleBorderTextRow());
            }
        }

        ComputeMeta();
    }

    private void ComputeMeta() {
        var index = 0;
        foreach (ITextColumn col in _columns) {
            col.First = index == 0;
            col.Last = index == _columns.Count - 1;
            col.Index = index++;
        }

        index = 0;
        foreach (ITextRow row in _rows) {
            row.First = index == 0;
            row.Last = index == _rows.Count - 1;
            row.Index = index++;
        }
    }
}

public class TextRowBuilder {
    public TextRowBuilder(TextTableBuilder table, List<ITextRow> rows, IReadOnlyList<ITextColumn> columns) {
        _table = table;
        _rows = rows;
        _columns = columns;

        var textColumns = _columns.OfType<TextColumn>().ToList();
        _dataColumnCount = textColumns.Any() ? textColumns.Max(i => i.DataColumnIndex) + 1 : 0;
    }

    private readonly IReadOnlyList<ITextColumn> _columns;
    private readonly int _dataColumnCount;
    private int _rowId = 1;
    private readonly List<ITextRow> _rows;
    private readonly TextTableBuilder _table;

    public TextRowBuilder AddBorderRow() {
        _rows.Add(new SingleBorderTextRow());

        return this;
    }

    public TextRowBuilder AddDataRow(params object[] cells) {
        return AddDataRow((IReadOnlyList<object>)cells);
    }

    public TextRowBuilder AddDataRow(IReadOnlyList<object> row, int? rowId = null) {
        if (row.Count != _dataColumnCount) {
            throw new Exception($"row columns ({row.Count}) does not match scheme columns ({_columns.Count})");
        }

        _rows.Add(new DataTextRow(rowId ?? _rowId++, row));

        return this;
    }

    public TextRowBuilder AddDataRows(IReadOnlyList<IReadOnlyList<object>> rows, int? startRowId = null) {
        foreach (IReadOnlyList<object> row in rows) {
            AddDataRow(row, startRowId == null ? null : startRowId++);
        }

        return this;
    }

    public TextRowBuilder AddDoubleBorderRow() {
        _rows.Add(new DoubleBorderTextRow());

        return this;
    }

    public TextRowBuilder AddEllipsisRow(string indicator = "...", int indicatorColumnIndex = 0) {
        _rows.Add(new EllipsisTextRow {
                                          Indicator = indicator,
                                          ColumnIndex = indicatorColumnIndex
                                      });

        return this;
    }

    public TextRowBuilder AddHeadingRow() {
        _rows.Add(new HeadingTextRow());

        return this;
    }

    public string Render() {
        return _table.Render();
    }

    public override string ToString() {
        return _table.ToString();
    }
}

public interface ITextRowBuilder {
    TextRowBuilder AddBorderRow();

    TextRowBuilder AddDataRow(params object[] cells);

    TextRowBuilder AddDataRow(IReadOnlyList<object> row, int? rowId = null);

    TextRowBuilder AddDataRows(IReadOnlyList<IReadOnlyList<object>> rows, int? startRowId = null);

    TextRowBuilder AddDoubleBorderRow();

    TextRowBuilder AddEllipsisRow(string indicator = "...", int indicatorColumnIndex = 0);

    TextRowBuilder AddHeadingRow();
}

/*************************************************************
 * Column Types
 *************************************************************/

public interface ITextColumn {
    bool First { get; set; }

    int Index { get; set; }

    bool Last { get; set; }

    int Width { get; set; }

    IReadOnlyList<string> Lines(ITextRow row);

    void Render(StringBuilder output, ITextRow row, int partitionIndex);
}

public interface IDynamicWidthTextColumn : ITextColumn {
    string Heading { get; }

    int? LeftPadding { get; set; }

    int? MaximumWidth { get; set; }

    int MinimumWidth { get; }

    int? RightPadding { get; set; }
}

public interface IBorderTextColumn : ITextColumn { }

public abstract class TextColumnBase : ITextColumn {
    public static char Space = ' ';

    public bool AlignRight { get; init; }

    public bool First { get; set; }

    public string Heading { get; set; } = string.Empty;

    public int Index { get; set; }

    public bool Last { get; set; }

    public int Width { get; set; }

    public abstract IReadOnlyList<string> Lines(ITextRow row);

    public virtual void Render(StringBuilder output, ITextRow row, int lineIndex) {
        if (row is HeadingTextRow) {
            output.Append(AlignRight ? Heading.PadLeft(Width) : Heading.PadRight(Width));

            return;
        }

        if (row is IBorderTextRow borderRow) {
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
}

public class TextColumn : TextColumnBase, IDynamicWidthTextColumn {
    private readonly Dictionary<ITextRow, IReadOnlyList<string>> _lineCache = new();

    public int DataColumnIndex { get; init; }

    public string Format { get; init; }

    public int? LeftPadding { get; set; }

    public int? MaximumWidth { get; set; }

    public int MinimumWidth { get; init; }

    public string NullProjection { get; init; }

    public int? RightPadding { get; set; }

    public string FormatValue(ITextRow row) {
        if (row is not DataTextRow dataRow) {
            return string.Empty;
        }

        object rawData = dataRow[DataColumnIndex] ?? NullProjection;
        return Format == null
                   ? rawData?.ToString() ?? string.Empty
                   : string.Format($"{{0:{Format}}}", rawData);
    }

    public override IReadOnlyList<string> Lines(ITextRow row) {
        if (_lineCache.TryGetValue(row, out IReadOnlyList<string> cachedLines)) {
            return cachedLines;
        }

        string formattedContent = FormatValue(row);

        string[] rawLines = formattedContent.Contains(Environment.NewLine)
                                ? formattedContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                                : new[] { formattedContent };

        if (MaximumWidth == null) {
            return _lineCache[row] = rawLines;
        }

        if (rawLines.Length == 1 && rawLines[0].Length < MaximumWidth) {
            return _lineCache[row] = rawLines;
        }

        return _lineCache[row] = rawLines.SelectMany(line => Partition(line, MaximumWidth.Value)).ToList();
    }

    private static IReadOnlyList<string> Partition(string text, int maxLength) {
        List<string> chunks = new();

        if (string.IsNullOrEmpty(text)) {
            chunks.Add(string.Empty);

            return chunks;
        }

        while (text.Length > 0) {
            if (text.Length <= maxLength) // If remaining string is less than length, add to list and break out of loop
            {
                chunks.Add(text);
                break;
            }

            string chunk = text.Substring(0, maxLength); // Get maxLength chunk from string.

            if (char.IsWhiteSpace(text[maxLength])) // If next char is a space, we can use the whole chunk and remove the space for the next line
            {
                chunks.Add(chunk);
                text = text.Substring(chunk.Length + 1); // Remove chunk plus space from original string
            }
            else {
                int splitIndex = chunk.LastIndexOf(Space); // Find last space in chunk.
                if (splitIndex != -1)                      // If space exists in string,
                {
                    chunk = chunk.Substring(0, splitIndex); // Remove chars after space.
                }

                text = text.Substring(chunk.Length + (splitIndex == -1 ? 0 : 1)); // Remove chunk plus space (if found) from original string
                chunks.Add(chunk);
            }
        }

        return chunks;
    }
}

public class SingleBorderTextColumn : TextColumnBase, IBorderTextColumn {
    private const char _verticalBar = '│';
    private IReadOnlyList<string> _lines;

    public string FormatValue(ITextRow row) {
        return _verticalBar.ToString();
    }

    public override IReadOnlyList<string> Lines(ITextRow row) {
        return _lines ??= new[] { FormatValue(row) };
    }

    public override void Render(StringBuilder output, ITextRow row, int lineIndex) {
        if (row is IBorderTextRow borderRow) {
            output.Append(borderRow.SingleJunction(this, row).ToString());

            return;
        }

        output.Append(_verticalBar);
    }
}

public class DoubleBorderTextColumn : TextColumnBase, IBorderTextColumn {
    private const char _verticalBar = '║';
    private IReadOnlyList<string> _lines;

    public virtual string FormatValue(ITextRow row) {
        return _verticalBar.ToString();
    }

    public override IReadOnlyList<string> Lines(ITextRow row) {
        return _lines ??= new[] { FormatValue(row) };
    }

    public override void Render(StringBuilder output, ITextRow row, int lineIndex) {
        if (row is IBorderTextRow borderRow) {
            output.Append(borderRow.DoubleJunction(this, row).ToString());

            return;
        }

        output.Append(_verticalBar);
    }
}

public class IdentityTextColumn : TextColumnBase, IDynamicWidthTextColumn {
    public int? LeftPadding { get; set; }

    public int? MaximumWidth { get; set; }

    public int MinimumWidth => 1;

    public int? RightPadding { get; set; }

    public virtual string FormatValue(ITextRow row) {
        return (row.Id ?? 0).ToString("N0").PadLeft(Width);
    }

    public override IReadOnlyList<string> Lines(ITextRow row) {
        return new[] { FormatValue(row) };
    }
}

public class PaddingTextColumn : TextColumnBase {
    public PaddingTextColumn(int width) {
        _width = width;
    }

    private readonly int _width;

    public override IReadOnlyList<string> Lines(ITextRow row) {
        return new[] { new string(Space, _width) };
    }

    public override void Render(StringBuilder output, ITextRow row, int lineIndex) {
        if (row is IBorderTextRow borderRow) {
            output.Append(borderRow.Dash, Width);

            return;
        }

        output.Append(Space, _width);
    }
}

public class StaticTextColumn : TextColumnBase {
    private IReadOnlyList<string> _lines;

    public bool ShowInHeading { get; init; }

    public bool ShowInRow { get; init; }

    public bool ShowInRowExtraLines { get; init; }

    public string Text { get; init; }

    public virtual string FormatValue(ITextRow row) {
        return Text;
    }

    public override IReadOnlyList<string> Lines(ITextRow row) {
        return _lines ??= new[] { Text };
    }

    public override void Render(StringBuilder output, ITextRow row, int lineIndex) {
        string blank = new(Space, Width);
        string fdata = FormatValue(row);

        if (row is HeadingTextRow) {
            output.Append(ShowInHeading ? fdata : blank);

            return;
        }

        if (row is IBorderTextRow borderRow) {
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

public interface ITextRow {
    bool First { get; set; }

    int? Id { get; }

    int Index { get; set; }

    bool Last { get; set; }

    void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns);
}

public interface IBorderTextRow : ITextRow {
    char Dash { get; }

    public char DoubleJunction(ITextColumn col, ITextRow row);

    public char SingleJunction(ITextColumn col, ITextRow row);
}

public class DataTextRow : List<object>, ITextRow {
    public DataTextRow(int id, IEnumerable<object> values) : base(values) {
        Id = id;
    }

    public bool First { get; set; }

    public int? Id { get; }

    public int Index { get; set; }

    public bool Last { get; set; }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns) {
        int lineIndexMax = columns.Select(c => c.Lines(this).Count).Max();

        for (var lineIndex = 0; lineIndex < lineIndexMax; lineIndex++) {
            foreach (ITextColumn col in columns) {
                col.Render(output, this, lineIndex);
            }

            output.AppendLine();
        }
    }
}

public class HeadingTextRow : ITextRow {
    public bool First { get; set; }

    public int? Id => null;

    public int Index { get; set; }

    public bool Last { get; set; }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns) {
        if (columns.OfType<TextColumnBase>().All(c => string.IsNullOrEmpty(c.Heading))) {
            return;
        }

        foreach (ITextColumn col in columns) {
            col.Render(output, this, 0);
        }

        output.AppendLine();
    }
}

public class EllipsisTextRow : ITextRow {
    public int ColumnIndex { get; set; }

    public bool First { get; set; }

    public int? Id => null;

    public int Index { get; set; }

    public string Indicator { get; set; } = "...";

    public bool Last { get; set; }

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns) {
        var index = 0;

        foreach (ITextColumn col in columns) {
            output.Append(index == ColumnIndex
                              ? Indicator
                              : new string(' ', col.Width));
            index++;
        }

        output.AppendLine();
    }
}

public class SingleBorderTextRow : IBorderTextRow {
    public const char _dashChar = '─';
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

    public char Dash => _dashChar;

    public bool First { get; set; }

    public int? Id => null;

    public int Index { get; set; }

    public bool Last { get; set; }

    public char DoubleJunction(ITextColumn col, ITextRow row) {
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

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns) {
        foreach (ITextColumn col in columns) {
            col.Render(output, this, 0);
        }

        output.AppendLine();
    }

    public char SingleJunction(ITextColumn col, ITextRow row) {
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

public class DoubleBorderTextRow : IBorderTextRow {
    public const char _dashChar = '═';
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

    public char Dash => _dashChar;

    public bool First { get; set; }

    public int? Id => null;

    public int Index { get; set; }

    public bool Last { get; set; }

    public char DoubleJunction(ITextColumn col, ITextRow row) {
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

    public void Render(StringBuilder output, IReadOnlyList<ITextColumn> columns) {
        foreach (ITextColumn col in columns) {
            col.Render(output, this, 0);
        }

        output.AppendLine();
    }

    public char SingleJunction(ITextColumn col, ITextRow row) {
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
public enum TextTableBorder {
    None = 0,
    Column = 1 << 2,
    Row = 1 << 3,
    Outer = 1 << 4,
    Inner = Row | Column,
    Default = None,
    All = int.MaxValue
}