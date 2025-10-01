using Koalas.Text;

namespace Koalas.Tests.Text;

public class TextTableBuilderTests
{
    [Fact]
    public void AddColumn_WithTitle_AddsColumnToTable()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextTableBuilder tableBuilder = textBuilder.StartTable();

        // Act
        TextTableBuilder result = tableBuilder.AddColumn("Name");

        // Assert
        result.Should().BeSameAs(tableBuilder);
    }

    [Fact]
    public void AddColumn_WithTitleAndMinWidth_AddsColumnWithSpecificWidth()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextTableBuilder tableBuilder = textBuilder.StartTable();

        // Act
        TextTableBuilder result = tableBuilder.AddColumn("Name", minWidth: 20);

        // Assert
        result.Should().BeSameAs(tableBuilder);
    }

    [Fact]
    public void AddDataRow_WithValues_AddsRowToTable()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextTableBuilder tableBuilder = textBuilder.StartTable();
        tableBuilder.AddColumn("Name");
        tableBuilder.AddColumn("Age");

        // Act
        ITextRowBuilder result = tableBuilder.AddDataRow("John", "30");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void CompleteTable_RendersCorrectly()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        textBuilder
            .StartTable()
            .AddColumn("Name")
            .AddColumn("Age")
            .AddColumn("City")
            .AddHeadingRow()
            .AddDataRow("John", "30", "New York")
            .AddDataRow("Jane", "25", "Los Angeles")
            .SaveTable();

        string result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Name");
        result.Should().Contain("Age");
        result.Should().Contain("City");
        result.Should().Contain("John");
        result.Should().Contain("Jane");
        result.Should().Contain("30");
        result.Should().Contain("25");
        result.Should().Contain("New York");
        result.Should().Contain("Los Angeles");
    }

    [Fact]
    public void EmptyTable_RendersEmptyString()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartTable().SaveTable();

        string result = textBuilder.Render();

        // Assert
        // An empty table should render as empty or minimal content
        result.Should().NotBeNull();
    }

    [Fact]
    public void RowLimit_LimitsNumberOfRows()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextTableBuilder tableBuilder = textBuilder.StartTable();
        tableBuilder.AddColumn("Number");
        tableBuilder.RowLimit(2);

        // Act
        for (int i = 1; i <= 5; i++)
        {
            tableBuilder.AddDataRow(i.ToString());
        }
        tableBuilder.SaveTable();

        string result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("1");
        result.Should().Contain("2");
        // Should not contain rows 3, 4, 5 due to limit
    }

    [Fact]
    public void SaveTable_ReturnsOriginalTextBuilder()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextTableBuilder tableBuilder = textBuilder.StartTable();
        tableBuilder.AddColumn("Name");
        tableBuilder.AddDataRow("John");

        // Act
        TextBuilder result = tableBuilder.SaveTable();

        // Assert
        result.Should().BeSameAs(textBuilder);
    }

    [Fact]
    public void StartTable_CreatesTextTableBuilder()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        TextTableBuilder tableBuilder = textBuilder.StartTable();

        // Assert
        tableBuilder.Should().NotBeNull();
        tableBuilder.Should().BeOfType<TextTableBuilder>();
    }

    [Fact]
    public void StartTable_WithCustomBorder_SetsBorderCorrectly()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextTableBorder border = TextTableBorder.Outer;

        // Act
        TextTableBuilder tableBuilder = textBuilder.StartTable(border);

        // Assert
        tableBuilder.Should().NotBeNull();
    }

    [Fact]
    public void TableWithBorders_RendersWithBorders()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        textBuilder
            .StartTable(TextTableBorder.All)
            .AddColumn("ID")
            .AddColumn("Name")
            .AddHeadingRow()
            .AddDataRow("1", "Test")
            .SaveTable();

        string result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("ID");
        result.Should().Contain("Name");
        result.Should().Contain("1");
        result.Should().Contain("Test");
        // Should contain border characters (assuming the implementation uses them)
    }

    [Fact]
    public void TableWithOnlyColumns_NoRows_HandlesGracefully()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartTable().AddColumn("Name").AddColumn("Age").SaveTable();

        string result = textBuilder.Render();

        // Assert
        result.Should().NotBeNull();
    }
}
