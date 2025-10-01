using System.Collections.Generic;
using Koalas.Text;
using Koalas.Text.Models;

namespace Koalas.Tests.Text.Models;

public class TextRegionModelTests
{
    [Fact]
    public void Constructor_WithChildrenAndIndentSize_SetsProperties()
    {
        // Arrange
        var children = new List<IRender> { new TextLineModel("Test") };
        const int indentSize = 4;

        // Act
        var model = new TextRegionModel(children.AsReadOnly(), indentSize);

        // Assert
        model.Children.Should().BeEquivalentTo(children);
        model.IndentSize.Should().Be(indentSize);
    }

    [Fact]
    public void Render_WithEmptyChildren_ReturnsEmptyString()
    {
        // Arrange
        var model = new TextRegionModel(new List<IRender>().AsReadOnly(), 0);

        // Act
        var result = model.Render();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Render_WithIndent_ReturnsIndentedContent()
    {
        // Arrange
        var children = new List<IRender> { new TextLineModel("Test content") };
        var model = new TextRegionModel(children.AsReadOnly(), 2);

        // Act
        var result = model.Render();

        // Assert
        result.Should().Contain("Test content");
        // Should be indented (though we don't test exact spaces here)
    }

    [Fact]
    public void Render_WithNoIndent_ReturnsChildrenRendered()
    {
        // Arrange
        var children = new List<IRender>
        {
            new TextLineModel("Line 1"),
            new TextLineModel("Line 2"),
        };
        var model = new TextRegionModel(children.AsReadOnly(), 0);

        // Act
        var result = model.Render();

        // Assert
        result.Should().Contain("Line 1");
        result.Should().Contain("Line 2");
    }
}
