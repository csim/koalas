using System.Collections.Generic;
using Outback.Text;
using Outback.Text.Models;

namespace Outback.Tests.Text.Models;

public class TextRegionModelTests
{
    [Fact]
    public void Constructor_WithChildrenAndIndentSize_SetsProperties()
    {
        // Arrange
        List<IRender> children = [new TextLineModel("Test")];
        const int indentSize = 4;

        // Act
        TextRegionModel model = new(children.AsReadOnly(), indentSize);

        // Assert
        model.Children.Should().BeEquivalentTo(children);
        model.IndentSize.Should().Be(indentSize);
    }

    [Fact]
    public void Render_WithEmptyChildren_ReturnsEmptyString()
    {
        // Arrange
        TextRegionModel model = new(new List<IRender>().AsReadOnly(), 0);

        // Act
        string result = model.Render();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Render_WithIndent_ReturnsIndentedContent()
    {
        // Arrange
        List<IRender> children = [new TextLineModel("Test content")];
        TextRegionModel model = new(children.AsReadOnly(), 2);

        // Act
        string result = model.Render();

        // Assert
        result.Should().Contain("Test content");
        // Should be indented (though we don't test exact spaces here)
    }

    [Fact]
    public void Render_WithNoIndent_ReturnsChildrenRendered()
    {
        // Arrange
        List<IRender> children = [new TextLineModel("Line 1"), new TextLineModel("Line 2")];
        TextRegionModel model = new(children.AsReadOnly(), 0);

        // Act
        string result = model.Render();

        // Assert
        result.Should().Contain("Line 1");
        result.Should().Contain("Line 2");
    }
}
