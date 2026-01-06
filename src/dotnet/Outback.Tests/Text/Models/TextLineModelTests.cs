using System;
using Outback.Text.Models;

namespace Outback.Tests.Text.Models;

public class TextLineModelTests
{
    [Fact]
    public void Constructor_WithText_SetsTextProperty()
    {
        // Arrange
        const string text = "Hello World";

        // Act
        TextLineModel model = new(text);

        // Assert
        model.Text.Should().Be(text);
    }

    [Fact]
    public void Render_ReturnsTextWithNewLine()
    {
        // Arrange
        const string text = "Hello World";
        TextLineModel model = new(text);

        // Act
        string result = model.Render();

        // Assert
        result.Should().Be(text + Environment.NewLine);
    }

    [Fact]
    public void Render_WithEmptyText_ReturnsNewLineOnly()
    {
        // Arrange
        TextLineModel model = new(string.Empty);

        // Act
        string result = model.Render();

        // Assert
        result.Should().Be(Environment.NewLine);
    }

    [Fact]
    public void Render_WithNullText_HandlesGracefully()
    {
        // Arrange
        TextLineModel model = new(null);

        // Act
        string result = model.Render();

        // Assert
        result.Should().NotBeNull();
    }
}
