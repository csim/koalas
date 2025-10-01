namespace Koalas.Text.Models;

public record class TextTableModel(
    List<ITextColumn> Columns,
    List<ITextRow> Rows,
    TextTableBorder Border,
    int? DefaultColumnMaxWidth,
    int DefaultColumnPadding,
    int? RowLimit
) : IRender
{
    private readonly bool _columnBorders =
        Border.HasFlag(TextTableBorder.Column) || Border.HasFlag(TextTableBorder.Inner);

    private readonly bool _headingRowBorder = Border.HasFlag(TextTableBorder.Inner);
    private bool _layoutBorderComputed;
    private readonly bool _outerBorders = Border.HasFlag(TextTableBorder.Outer);
    private readonly bool _rowBorders = Border.HasFlag(TextTableBorder.Row);

    public string Render()
    {
        if (!Columns.Any() || !Rows.Any())
        {
            return string.Empty;
        }

        ComputeLayout();

        StringBuilder output = new();

        int rowLimit = RowLimit ?? int.MaxValue;
        int dataRowCount = 0;
        bool limitReached = false;

        foreach (ITextRow row in Rows)
        {
            if (RowLimit != null && row is DataTextRow && ++dataRowCount > RowLimit.Value)
            {
                limitReached = true;
                break;
            }

            row.Render(output, Columns);
        }

        string ret = output.ToString().TrimEnd('\r', '\n') + Environment.NewLine;

        if (!limitReached)
        {
            return ret;
        }

        int totalDataRowCount = Rows.Count(static r => r is DataTextRow);
        ret += $"[{totalDataRowCount - rowLimit:N0} more]{Environment.NewLine}";

        return ret;
    }

    private void ComputeLayout()
    {
        // Resolve inner borders
        if (!_layoutBorderComputed && _columnBorders)
        {
            ComputeMeta();
            IEnumerable<int> dataColumnIndexes =
                from col in Columns
                where
                    col is IDynamicWidthTextColumn
                    && !col.First
                    && Columns[col.Index - 1] is not IBorderTextColumn
                orderby col.Index descending
                select col.Index;
            foreach (int index in dataColumnIndexes.ToList())
            {
                Columns.Insert(index, new SingleBorderTextColumn());
            }
        }

        if (!_layoutBorderComputed && _rowBorders)
        {
            ComputeMeta();
            IEnumerable<int> dataRowIndexes =
                from row in Rows
                where row is DataTextRow && !row.First && Rows[row.Index - 1] is not IBorderTextRow
                orderby row.Index descending
                select row.Index;
            foreach (int index in dataRowIndexes.ToList())
            {
                Rows.Insert(index, new SingleBorderTextRow());
            }
        }

        if (!_layoutBorderComputed && _headingRowBorder)
        {
            ComputeMeta();
            IEnumerable<int> insertIndexes =
                from row in Rows
                where row is HeadingTextRow
                from index in row.First ? (row.Index + 1).Yield()
                : row.Last ? row.Index.Yield()
                : [row.Index, row.Index + 1]
                orderby index descending
                select index;
            foreach (int index in insertIndexes.ToList())
            {
                Rows.Insert(index, new SingleBorderTextRow());
            }
        }

        // Resolve padding
        ComputeMeta();
        if (!_layoutBorderComputed)
        {
            IReadOnlyList<IDynamicWidthTextColumn> dynamicColumns =
            [
                .. (
                    from col in Columns
                    where col is IDynamicWidthTextColumn
                    let dynamicCol = (IDynamicWidthTextColumn)col
                    orderby dynamicCol.Index descending
                    select dynamicCol
                ),
            ];
            foreach (
                IDynamicWidthTextColumn column in dynamicColumns
                    .Where(col => col.LeftPadding == null)
                    .ToList()
            )
            {
                column.LeftPadding = column.First && !_outerBorders ? 0 : DefaultColumnPadding;
            }

            foreach (
                IDynamicWidthTextColumn column in dynamicColumns
                    .Where(col => col.RightPadding == null)
                    .ToList()
            )
            {
                column.RightPadding = column.Last && !_outerBorders ? 0 : DefaultColumnPadding;
            }

            foreach (IDynamicWidthTextColumn dynamicColumn in dynamicColumns)
            {
                if (dynamicColumn.RightPadding is > 0)
                {
                    Columns.Insert(
                        dynamicColumn.Index + 1,
                        new PaddingTextColumn(dynamicColumn.RightPadding.Value)
                    );
                }

                // ReSharper disable once InvertIf
                if (dynamicColumn.LeftPadding > 0)
                {
                    Columns.Insert(
                        dynamicColumn.Index,
                        new PaddingTextColumn(dynamicColumn.LeftPadding.Value)
                    );
                }
            }
        }

        // Resolve widths
        ComputeMeta();
        foreach (ITextColumn col in Columns)
        {
            IEnumerable<string> x =
                from row in Rows
                from partition in col.Lines(row)
                select partition;

            if (x.Any(y => y == null))
            {
                Console.Write('x');
            }

            IReadOnlyList<int> partitionLengths =
            [
                .. from row in Rows from partition in col.Lines(row) select partition.Length,
            ];
            int dataWidth = partitionLengths.Any() ? partitionLengths.Max() : 0;
            bool hasHeading = Rows.Any(r => r is HeadingTextRow);
            if (col is IDynamicWidthTextColumn dynamicCol)
            {
                dynamicCol.MaximumWidth ??= DefaultColumnMaxWidth;
                int headingWidth = hasHeading ? dynamicCol.Heading?.Length ?? 0 : 0;
                dynamicCol.Width = Math.Max(
                    dynamicCol.MinimumWidth,
                    Math.Max(headingWidth, dataWidth)
                );
            }
            else
            {
                col.Width = dataWidth;
            }
        }

        // Resolve outer border
        ComputeMeta();
        if (!_layoutBorderComputed && _outerBorders)
        {
            if (Columns[0] is not IBorderTextColumn)
            {
                Columns.Insert(0, new SingleBorderTextColumn());
            }

            if (Columns[Columns.Count - 1] is not IBorderTextColumn)
            {
                Columns.Add(new SingleBorderTextColumn());
            }

            if (Rows[0] is not IBorderTextRow)
            {
                Rows.Insert(0, new SingleBorderTextRow());
            }

            if (Rows[Rows.Count - 1] is not IBorderTextRow)
            {
                Rows.Add(new SingleBorderTextRow());
            }
        }

        ComputeMeta();

        _layoutBorderComputed = true;
    }

    private void ComputeMeta()
    {
        int index = 0;
        foreach (ITextColumn col in Columns)
        {
            col.First = index == 0;
            col.Last = index == Columns.Count - 1;
            col.Index = index++;
        }

        index = 0;
        foreach (ITextRow row in Rows)
        {
            row.First = index == 0;
            row.Last = index == Rows.Count - 1;
            row.Index = index++;
        }
    }
}
