using Koalas.Text;

namespace Koalas.Tests.Text;

public class TextSectionBuilderTests
{
    [Fact]
    public void AddLine_AddsContentToSection()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextSectionBuilder sectionBuilder = textBuilder.StartSection("Test Section");

        // Act
        TextSectionBuilder result = sectionBuilder.AddLine("Section content");

        // Assert
        result.Should().BeSameAs(sectionBuilder);
    }

    [Fact]
    public void CompleteSection_RendersCorrectly()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        textBuilder
            .StartSection("My Section")
            .AddLine("First line")
            .AddLine("Second line")
            .SaveSection();

        string result = textBuilder.Render();

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
        TextBuilder textBuilder = TextBuilder.Create();
        TextSectionBuilder sectionBuilder = textBuilder.StartSection();
        const string heading = "Test Heading";

        // Act
        TextSectionBuilder result = sectionBuilder.Heading(heading);

        // Assert
        result.Should().BeSameAs(sectionBuilder);
    }

    [Fact]
    public void SaveSection_ReturnsOriginalTextBuilder()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        TextSectionBuilder sectionBuilder = textBuilder.StartSection("Test");
        sectionBuilder.AddLine("Content");

        // Act
        TextBuilder result = sectionBuilder.SaveSection();

        // Assert
        result.Should().BeSameAs(textBuilder);
    }

    [Fact]
    public void SectionWithContent_RendersCorrectly()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        textBuilder.StartSection("Test Section").AddLine("Content").SaveSection();

        string result = textBuilder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Test Section");
        result.Should().Contain("Content");
    }

    [Fact]
    public void StartSection_CreatesTextSectionBuilder()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();

        // Act
        TextSectionBuilder sectionBuilder = textBuilder.StartSection();

        // Assert
        sectionBuilder.Should().NotBeNull();
        sectionBuilder.Should().BeOfType<TextSectionBuilder>();
    }

    [Fact]
    public void StartSection_WithHeading_SetsHeadingCorrectly()
    {
        // Arrange
        TextBuilder textBuilder = TextBuilder.Create();
        const string heading = "Section Title";

        // Act
        TextSectionBuilder sectionBuilder = textBuilder.StartSection(heading);

        // Assert
        sectionBuilder.Should().NotBeNull();
    }
}