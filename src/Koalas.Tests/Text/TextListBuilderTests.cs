using System;
using Koalas.Text;
using Koalas.Text.Models;

namespace Koalas.Tests.Text;

public class TextListBuilderTests
{
    [Fact]
    public void StartList_CreatesTextListBuilder()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        var listBuilder = textBuilder.StartList();

        // Assert
        listBuilder.Should().NotBeNull();
        listBuilder.Should().BeOfType<TextListBuilder>();
    }

    [Fact]
    public void StartList_WithCustomSeparator_SetsSeparatorCorrectly()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var separator = "-";

        // Act
        var listBuilder = textBuilder.StartList(separator);

        // Assert
        listBuilder.Should().NotBeNull();
    }

    [Fact]
    public void AddItem_WithLabel_AddsItemToList()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var listBuilder = textBuilder.StartList();

        // Act
        var result = listBuilder.AddItem("Item 1");

        // Assert
        result.Should().BeSameAs(listBuilder);
    }

    [Fact]
    public void AddItem_WithLabelAndValue_AddsItemWithValueToList()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var listBuilder = textBuilder.StartList();

        // Act
        var result = listBuilder.AddItem("Name", "John");

        // Assert
        result.Should().BeSameAs(listBuilder);
    }

    [Fact]
    public void SaveList_ReturnsOriginalTextBuilder()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var listBuilder = textBuilder.StartList();
        listBuilder.AddItem("Item 1");

        // Act
        var result = listBuilder.SaveList();

        // Assert
        result.Should().BeSameAs(textBuilder);
    }

    [Fact]
    public void CompleteList_RendersCorrectly()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartList()
            .AddItem("First Item")
            .AddItem("Second Item")
            .AddItem("Third Item")
            .SaveList();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("First Item");
        result.Should().Contain("Second Item");
        result.Should().Contain("Third Item");
    }

    [Fact]
    public void ListWithCustomSeparator_RendersWithCorrectSeparator()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var separator = "->";

        // Act
        textBuilder.StartList(separator)
            .AddItem("Key", "Value")
            .SaveList();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Key");
        result.Should().Contain("Value");
    }

    [Fact]
    public void EmptyList_RendersEmptyOrMinimal()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartList()
            .SaveList();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ListWithBlankLines_HandlesTrailingBlankLines()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartList(defaultTrailingBlankLines: 2)
            .AddItem("Item 1")
            .AddItem("Item 2")
            .SaveList();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Item 1");
        result.Should().Contain("Item 2");
    }

    [Fact]
    public void AddItem_WithTrailingBlankLines_AddsCorrectSpacing()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var listBuilder = textBuilder.StartList();

        // Act
        var result = listBuilder.AddItem("Item with spacing", trailingBlankLines: 1);

        // Assert
        result.Should().BeSameAs(listBuilder);
    }
}