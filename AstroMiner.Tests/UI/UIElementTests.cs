using System.Collections.Generic;
using AstroMiner.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Tests.UI;

[TestClass]
public class UIElementTests
{
    private Dictionary<string, Texture2D> _textures;

    [TestInitialize]
    public void Setup()
    {
        // Mock textures dictionary - we don't need actual textures for layout tests
        _textures = new Dictionary<string, Texture2D>
        {
            { "white", null }  // The actual texture isn't needed for layout tests
        };
    }

    [TestMethod]
    public void UIElement_ComputesLayoutCorrectly()
    {
        // Arrange - Create a tree similar to UIState.GetTree()
        var root = new UIElement(_textures)
        {
            FixedWidth = 100,
            FixedHeight = 100,
            ChildrenAlign = ChildrenAlign.Center
        };

        var container = new UIElement(_textures)
        {
            BackgroundColor = Color.Green,
            ChildrenAlign = ChildrenAlign.Center
        };

        var child1 = new UIElement(_textures)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 50,
            FixedHeight = 30
        };

        var child2 = new UIElement(_textures)
        {
            BackgroundColor = Color.DarkBlue,
            FixedWidth = 40,
            FixedHeight = 20
        };

        // Build the tree
        container.Children.Add(child1);
        container.Children.Add(child2);
        root.Children.Add(container);

        // Act
        root.ComputeDimensions();
        root.ComputePositions(0, 0);

        // Assert
        // Root should be at 0,0 with fixed dimensions
        Assert.AreEqual(0, root.X);
        Assert.AreEqual(0, root.Y);
        Assert.AreEqual(100, root.ComputedWidth);
        Assert.AreEqual(100, root.ComputedHeight);

        // Container should be centered in root and sized to fit children
        Assert.AreEqual(50, child1.ComputedWidth);  // Fixed width
        Assert.AreEqual(30, child1.ComputedHeight); // Fixed height
        Assert.AreEqual(50, container.ChildrenWidth);  // Takes largest child width
        Assert.AreEqual(50, container.ComputedHeight); // Sum of children heights

        // Container should be centered horizontally in root
        Assert.AreEqual(25, container.X); // (100 - 50) / 2
        Assert.AreEqual(0, container.Y);  // Top aligned

        // Child1 should be centered in container
        Assert.AreEqual(25, child1.X);    // Container X + (Container Width - Child Width) / 2
        Assert.AreEqual(0, child1.Y);     // First child starts at top

        // Child2 should be centered in container and below child1
        Assert.AreEqual(30, child2.X);    // Container X + (Container Width - Child Width) / 2
        Assert.AreEqual(30, child2.Y);    // Starts after child1
    }
}