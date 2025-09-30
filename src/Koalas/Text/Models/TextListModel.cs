namespace Koalas.Text.Models;

public record class TextListModel(IReadOnlyList<TextListItemModel> Items) : IRender
{
    public string Render()
    {
        TextTableBuilder table = TextTableBuilder.Create()
                                                 .AddColumn(leftPadding: 0, rightPadding: 0, alignRight: true) // Indicator
                                                 .AddColumn(leftPadding: 0, rightPadding: 0, alignRight: true) // Id
                                                 .AddColumn(leftPadding: 0, rightPadding: 0)                   // Separator
                                                 .AddColumn();                                                 // Body

        int currentDefaultId = 0;

        foreach (TextListItemModel item in Items)
        {
            currentDefaultId++;

            string body = item.Body
                              .Render();

            table.AddDataRow(item.Indicator ?? string.Empty,
                             item.Id ?? currentDefaultId.ToString(),
                             item.Separator,
                             body);
        }

        return table.Render();
    }
}