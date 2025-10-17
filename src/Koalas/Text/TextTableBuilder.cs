namespace Koalas.Text;

/*************************************************************
 * Column Types
 *************************************************************/
public interface IDynamicWidthTextColumn : ITextColumn
{
    public string Heading { get; }
    public int? LeftPadding { get; set; }
    public int? MaximumWidth { get; set; }
    public int MinimumWidth { get; }
    public int? RightPadding { get; set; }
}

public interface ITextRowBuilder : IRender
{
    public IReadOnlyList<ITextRow> Rows { get; }

    public ITextRowBuilder AddBorderRow();
    public ITextRowBuilder AddDataRow(params object[] cells);
    public ITextRowBuilder AddDataRow(IReadOnlyList<object?> row, int? rowId = null);
    public ITextRowBuilder AddDataRows(
        IReadOnlyList<IReadOnlyList<object?>> rows,
        int? startRowId = null
    );
    public ITextRowBuilder AddDoubleBorderRow();
    public ITextRowBuilder AddEllipsisRow(string indicator = "...", int indicatorColumnIndex = 0);
    public ITextRowBuilder AddHeadingRow();
    public ITextRowBuilder AddHeadingRow(params string[] headingOverrides);
    public ITextRowBuilder ClearRows();
    public ITextRowBuilder InsertRow(int index, ITextRow row);
    public ITextRowBuilder RemoveRow(int index);
    public TextBuilder SaveTable();
}

//_layoutBorderComputed = false;
public class TextRowBuilder : ITextRowBuilder
{
    public IReadOnlyList<ITextRow> Rows => _rows;

    private readonly int _dataColumnCount;
    private int _rowId = 1;
    private readonly List<ITextRow> _rows;
    private readonly TextTableBuilder _table;

    public TextRowBuilder(
        TextTableBuilder table,
        List<ITextRow> rows,
        IReadOnlyList<ITextColumn> columns
    )
    {
        _table = table;
        _rows = rows;

        List<TextColumn> textColumns = [.. columns.OfType<TextColumn>()];
        _dataColumnCount =
            textColumns.Count != 0 ? textColumns.Max(static i => i.DataColumnIndex) + 1 : 0;
    }

    public ITextRowBuilder AddBorderRow()
    {
        _rows.Add(new SingleBorderTextRow { External = true });

        return this;
    }

    public ITextRowBuilder AddDataRow(params object[] cells)
    {
        return AddDataRow((IReadOnlyList<object>)cells);
    }

    public ITextRowBuilder AddDataRow(IReadOnlyList<object?> row, int? rowId = null)
    {
        if (row.Count != _dataColumnCount)
        {
            throw new InvalidOperationException(
                $"row columns ({row.Count}) does not match scheme columns ({_dataColumnCount})"
            );
        }

        _rows.Add(new DataTextRow(rowId ?? _rowId++, row));

        return this;
    }

    public ITextRowBuilder AddDataRows(
        IReadOnlyList<IReadOnlyList<object?>> rows,
        int? startRowId = null
    )
    {
        foreach (IReadOnlyList<object?> row in rows)
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
        _rows.Add(
            new EllipsisTextRow { Indicator = indicator, ColumnIndex = indicatorColumnIndex }
        );

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
        _rows.RemoveAll(static r => r is DataTextRow);
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

    public TextBuilder SaveTable()
    {
        return _table.SaveTable();
    }

    public override string ToString()
    {
        return _table.ToString();
    }

    private void ResetLayout()
    {
        _table.ResetLayout();
        _rowId = 1;
        _rows.Where(r => r is DataTextRow).ForEach(r => r.Id = _rowId++);
    }
}

public partial class TextTableBuilder : ITextBuilder, ITextRowBuilder
{
    public IReadOnlyList<ITextRow> Rows => _rows;
    private ITextRowBuilder RowBuilder => _rowBuilder ??= new TextRowBuilder(this, _rows, _columns);

    private TextTableBorder _border = TextTableBorder.None;
    private readonly List<ITextColumn> _columns = [];
    private int _currentDataColumnIndex;
    private int? _defaultColumnMaxWidth;
    private int _defaultColumnPadding = 1;
    private readonly TextBuilder _parent;
    private TextRowBuilder? _rowBuilder;
    private int? _rowLimit;
    private readonly List<ITextRow> _rows = [];
    private bool _saved;

    internal TextTableBuilder(TextBuilder parent)
    {
        _parent = parent;
    }

    public TextTableBuilder AddBorderColumn()
    {
        _columns.Add(new SingleBorderTextColumn { External = true });

        return this;
    }

    public ITextRowBuilder AddBorderRow()
    {
        return RowBuilder.AddBorderRow();
    }

    public TextTableBuilder AddColumn(
        string heading = "",
        int minWidth = 0,
        int? maxWidth = null,
        int? wrapOverflowIndent = null,
        bool alignRight = false,
        string? format = null,
        string? nullProjection = null,
        int? leftPadding = null,
        int? rightPadding = null
    )
    {
        _columns.Add(
            new TextColumn
            {
                Heading = heading,
                MinimumWidth = minWidth,
                MaximumWidth = maxWidth,
                WrapOverflowIndent = wrapOverflowIndent ?? 0,
                DataColumnIndex = _currentDataColumnIndex++,
                Format = format,
                AlignRight = alignRight,
                NullProjection = nullProjection,
                LeftPadding = leftPadding,
                RightPadding = rightPadding,
            }
        );

        return this;
    }

    public ITextRowBuilder AddDataRow(params object?[] cells)
    {
        return RowBuilder.AddDataRow((IReadOnlyList<object>)cells);
    }

    public ITextRowBuilder AddDataRow(IReadOnlyList<object?> row, int? rowId = null)
    {
        return RowBuilder.AddDataRow(row, rowId);
    }

    public ITextRowBuilder AddDataRows(
        IReadOnlyList<IReadOnlyList<object?>> rows,
        int? startRowId = null
    )
    {
        return RowBuilder.AddDataRows(rows, startRowId);
    }

    public TextTableBuilder AddDoubleBorderColumn()
    {
        _columns.Add(new DoubleBorderTextColumn { External = true });

        return this;
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

    public TextTableBuilder AddIdentityColumn(
        string heading = "",
        int? leftPadding = null,
        int? rightPadding = null
    )
    {
        _columns.Add(
            new IdentityTextColumn
            {
                Heading = heading,
                LeftPadding = leftPadding,
                RightPadding = rightPadding,
                AlignRight = true,
            }
        );

        return this;
    }

    public TextTableBuilder AddNumberColumn(
        string heading = "",
        string format = "N0",
        int minWidth = 0,
        string nullProjection = "--",
        int? leftPadding = null,
        int? rightPadding = null
    )
    {
        return AddColumn(
            heading,
            minWidth: minWidth,
            alignRight: true,
            format: format,
            nullProjection: nullProjection,
            leftPadding: leftPadding,
            rightPadding: rightPadding
        );
    }

    public TextTableBuilder AddStaticColumn(
        string value,
        bool showInHeading = false,
        bool showInRow = true,
        bool showInRowExtraLines = false
    )
    {
        _columns.Add(
            new StaticTextColumn
            {
                Text = value ?? string.Empty,
                ShowInHeading = showInHeading,
                ShowInRow = showInRow,
                ShowInRowExtraLines = showInRowExtraLines,
            }
        );

        return this;
    }

    public TextTableBuilder Border(TextTableBorder value)
    {
        _border = value;

        return this;
    }

    public ITextRowBuilder ClearRows()
    {
        return RowBuilder.ClearRows();
    }

    public static TextTableBuilder Create(TextTableBorder border = TextTableBorder.None)
    {
        return new TextTableBuilder(TextBuilder.Create()).Border(border);
    }

    public static TextTableBuilder Create(
        IEnumerable<IEnumerable<object>> values,
        TextTableBorder border = TextTableBorder.Inner,
        IEnumerable<string>? columnNames = null,
        int defaultColumnPadding = 1,
        int? defaultColumnMaxWidth = 50,
        Func<object, string>? formatCellValue = null,
        bool includeIdentityColumn = false
    )
    {
        formatCellValue ??= v => v?.Render() ?? string.Empty;

        TextTableBuilder builder = Create()
            .Border(border)
            .DefaultColumnPadding(defaultColumnPadding)
            .DefaultColumnMaxWidth(defaultColumnMaxWidth);

        IReadOnlyList<IEnumerable<object>> ivalues =
            values as IReadOnlyList<IEnumerable<object>> ?? values?.ToList() ?? [];
        IReadOnlyList<string> icolumnNames =
            columnNames as IReadOnlyList<string> ?? columnNames?.ToList() ?? [];

        if (!ivalues.Any() && icolumnNames.Any())
        {
            foreach (string columnName in icolumnNames)
            {
                builder.AddColumn(
                    columnName,
                    maxWidth: defaultColumnMaxWidth,
                    nullProjection: "--"
                );
            }

            builder.AddHeadingRow();

            return builder;
        }

        icolumnNames = icolumnNames?.ToList() ?? [];

        int maxColumnCount = ivalues.Any() ? ivalues.Max(row => row.Count()) : 0;
        if (includeIdentityColumn)
        {
            builder.AddIdentityColumn();
        }

        for (int index = 0; index < maxColumnCount; index++)
        {
            bool allNumeric = ivalues.All(row =>
                row.ElementAtOrDefault(index)?.IsNumeric() == true
            );
            string columnName = icolumnNames?.ElementAtOrDefault(index) ?? $"Column{index + 1}";

            builder.AddColumn(
                columnName,
                maxWidth: defaultColumnMaxWidth,
                alignRight: allNumeric,
                nullProjection: "--"
            );
        }

        builder.AddHeadingRow();

        foreach (IEnumerable<object> row in ivalues)
        {
            List<object?> rowValues = [.. row.Select(formatCellValue).ToArray()];

            for (int position = rowValues.Count; position < maxColumnCount; position++)
            {
                rowValues.Add(null);
            }

            builder.AddDataRow(rowValues);
        }

        return builder;
    }

    public TextTableBuilder DefaultColumnMaxWidth(int? value)
    {
        _defaultColumnMaxWidth = value;

        return this;
    }

    public TextTableBuilder DefaultColumnPadding(int value)
    {
        _defaultColumnPadding = value;

        return this;
    }

    public ITextRowBuilder InsertRow(int index, ITextRow row)
    {
        return RowBuilder.InsertRow(index, row);
    }

    public ITextRowBuilder RemoveRow(int index)
    {
        return RowBuilder.RemoveRow(index);
    }

    public string Render()
    {
        return new TextTableModel(
            Columns: _columns,
            Rows: _rows,
            Border: _border,
            DefaultColumnMaxWidth: _defaultColumnMaxWidth,
            DefaultColumnPadding: _defaultColumnPadding,
            RowLimit: _rowLimit
        ).Render();
    }

    public TextTableBuilder RowLimit(int? rowLimit)
    {
        _rowLimit = rowLimit;

        return this;
    }

    public TextBuilder SaveTable()
    {
        if (_saved)
        {
            throw new InvalidOperationException(
                $"Cannot {nameof(SaveTable)}, {nameof(TextSectionBuilder)} already saved."
            );
        }

        _saved = true;

        return _parent.Add(this);
    }

    public override string ToString()
    {
        return $"Table, {_columns.Count} Columns, {_rows.Count} Rows";
    }

    internal void ResetLayout()
    {
        //_layoutBorderComputed = false;
        _columns.RemoveAll(static c =>
            c is PaddingTextColumn or IBorderTextColumn { External: false }
        );
        _rows.RemoveAll(static c => c is IBorderTextRow { External: false });
    }
}

/*************************************************************
 * Rows Types
 *************************************************************/
[Flags]
public enum TextTableBorder
{
    None = 0,
    Column = 1 << 1,
    Row = 1 << 2,
    Outer = 1 << 3,
    Inner = 1 << 4,
    InnerFull = Row | Column,
    All = int.MaxValue,
}
