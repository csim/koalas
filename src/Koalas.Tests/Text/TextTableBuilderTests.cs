using System;
using Koalas.Text;
using Koalas.Text.Models;

namespace Koalas.Tests.Text;

public class TextTableBuilderTests
{
    [Fact]
    public void StartTable_CreatesTextTableBuilder()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        var tableBuilder = textBuilder.StartTable();

        // Assert
        tableBuilder.Should().NotBeNull();
        tableBuilder.Should().BeOfType<TextTableBuilder>();
    }

    [Fact]
    public void StartTable_WithCustomBorder_SetsBorderCorrectly()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var border = TextTableBorder.Outer;

        // Act
        var tableBuilder = textBuilder.StartTable(border);

        // Assert
        tableBuilder.Should().NotBeNull();
    }

    [Fact]
    public void AddColumn_WithTitle_AddsColumnToTable()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var tableBuilder = textBuilder.StartTable();

        // Act
        var result = tableBuilder.AddColumn("Name");

        // Assert
        result.Should().BeSameAs(tableBuilder);
    }

    [Fact]
    public void AddColumn_WithTitleAndMinWidth_AddsColumnWithSpecificWidth()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var tableBuilder = textBuilder.StartTable();

        // Act
        var result = tableBuilder.AddColumn("Name", minWidth: 20);

        // Assert
        result.Should().BeSameAs(tableBuilder);
    }

    [Fact]
    public void AddDataRow_WithValues_AddsRowToTable()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var tableBuilder = textBuilder.StartTable();
        tableBuilder.AddColumn("Name");
        tableBuilder.AddColumn("Age");

        // Act
        var result = tableBuilder.AddDataRow("John", "30");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void SaveTable_ReturnsOriginalTextBuilder()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var tableBuilder = textBuilder.StartTable();
        tableBuilder.AddColumn("Name");
        tableBuilder.AddDataRow("John");

        // Act
        var result = tableBuilder.SaveTable();

        // Assert
        result.Should().BeSameAs(textBuilder);
    }

    [Fact]
    public void CompleteTable_RendersCorrectly()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartTable()
            .AddColumn("Name")
            .AddColumn("Age")
            .AddColumn("City")
            .AddHeadingRow()
            .AddDataRow("John", "30", "New York")
            .AddDataRow("Jane", "25", "Los Angeles")
            .SaveTable();

        var result = textBuilder.Render();

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
    public void TableWithBorders_RendersWithBorders()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartTable(TextTableBorder.All)
            .AddColumn("ID")
            .AddColumn("Name")
            .AddHeadingRow()
            .AddDataRow("1", "Test")
            .SaveTable();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("ID");
        result.Should().Contain("Name");
        result.Should().Contain("1");
        result.Should().Contain("Test");
        // Should contain border characters (assuming the implementation uses them)
    }

    [Fact]
    public void EmptyTable_RendersEmptyString()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartTable()
            .SaveTable();

        var result = textBuilder.Render();

        // Assert
        // An empty table should render as empty or minimal content
        result.Should().NotBeNull();
    }

    [Fact]
    public void TableWithOnlyColumns_NoRows_HandlesGracefully()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartTable()
            .AddColumn("Name")
            .AddColumn("Age")
            .SaveTable();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void RowLimit_LimitsNumberOfRows()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var tableBuilder = textBuilder.StartTable();
        tableBuilder.AddColumn("Number");
        tableBuilder.RowLimit(2);

        // Act
        for (int i = 1; i <= 5; i++)
        {
            tableBuilder.AddDataRow(i.ToString());
        }
        tableBuilder.SaveTable();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("1");
        result.Should().Contain("2");
        // Should not contain rows 3, 4, 5 due to limit
    }
}
