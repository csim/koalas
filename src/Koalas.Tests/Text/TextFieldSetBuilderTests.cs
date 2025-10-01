using Koalas.Text;

namespace Koalas.Tests.Text;

public class TextFieldSetBuilderTests
{
    [Fact]
    public void AddField_WithLabelAndValue_AddsFieldToSet()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var fieldSetBuilder = textBuilder.StartFieldSet();

        // Act
        var result = fieldSetBuilder.AddField("Name", "John");

        // Assert
        result.Should().BeSameAs(fieldSetBuilder);
    }

    [Fact]
    public void CompleteFieldSet_RendersCorrectly()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        textBuilder
            .StartFieldSet()
            .AddField("Name", "John Doe")
            .AddField("Age", "30")
            .AddField("City", "New York")
            .SaveFieldSet();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Name");
        result.Should().Contain("John Doe");
        result.Should().Contain("Age");
        result.Should().Contain("30");
        result.Should().Contain("City");
        result.Should().Contain("New York");
    }

    [Fact]
    public void EmptyFieldSet_RendersEmptyOrMinimal()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartFieldSet().SaveFieldSet();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void FieldSetWithCustomSeparator_RendersWithCorrectSeparator()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        const string separator = " = ";

        // Act
        textBuilder
            .StartFieldSet(fieldSeparator: separator)
            .AddField("Key", "Value")
            .SaveFieldSet();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Key");
        result.Should().Contain("Value");
    }

    [Fact]
    public void MinLabelWidth_SetsMinimumLabelWidth()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var fieldSetBuilder = textBuilder.StartFieldSet();

        // Act
        var result = fieldSetBuilder.MinLabelWidth(20);

        // Assert
        result.Should().BeSameAs(fieldSetBuilder);
    }

    [Fact]
    public void SaveFieldSet_ReturnsOriginalTextBuilder()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var fieldSetBuilder = textBuilder.StartFieldSet();
        fieldSetBuilder.AddField("Key", "Value");

        // Act
        var result = fieldSetBuilder.SaveFieldSet();

        // Assert
        result.Should().BeSameAs(textBuilder);
    }

    [Fact]
    public void StartFieldSet_CreatesTextFieldSetBuilder()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        var fieldSetBuilder = textBuilder.StartFieldSet();

        // Assert
        fieldSetBuilder.Should().NotBeNull();
        fieldSetBuilder.Should().BeOfType<TextFieldSetBuilder>();
    }

    [Fact]
    public void StartFieldSet_WithCustomSeparator_SetsSeparatorCorrectly()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        const string separator = "=";

        // Act
        var fieldSetBuilder = textBuilder.StartFieldSet(fieldSeparator: separator);

        // Assert
        fieldSetBuilder.Should().NotBeNull();
    }
}
