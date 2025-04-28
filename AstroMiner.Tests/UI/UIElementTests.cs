using System.Collections.Generic;
using AstroMiner.UI;
using AstroMiner.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Tests.UI;

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
            ChildrenAlign = ChildrenAlign.Stretch,
            Padding = 10 // Add padding to test padding + stretch interaction
        };

        // Create children with different widths
        var child1 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 50, // Narrower than container
            FixedHeight = 30
        };

        var child2 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.DarkBlue,
            FixedWidth = 80, // Wider than child1 but narrower than container
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

        // Children should be stretched to container width minus padding
        Assert.AreEqual(200 - 2 * 10, child1.ComputedWidth); // Stretched to container width minus padding
        Assert.AreEqual(30, child1.ComputedHeight); // Height unchanged
        Assert.AreEqual(200 - 2 * 10, child2.ComputedWidth); // Stretched to container width minus padding
        Assert.AreEqual(20, child2.ComputedHeight); // Height unchanged

        // Children should be positioned with padding offset
        Assert.AreEqual(10, child1.X); // X position includes left padding
        Assert.AreEqual(10, child1.Y); // Y position includes top padding
        Assert.AreEqual(10, child2.X); // X position includes left padding
        Assert.AreEqual(10 + 30, child2.Y); // After child1 with padding offset
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
            ChildrenAlign = ChildrenAlign.Stretch,
            Padding = 5 // Add padding to test padding + stretch interaction
        };

        // Create children with different heights
        var child1 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 50,
            FixedHeight = 30 // Shorter than container
        };

        var child2 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.DarkBlue,
            FixedWidth = 80,
            FixedHeight = 60 // Taller than child1 but shorter than container
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

        // Children should be stretched to container height minus padding
        Assert.AreEqual(50, child1.ComputedWidth); // Width unchanged
        Assert.AreEqual(100 - 2 * 5, child1.ComputedHeight); // Stretched to container height minus padding
        Assert.AreEqual(80, child2.ComputedWidth); // Width unchanged
        Assert.AreEqual(100 - 2 * 5, child2.ComputedHeight); // Stretched to container height minus padding

        // Children should be positioned properly with padding included
        Assert.AreEqual(5, child1.X); // X position includes left padding
        Assert.AreEqual(5, child1.Y); // Y position includes top padding
        Assert.AreEqual(5 + 50, child2.X); // After child1 with padding offset
        Assert.AreEqual(5, child2.Y); // Y position includes top padding
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
            FixedWidth = 80, // Medium width
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
            FixedWidth = 60, // Narrowest item
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
        Assert.AreEqual(90, dropdown.ComputedHeight); // Sum of item heights

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

    [TestMethod]
    public void UIElement_Padding_Works_Correctly()
    {
        // Arrange
        var container = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Green,
            Padding = 10, // Add padding of 10px on all sides
            FixedWidth = 200,
            FixedHeight = 150
        };

        var child = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Blue,
            FixedWidth = 100,
            FixedHeight = 50
        };

        container.Children.Add(child);

        // Act
        container.ComputeDimensions(800, 600);
        container.ComputePositions(0, 0);

        // Assert
        // Container dimensions should remain as specified
        Assert.AreEqual(200, container.ComputedWidth);
        Assert.AreEqual(150, container.ComputedHeight);

        // Child should be positioned with padding offset
        Assert.AreEqual(10, child.X); // X position should include left padding
        Assert.AreEqual(10, child.Y); // Y position should include top padding

        // Verify content area dimensions
        var contentWidth = container.ComputedWidth - 2 * container.Padding; // 200 - 20 = 180
        var contentHeight = container.ComputedHeight - 2 * container.Padding; // 150 - 20 = 130
        Assert.AreEqual(180, contentWidth);
        Assert.AreEqual(130, contentHeight);

        // Add a second test for auto-sized container with padding
        var autoContainer = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Red,
            Padding = 15
        };

        var autoChild = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Yellow,
            FixedWidth = 100,
            FixedHeight = 50
        };

        autoContainer.Children.Add(autoChild);

        // Act
        autoContainer.ComputeDimensions(800, 600);
        autoContainer.ComputePositions(0, 0);

        // Assert
        // Container should size itself based on child plus padding
        Assert.AreEqual(100 + 2 * 15, autoContainer.ComputedWidth); // Child width + padding on both sides
        Assert.AreEqual(50 + 2 * 15, autoContainer.ComputedHeight); // Child height + padding on both sides

        // Child should be positioned with padding offset
        Assert.AreEqual(15, autoChild.X);
        Assert.AreEqual(15, autoChild.Y);

        // Add a test with multiple children to verify padding works with layout
        var multiContainer = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Gray,
            Padding = 20,
            ChildrenDirection = ChildrenDirection.Column
        };

        var child1 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Red,
            FixedWidth = 50,
            FixedHeight = 30
        };

        var child2 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Blue,
            FixedWidth = 80,
            FixedHeight = 40
        };

        multiContainer.Children.Add(child1);
        multiContainer.Children.Add(child2);

        // Act
        multiContainer.ComputeDimensions(800, 600);
        multiContainer.ComputePositions(0, 0);

        // Assert
        // Container size includes padding on both sides
        Assert.AreEqual(80 + 2 * 20, multiContainer.ComputedWidth); // Widest child + padding
        Assert.AreEqual(70 + 2 * 20, multiContainer.ComputedHeight); // Sum of children heights + padding

        // First child is positioned with padding offset
        Assert.AreEqual(20, child1.X);
        Assert.AreEqual(20, child1.Y);

        // Second child is positioned below first child but still respects padding
        Assert.AreEqual(20, child2.X);
        Assert.AreEqual(20 + 30, child2.Y); // Padding + height of first child
    }

    [TestMethod]
    public void UITextElement_Padding_Works_Correctly()
    {
        // Mock the FontHelpers.TransformString method with test data
        var testChar = (x: 0, y: 0, width: 8, height: 10);
        FontHelpers.MockCharacterSizeForTest = testChar;

        // Create a text element with padding
        var textElement = new UITextElement(_mockGame)
        {
            Text = "Test", // 4 characters
            Padding = 5,
            Scale = 1
        };

        // Act
        textElement.ComputeDimensions(800, 600);
        textElement.ComputePositions(0, 0);

        // Assert
        // Text width should be 4 chars * 8 pixels = 32 pixels
        // Text height should be 10 pixels
        // Total width with padding should be 32 + 2*5 = 42
        // Total height with padding should be 10 + 2*5 = 20
        Assert.AreEqual(32 + 2 * 5, textElement.ComputedWidth);
        Assert.AreEqual(10 + 2 * 5, textElement.ComputedHeight);

        // Position should be set to the provided values
        Assert.AreEqual(0, textElement.X);
        Assert.AreEqual(0, textElement.Y);
    }

    [TestMethod]
    public void UIElement_AbsolutePosition_Works_Correctly()
    {
        // Arrange - Create a container with standard flow children and an absolute child
        var container = new UIElement(_mockGame)
        {
            FixedWidth = 200,
            FixedHeight = 150,
            BackgroundColor = Color.Gray,
            Padding = 10
        };

        // Create flow children
        var flowChild1 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Blue,
            FixedWidth = 80,
            FixedHeight = 30
        };

        var flowChild2 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Green,
            FixedWidth = 80,
            FixedHeight = 30
        };

        // Create absolute positioned child
        var absoluteChild = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Red,
            FixedWidth = 50,
            FixedHeight = 50,
            Position = PositionMode.Absolute
        };

        // Add children to container
        container.Children.Add(flowChild1);
        container.Children.Add(absoluteChild); // Add absolute child between flow children in the hierarchy
        container.Children.Add(flowChild2);

        // Act - Compute dimensions and positions
        container.ComputeDimensions(800, 600);
        container.ComputePositions(20, 30); // Position container at (20, 30)

        // Assert
        // Container should maintain its fixed dimensions and position
        Assert.AreEqual(20, container.X);
        Assert.AreEqual(30, container.Y);
        Assert.AreEqual(200, container.ComputedWidth);
        Assert.AreEqual(150, container.ComputedHeight);

        // Flow children should be positioned according to normal flow rules with padding
        Assert.AreEqual(20 + 10, flowChild1.X); // container.X + padding
        Assert.AreEqual(30 + 10, flowChild1.Y); // container.Y + padding
        Assert.AreEqual(20 + 10, flowChild2.X); // container.X + padding
        Assert.AreEqual(30 + 10 + 30, flowChild2.Y); // container.Y + padding + flowChild1.Height

        // Absolute child should be positioned relative to container origin, ignoring padding and flow
        Assert.AreEqual(20, absoluteChild.X); // container.X (no padding applied)
        Assert.AreEqual(30, absoluteChild.Y); // container.Y (no padding applied)

        // Absolute child should still have its dimensions computed correctly
        Assert.AreEqual(50, absoluteChild.ComputedWidth);
        Assert.AreEqual(50, absoluteChild.ComputedHeight);

        // The absolute child should not affect the layout of flow children
        Assert.AreEqual(80, container.ChildrenWidth); // Only flow children widths count
        Assert.AreEqual(60, container.ChildrenHeight); // Only flow children heights count (30 + 30)
    }

    [TestMethod]
    public void UIElement_AbsoluteElement_Children_Layout_Works()
    {
        // Arrange - Create an absolute positioned parent with child elements
        var container = new UIElement(_mockGame)
        {
            FixedWidth = 300,
            FixedHeight = 200,
            BackgroundColor = Color.Gray
        };

        // Create an absolute positioned element with its own children
        var absoluteParent = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Red,
            Position = PositionMode.Absolute,
            ChildrenDirection = ChildrenDirection.Column,
            Padding = 5
        };

        // Add flow children to the absolute element
        var child1 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Blue,
            FixedWidth = 50,
            FixedHeight = 20
        };

        var child2 = new UIElement(_mockGame)
        {
            BackgroundColor = Color.Green,
            FixedWidth = 70,
            FixedHeight = 30
        };

        // Build the hierarchy
        absoluteParent.Children.Add(child1);
        absoluteParent.Children.Add(child2);
        container.Children.Add(absoluteParent);

        // Act - Compute dimensions and positions
        container.ComputeDimensions(800, 600);
        container.ComputePositions(10, 10);

        // Assert
        // Container dimensions should be as specified
        Assert.AreEqual(300, container.ComputedWidth);
        Assert.AreEqual(200, container.ComputedHeight);

        // Absolute parent should size according to its children plus padding
        Assert.AreEqual(70 + 2 * 5, absoluteParent.ComputedWidth); // Width of widest child + padding
        Assert.AreEqual(50 + 2 * 5, absoluteParent.ComputedHeight); // Sum of children heights + padding

        // Absolute parent should be positioned at container's origin
        Assert.AreEqual(10, absoluteParent.X);
        Assert.AreEqual(10, absoluteParent.Y);

        // Children of absolute element should be positioned correctly within it
        Assert.AreEqual(10 + 5, child1.X); // container.X + parent padding
        Assert.AreEqual(10 + 5, child1.Y); // container.Y + parent padding

        Assert.AreEqual(10 + 5, child2.X); // container.X + parent padding
        Assert.AreEqual(10 + 5 + 20, child2.Y); // container.Y + parent padding + child1.Height
    }

    [TestMethod]
    public void UIElement_ChildrenJustify_Center_Works_Correctly()
    {
        // Create test elements
        var rowParent = new UIElement(_mockGame)
        {
            FixedWidth = 200,
            FixedHeight = 50,
            ChildrenDirection = ChildrenDirection.Row,
            ChildrenJustify = ChildrenJustify.Center
        };

        var columnParent = new UIElement(_mockGame)
        {
            FixedWidth = 50,
            FixedHeight = 200,
            ChildrenDirection = ChildrenDirection.Column,
            ChildrenJustify = ChildrenJustify.Center
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

        // Set up the row parent with children
        rowParent.Children.Add(child1);
        rowParent.Children.Add(child2);

        // Test the row layout with Center
        rowParent.ComputeDimensions(800, 600);
        rowParent.ComputePositions(0, 0);

        // Calculate expected positions
        var totalRowWidth = child1.ComputedWidth + child2.ComputedWidth; // 60
        var rowCenterOffset = (rowParent.ComputedWidth - totalRowWidth) / 2; // (200 - 60) / 2 = 70

        // Check positions in row layout
        Assert.AreEqual(rowCenterOffset, child1.X); // First child starts at center offset
        Assert.AreEqual(rowCenterOffset + child1.ComputedWidth, child2.X); // Second child follows first

        // Set up the column parent with children
        var child3 = new UIElement(_mockGame) { FixedWidth = 30, FixedHeight = 30 };
        var child4 = new UIElement(_mockGame) { FixedWidth = 30, FixedHeight = 30 };

        columnParent.Children.Add(child3);
        columnParent.Children.Add(child4);

        // Test the column layout with Center
        columnParent.ComputeDimensions(800, 600);
        columnParent.ComputePositions(0, 0);

        // Calculate expected positions
        var totalColumnHeight = child3.ComputedHeight + child4.ComputedHeight; // 60
        var columnCenterOffset = (columnParent.ComputedHeight - totalColumnHeight) / 2; // (200 - 60) / 2 = 70

        // Check positions in column layout
        Assert.AreEqual(columnCenterOffset, child3.Y); // First child starts at center offset
        Assert.AreEqual(columnCenterOffset + child3.ComputedHeight, child4.Y); // Second child follows first
    }
}