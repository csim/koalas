using System;
using Koalas.Text.Models;

namespace Koalas.Tests.Text.Models;

public class TextLineModelTests
{
    [Fact]
    public void Constructor_WithText_SetsTextProperty()
    {
        // Arrange
        const string text = "Hello World";

        // Act
        var model = new TextLineModel(text);

        // Assert
        model.Text.Should().Be(text);
    }

    [Fact]
    public void Render_ReturnsTextWithNewLine()
    {
        // Arrange
        const string text = "Hello World";
        var model = new TextLineModel(text);

        // Act
        var result = model.Render();

        // Assert
        result.Should().Be(text + Environment.NewLine);
    }

    [Fact]
    public void Render_WithEmptyText_ReturnsNewLineOnly()
    {
        // Arrange
        var model = new TextLineModel(string.Empty);

        // Act
        var result = model.Render();

        // Assert
        result.Should().Be(Environment.NewLine);
    }

    [Fact]
    public void Render_WithNullText_HandlesGracefully()
    {
        // Arrange
        var model = new TextLineModel(null);

        // Act
        var result = model.Render();

        // Assert
        result.Should().NotBeNull();
    }
}