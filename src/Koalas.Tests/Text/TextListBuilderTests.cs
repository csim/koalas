using Koalas.Text;

namespace Koalas.Tests.Text;

public class TextListBuilderTests
{
    [Fact]
    public void AddItem_WithLabel_AddsItemToList()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextListBuilder listBuilder = textBuilder.StartList();

        // Act
        TextListBuilder result = listBuilder.AddItem("Item 1");

        // Assert
        result.Should().BeSameAs(listBuilder);
    }

    [Fact]
    public void AddItem_WithLabelAndValue_AddsItemWithValueToList()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextListBuilder listBuilder = textBuilder.StartList();

        // Act
        TextListBuilder result = listBuilder.AddItem("Name", "John");

        // Assert
        result.Should().BeSameAs(listBuilder);
    }

    [Fact]
    public void AddItem_WithTrailingBlankLines_AddsCorrectSpacing()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextListBuilder listBuilder = textBuilder.StartList();

        // Act
        TextListBuilder result = listBuilder.AddItem("Item with spacing", trailingBlankLines: 1);

        // Assert
        result.Should().BeSameAs(listBuilder);
    }

    [Fact]
    public void CompleteList_RendersCorrectly()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        textBuilder
            .StartList()
            .AddItem("First Item")
            .AddItem("Second Item")
            .AddItem("Third Item")
            .SaveList();

        string result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("First Item");
        result.Should().Contain("Second Item");
        result.Should().Contain("Third Item");
    }

    [Fact]
    public void EmptyList_RendersEmptyOrMinimal()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartList().SaveList();

        string result = textBuilder.Render();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ListWithBlankLines_HandlesTrailingBlankLines()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        textBuilder
            .StartList(defaultTrailingBlankLines: 2)
            .AddItem("Item 1")
            .AddItem("Item 2")
            .SaveList();

        string result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Item 1");
        result.Should().Contain("Item 2");
    }

    [Fact]
    public void ListWithCustomSeparator_RendersWithCorrectSeparator()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        string separator = "->";

        // Act
        textBuilder.StartList(separator).AddItem("Key", "Value").SaveList();

        string result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Key");
        result.Should().Contain("Value");
    }

    [Fact]
    public void SaveList_ReturnsOriginalTextBuilder()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextListBuilder listBuilder = textBuilder.StartList();
        listBuilder.AddItem("Item 1");

        // Act
        TextBuilder result = listBuilder.SaveList();

        // Assert
        result.Should().BeSameAs(textBuilder);
    }

    [Fact]
    public void StartList_CreatesTextListBuilder()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        TextListBuilder listBuilder = textBuilder.StartList();

        // Assert
        listBuilder.Should().NotBeNull();
        listBuilder.Should().BeOfType<TextListBuilder>();
    }

    [Fact]
    public void StartList_WithCustomSeparator_SetsSeparatorCorrectly()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        string separator = "-";

        // Act
        TextListBuilder listBuilder = textBuilder.StartList(separator);

        // Assert
        listBuilder.Should().NotBeNull();
    }
}
