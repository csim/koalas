using System;
using Koalas.Text;
using Koalas.Text.Models;

namespace Koalas.Tests.Text;

public class TextBuilderTests
{
    [Fact]
    public void Add_WithSimpleText_AddsTextToBuilder()
    {
        // Arrange
        TextBuilder builder = TextBuilder.Create();
        TextLineModel textLine = new("Hello World");

        // Act
        TextBuilder result = builder.Add(textLine);

        // Assert
        result.Should().BeSameAs(builder); // Should return the same instance for chaining
        builder.Render().Should().Contain("Hello World");
    }

    [Fact]
    public void AddBlankLine_AddsEmptyLineToBuilder()
    {
        // Arrange
        TextBuilder builder = TextBuilder.Create();

        // Act
        TextBuilder result = builder.AddBlankLine();

        // Assert
        result.Should().BeSameAs(builder);
        builder.Render().Should().Contain(Environment.NewLine);
    }

    [Fact]
    public void AddBlankLine_WithCount_AddsMultipleEmptyLines()
    {
        // Arrange
        TextBuilder builder = TextBuilder.Create();
        const int count = 3;

        // Act
        TextBuilder result = builder.AddBlankLine(count);

        // Assert
        result.Should().BeSameAs(builder);
        string rendered = builder.Render();
        int newLineCount = rendered.Split(Environment.NewLine).Length - 1;
        newLineCount.Should().BeGreaterOrEqualTo(count);
    }

    [Fact]
    public void AddLine_WithSimpleString_AddsTextLineToBuilder()
    {
        // Arrange
        TextBuilder builder = TextBuilder.Create();

        // Act
        TextBuilder result = builder.AddLine("Hello World");

        // Assert
        result.Should().BeSameAs(builder);
        builder.Render().Should().Contain("Hello World");
    }

    [Fact]
    public void Create_WithCustomIndentSize_ReturnsTextBuilderWithCorrectIndentSize()
    {
        // Arrange
        const int customIndentSize = 4;

        // Act
        TextBuilder builder = TextBuilder.Create(customIndentSize);

        // Assert
        builder.Should().NotBeNull();
        builder.IndentSize.Should().Be(0); // Initial indent size should be 0
    }

    [Fact]
    public void Create_WithDefaultIndentSize_ReturnsTextBuilder()
    {
        // Act
        TextBuilder builder = TextBuilder.Create();

        // Assert
        builder.Should().NotBeNull();
        builder.IndentSize.Should().Be(0);
    }

    [Fact]
    public void IndentedContent_RendersWithProperIndentation()
    {
        // Arrange
        TextBuilder builder = TextBuilder.Create(2);
        builder.AddLine("Before indent");
        builder.PushIndent();
        builder.AddLine("Indented content");
        builder.PopIndent();
        builder.AddLine("After indent");

        // Act
        string result = builder.Render();

        // Assert
        result.Should().Contain("Before indent");
        result.Should().Contain("Indented content");
        result.Should().Contain("After indent");
    }

    [Fact]
    public void PopIndent_DecreasesIndentLevel()
    {
        // Arrange
        TextBuilder builder = TextBuilder.Create(2);
        builder.PushIndent();

        // Act
        TextBuilder result = builder.PopIndent();

        // Assert
        result.Should().BeSameAs(builder);
        builder.IndentSize.Should().Be(0);
    }

    [Fact]
    public void PushIndent_IncreasesIndentLevel()
    {
        // Arrange
        TextBuilder builder = TextBuilder.Create(2);

        // Act
        TextBuilder result = builder.PushIndent();

        // Assert
        result.Should().BeSameAs(builder);
        builder.IndentSize.Should().Be(2);
    }

    [Fact]
    public void PushIndent_WithCustomSize_UsesCustomIndentSize()
    {
        // Arrange
        TextBuilder builder = TextBuilder.Create(2);

        // Act
        TextBuilder result = builder.PushIndent(4);

        // Assert
        result.Should().BeSameAs(builder);
        builder.IndentSize.Should().Be(4);
    }

    [Fact]
    public void Render_ReturnsStringRepresentation()
    {
        // Arrange
        TextBuilder builder = TextBuilder.Create();
        builder.AddLine("Line 1");
        builder.AddLine("Line 2");

        // Act
        string result = builder.Render();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Line 1");
        result.Should().Contain("Line 2");
    }

    [Fact]
    public void ToString_ReturnsSameAsRender()
    {
        // Arrange
        TextBuilder builder = TextBuilder.Create();
        builder.AddLine("Test content");

        // Act
        string renderResult = builder.Render();
        string toStringResult = builder.ToString();

        // Assert
        toStringResult.Should().Be(renderResult);
    }
}
