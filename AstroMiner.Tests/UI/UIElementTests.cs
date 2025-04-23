using System.Collections.Generic;
using AstroMiner.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Tests.UI;

// Mock implementation of BaseGame for testing
public class MockBaseGame(Dictionary<string, Texture2D> textures) : BaseGame
{
    public Dictionary<string, Texture2D> Textures { get; } = textures;

    // required abstract method
    protected override void InitializeControls()
    {
    }
}

[TestClass]
public class UIElementTests
{
    private MockBaseGame _mockGame;
    private Dictionary<string, Texture2D> _textures;

    [TestInitialize]
    public void Setup()
    {
        // Mock textures dictionary - we don't need actual textures for layout tests
        _textures = new Dictionary<string, Texture2D>
        {
            { "white", null } // The actual texture isn't needed for layout tests
        };

        // Create mock game
        _mockGame = new MockBaseGame(_textures);
    }

    [TestMethod]
    public void UIElement_ComputesLayoutCorrectly()
    {
        // Arrange - Create a tree similar to UI.GetTree()
        var root = new UIElement(_mockGame)
        {
            FixedWidth = 100,
            FixedHeight = 100,
            ChildrenAlign = ChildrenAlign.Center
        };

        var container = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Green,
            ChildrenAlign = ChildrenAlign.Center
        };

        var child1 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 50,
            FixedHeight = 30
        };

        var child2 = new UIElement(_mockGame)
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
        root.ComputeDimensions(800, 600); // Pass in some parent dimensions (e.g., screen size)
        root.ComputePositions(0, 0);

        // Assert
        // Root should be at 0,0 with fixed dimensions
        Assert.AreEqual(0, root.X);
        Assert.AreEqual(0, root.Y);
        Assert.AreEqual(100, root.ComputedWidth);
        Assert.AreEqual(100, root.ComputedHeight);

        // Container should be centered in root and sized to fit children
        Assert.AreEqual(50, child1.ComputedWidth); // Fixed width
        Assert.AreEqual(30, child1.ComputedHeight); // Fixed height
        Assert.AreEqual(50, container.ChildrenWidth); // Takes largest child width
        Assert.AreEqual(50, container.ComputedHeight); // Sum of children heights

        // Container should be centered horizontally in root
        Assert.AreEqual(25, container.X); // (100 - 50) / 2
        Assert.AreEqual(0, container.Y); // Top aligned

        // Child1 should be centered in container
        Assert.AreEqual(25, child1.X); // Container X + (Container Width - Child Width) / 2
        Assert.AreEqual(0, child1.Y); // First child starts at top

        // Child2 should be centered in container and below child1
        Assert.AreEqual(30, child2.X); // Container X + (Container Width - Child Width) / 2
        Assert.AreEqual(30, child2.Y); // Starts after child1
    }

    [TestMethod]
    public void UIElement_ComputesRowLayoutCorrectly()
    {
        // Arrange - Create a tree with row direction
        var root = new UIElement(_mockGame)
        {
            FixedWidth = 200,
            FixedHeight = 100
        };

        var container = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Green,
            ChildrenAlign = ChildrenAlign.Center,
            ChildrenDirection = ChildrenDirection.Row
        };

        var child1 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 50,
            FixedHeight = 30
        };

        var child2 = new UIElement(_mockGame)
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
        root.ComputeDimensions(800, 600); // Pass in some parent dimensions (e.g., screen size)
        root.ComputePositions(0, 0);

        // Assert
        // Root should be at 0,0 with fixed dimensions
        Assert.AreEqual(0, root.X);
        Assert.AreEqual(0, root.Y);
        Assert.AreEqual(200, root.ComputedWidth);
        Assert.AreEqual(100, root.ComputedHeight);

        // Container should be sized to fit children
        Assert.AreEqual(90, container.ChildrenWidth); // Sum of children widths (50 + 40)
        Assert.AreEqual(30, container.ChildrenHeight); // Takes largest child height
        Assert.AreEqual(90, container.ComputedWidth); // No fixed width
        Assert.AreEqual(30, container.ComputedHeight); // No fixed height

        // Container position
        Assert.AreEqual(0, container.X);
        Assert.AreEqual(0, container.Y);

        // Child1 should be at the start of the row
        Assert.AreEqual(0, child1.X);
        Assert.AreEqual(0, child1.Y);

        // Child2 should be next to child1 and vertically centered according to container's alignment
        Assert.AreEqual(50, child2.X); // child1.X + child1.ComputedWidth
        Assert.AreEqual(5,
            child2.Y); // Center aligned: container.Y + (container.ComputedHeight - child2.ComputedHeight) / 2
    }

    [TestMethod]
    public void UIElement_ChildrenJustify_Works_Correctly()
    {
        // Create test elements
        var rowParent = new UIElement(_mockGame)
        {
            FixedWidth = 200,
            FixedHeight = 50,
            ChildrenDirection = ChildrenDirection.Row,
            ChildrenJustify = ChildrenJustify.SpaceBetween
        };

        var columnParent = new UIElement(_mockGame)
        {
            FixedWidth = 50,
            FixedHeight = 200,
            ChildrenDirection = ChildrenDirection.Column,
            ChildrenJustify = ChildrenJustify.SpaceBetween
        };

        var child1 = new UIElement(_mockGame)
        {
            FixedWidth = 30,
            FixedHeight = 30
        };

        var child2 = new UIElement(_mockGame)
        {
            FixedWidth = 30,
            FixedHeight = 30
        };

        var child3 = new UIElement(_mockGame)
        {
            FixedWidth = 30,
            FixedHeight = 30
        };

        // Set up the row parent with children
        rowParent.Children.Add(child1);
        rowParent.Children.Add(child2);
        rowParent.Children.Add(child3);

        // Test the row layout with SpaceBetween
        rowParent.ComputeDimensions(800, 600); // Pass in some parent dimensions
        rowParent.ComputePositions(0, 0);

        // Calculate expected spacing
        var totalRowWidth = child1.ComputedWidth + child2.ComputedWidth + child3.ComputedWidth; // 90
        var rowSpacing =
            (rowParent.ComputedWidth - totalRowWidth) / (rowParent.Children.Count - 1); // (200 - 90) / 2 = 55

        // Check positions in row layout
        Assert.AreEqual(0, child1.X); // First child at start
        Assert.AreEqual(child1.ComputedWidth + rowSpacing, child2.X); // Second child after spacing (30 + 55 = 85)
        Assert.AreEqual(child1.ComputedWidth + child2.ComputedWidth + 2 * rowSpacing,
            child3.X); // Third child (30 + 30 + 2*55 = 170)

        // Set up the column parent with children
        var child4 = new UIElement(_mockGame) { FixedWidth = 30, FixedHeight = 30 };
        var child5 = new UIElement(_mockGame) { FixedWidth = 30, FixedHeight = 30 };
        var child6 = new UIElement(_mockGame) { FixedWidth = 30, FixedHeight = 30 };

        columnParent.Children.Add(child4);
        columnParent.Children.Add(child5);
        columnParent.Children.Add(child6);

        // Test the column layout with SpaceBetween
        columnParent.ComputeDimensions(800, 600); // Pass in some parent dimensions
        columnParent.ComputePositions(0, 0);

        // Calculate expected spacing
        var totalColumnHeight = child4.ComputedHeight + child5.ComputedHeight + child6.ComputedHeight; // 90
        var columnSpacing =
            (columnParent.ComputedHeight - totalColumnHeight) /
            (columnParent.Children.Count - 1); // (200 - 90) / 2 = 55

        // Check positions in column layout
        Assert.AreEqual(0, child4.Y); // First child at start
        Assert.AreEqual(child4.ComputedHeight + columnSpacing, child5.Y); // Second child after spacing (30 + 55 = 85)
        Assert.AreEqual(child4.ComputedHeight + child5.ComputedHeight + 2 * columnSpacing,
            child6.Y); // Third child (30 + 30 + 2*55 = 170)
    }

    [TestMethod]
    public void UIElement_FullWidth_InheritsParentWidth()
    {
        // Arrange
        var parent = new UIElement(_mockGame)
        {
            FixedWidth = 200,
            FixedHeight = 100
        };

        var child = new UIElement(_mockGame)
        {
            FullWidth = true,
            FixedHeight = 50
        };

        parent.Children.Add(child);

        // Act
        parent.ComputeDimensions(800, 600);
        parent.ComputePositions(0, 0);

        // Assert
        Assert.AreEqual(200, parent.ComputedWidth);
        Assert.AreEqual(200, child.ComputedWidth); // Child should inherit parent's width
    }

    [TestMethod]
    public void UIElement_FullHeight_InheritsParentHeight()
    {
        // Arrange
        var parent = new UIElement(_mockGame)
        {
            FixedWidth = 200,
            FixedHeight = 100
        };

        var child = new UIElement(_mockGame)
        {
            FixedWidth = 50,
            FullHeight = true
        };

        parent.Children.Add(child);

        // Act
        parent.ComputeDimensions(800, 600);
        parent.ComputePositions(0, 0);

        // Assert
        Assert.AreEqual(100, parent.ComputedHeight);
        Assert.AreEqual(100, child.ComputedHeight); // Child should inherit parent's height
    }

    [TestMethod]
    public void UIElement_ChildrenAlign_Stretch_Works_Correctly()
    {
        // Arrange - Create a container with stretch alignment
        var container = new UIElement(_mockGame)
        {
            FixedWidth = 200,
            FixedHeight = 100,
            ChildrenDirection = ChildrenDirection.Column,
            ChildrenAlign = ChildrenAlign.Stretch
        };

        // Create children with different widths
        var child1 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 50,  // Narrower than container
            FixedHeight = 30
        };

        var child2 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.DarkBlue,
            FixedWidth = 80,  // Wider than child1 but narrower than container
            FixedHeight = 20
        };

        // Add children to container
        container.Children.Add(child1);
        container.Children.Add(child2);

        // Act - Compute dimensions and positions
        container.ComputeDimensions(800, 600);
        container.ComputePositions(0, 0);

        // Assert
        // Container should maintain its fixed dimensions
        Assert.AreEqual(200, container.ComputedWidth);
        Assert.AreEqual(100, container.ComputedHeight);

        // Children should be stretched to container width
        Assert.AreEqual(200, child1.ComputedWidth); // Stretched to container width
        Assert.AreEqual(30, child1.ComputedHeight);  // Height unchanged
        Assert.AreEqual(200, child2.ComputedWidth); // Stretched to container width
        Assert.AreEqual(20, child2.ComputedHeight);  // Height unchanged

        // Children should be positioned at the start (X position)
        Assert.AreEqual(0, child1.X);
        Assert.AreEqual(0, child1.Y);
        Assert.AreEqual(0, child2.X);
        Assert.AreEqual(30, child2.Y); // After child1
    }

    [TestMethod]
    public void UIElement_ChildrenAlign_Stretch_Works_In_Row_Direction()
    {
        // Arrange - Create a container with stretch alignment and row direction
        var container = new UIElement(_mockGame)
        {
            FixedWidth = 200,
            FixedHeight = 100,
            ChildrenDirection = ChildrenDirection.Row,
            ChildrenAlign = ChildrenAlign.Stretch
        };

        // Create children with different heights
        var child1 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 50,
            FixedHeight = 30  // Shorter than container
        };

        var child2 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.DarkBlue,
            FixedWidth = 80,
            FixedHeight = 60  // Taller than child1 but shorter than container
        };

        // Add children to container
        container.Children.Add(child1);
        container.Children.Add(child2);

        // Act - Compute dimensions and positions
        container.ComputeDimensions(800, 600);
        container.ComputePositions(0, 0);

        // Assert
        // Container should maintain its fixed dimensions
        Assert.AreEqual(200, container.ComputedWidth);
        Assert.AreEqual(100, container.ComputedHeight);

        // Children should be stretched to container height
        Assert.AreEqual(50, child1.ComputedWidth);   // Width unchanged
        Assert.AreEqual(100, child1.ComputedHeight); // Stretched to container height
        Assert.AreEqual(80, child2.ComputedWidth);   // Width unchanged
        Assert.AreEqual(100, child2.ComputedHeight); // Stretched to container height

        // Children should be positioned properly
        Assert.AreEqual(0, child1.X);
        Assert.AreEqual(0, child1.Y);
        Assert.AreEqual(50, child2.X); // After child1
        Assert.AreEqual(0, child2.Y);
    }

    [TestMethod]
    public void UIElement_Stretch_Works_For_Dropdown_Scenario()
    {
        // Arrange - Create a dropdown container with stretch alignment
        var dropdown = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Gray,
            ChildrenDirection = ChildrenDirection.Column,
            ChildrenAlign = ChildrenAlign.Stretch
        };

        // Create dropdown items with different intrinsic widths
        var item1 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 80,  // Medium width
            FixedHeight = 30
        };

        var item2 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.White,
            FixedWidth = 120, // Widest item
            FixedHeight = 30
        };

        var item3 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 60,  // Narrowest item
            FixedHeight = 30
        };

        // Add items to dropdown
        dropdown.Children.Add(item1);
        dropdown.Children.Add(item2);
        dropdown.Children.Add(item3);

        // Act - Compute dimensions and positions
        dropdown.ComputeDimensions(800, 600);
        dropdown.ComputePositions(0, 0);

        // Assert
        // Dropdown container width should be determined by the widest child
        Assert.AreEqual(120, dropdown.ChildrenWidth); // Width of the widest item
        Assert.AreEqual(120, dropdown.ComputedWidth); // Container adapts to widest item
        Assert.AreEqual(90, dropdown.ComputedHeight);  // Sum of item heights

        // All items should be stretched to the width of the container (which is the width of the widest item)
        Assert.AreEqual(120, item1.ComputedWidth); // Stretched to match widest item
        Assert.AreEqual(120, item2.ComputedWidth); // Already the widest, unchanged
        Assert.AreEqual(120, item3.ComputedWidth); // Stretched to match widest item

        // Items should be positioned correctly
        Assert.AreEqual(0, item1.X);
        Assert.AreEqual(0, item1.Y);
        Assert.AreEqual(0, item2.X);
        Assert.AreEqual(30, item2.Y); // After item1
        Assert.AreEqual(0, item3.X);
        Assert.AreEqual(60, item3.Y); // After item1 and item2
    }
}