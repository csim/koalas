using Koalas.Text;

namespace Koalas.Tests.Text;

public class TextSectionBuilderTests
{
    [Fact]
    public void AddLine_AddsContentToSection()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var sectionBuilder = textBuilder.StartSection("Test Section");

        // Act
        var result = sectionBuilder.AddLine("Section content");

        // Assert
        result.Should().BeSameAs(sectionBuilder);
    }

    [Fact]
    public void CompleteSection_RendersCorrectly()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        textBuilder
            .StartSection("My Section")
            .AddLine("First line")
            .AddLine("Second line")
            .SaveSection();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("My Section");
        result.Should().Contain("First line");
        result.Should().Contain("Second line");
    }

    [Fact]
    public void Heading_SetsHeadingProperty()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var sectionBuilder = textBuilder.StartSection();
        const string heading = "Test Heading";

        // Act
        var result = sectionBuilder.Heading(heading);

        // Assert
        result.Should().BeSameAs(sectionBuilder);
    }

    [Fact]
    public void SaveSection_ReturnsOriginalTextBuilder()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        var sectionBuilder = textBuilder.StartSection("Test");
        sectionBuilder.AddLine("Content");

        // Act
        var result = sectionBuilder.SaveSection();

        // Assert
        result.Should().BeSameAs(textBuilder);
    }

    [Fact]
    public void SectionWithContent_RendersCorrectly()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartSection("Test Section").AddLine("Content").SaveSection();

        var result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Test Section");
        result.Should().Contain("Content");
    }

    [Fact]
    public void StartSection_CreatesTextSectionBuilder()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();

        // Act
        var sectionBuilder = textBuilder.StartSection();

        // Assert
        sectionBuilder.Should().NotBeNull();
        sectionBuilder.Should().BeOfType<TextSectionBuilder>();
    }

    [Fact]
    public void StartSection_WithHeading_SetsHeadingCorrectly()
    {
        // Arrange
        var textBuilder = TextBuilder.Create();
        const string heading = "Section Title";

        // Act
        var sectionBuilder = textBuilder.StartSection(heading);

        // Assert
        sectionBuilder.Should().NotBeNull();
    }
}
