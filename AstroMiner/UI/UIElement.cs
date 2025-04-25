using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public enum ChildrenDirection
{
    Row,
    Column
}

public enum ChildrenJustify
{
    Start,
    End,
    SpaceBetween
}

public enum ChildrenAlign
{
    Start,
    Center,
    End,
    Stretch
}

public enum PositionMode
{
    Flow,    // Element follows normal flow layout
    Absolute // Element is positioned absolutely relative to parent
}

public class UIElement(BaseGame game)
{
    public Color? BackgroundColor { get; set; }
    public int? FixedWidth { get; set; }
    public int? FixedHeight { get; set; }

    public bool FullWidth { get; set; } = false;
    public bool FullHeight { get; set; } = false;

    public int ChildrenWidth { get; set; }
    public int ChildrenHeight { get; set; }

    public int ComputedWidth { get; private set; }
    public int ComputedHeight { get; private set; }

    // Padding property, defaulted to 0
    public int Padding { get; set; } = 0;

    // Positioning
    public PositionMode Position { get; set; } = PositionMode.Flow;
    public int ZIndex { get; set; } = 0;

    public ChildrenDirection ChildrenDirection { get; set; } = ChildrenDirection.Column;
    public ChildrenJustify ChildrenJustify { get; set; } = ChildrenJustify.Start;
    public ChildrenAlign ChildrenAlign { get; set; } = ChildrenAlign.Start;

    public int X { get; set; }
    public int Y { get; set; }
    public List<UIElement> Children { get; set; } = new();

    // Add an Action property for click handling
    public Action OnClick { get; set; }

    // Method to check if a point is inside this element
    public bool Contains(int x, int y)
    {
        return x >= X && x < X + ComputedWidth && y >= Y && y < Y + ComputedHeight;
    }

    // Method to handle clicks, returns true if click was handled
    public bool HandleClick(int x, int y)
    {
        // First, check children (in reverse order for proper DOM-like precedence)
        // Later elements in the tree take precedence
        for (var i = Children.Count - 1; i >= 0; i--)
        {
            var child = Children[i];
            if (child.Contains(x, y) && child.HandleClick(x, y)) return true; // Click was handled by a child
        }

        // If no child handled the click, check if this element has a handler
        if (Contains(x, y) && OnClick != null)
        {
            OnClick.Invoke();
            return true; // Click was handled by this element
        }

        return false; // Click was not handled
    }

    public virtual void Render(SpriteBatch spriteBatch)
    {
        if (BackgroundColor.HasValue)
            spriteBatch.Draw(game.Textures["white"],
                new Rectangle(X, Y, ComputedWidth, ComputedHeight),
                BackgroundColor.Value);

        // Render children in z-index order
        foreach (var child in Children.OrderBy(c => c.ZIndex))
            child.Render(spriteBatch);
    }

    /// <summary>
    ///     Computes dimensions for both this element and its children.
    /// </summary>
    /// <param name="parentWidth">The width of the parent container</param>
    /// <param name="parentHeight">The height of the parent container</param>
    /// <returns>A tuple of (width, height) computed for this element</returns>
    public (int, int) ComputeDimensions(int parentWidth, int parentHeight)
    {
        // Step 1: Determine initial dimensions based on fixed values, parent dimensions, or zero if unspecified.
        ComputedWidth = DetermineInitialWidth(parentWidth);
        ComputedHeight = DetermineInitialHeight(parentHeight);

        // Step 2: Calculate children dimensions and update our size if needed
        MeasureChildrenAndAdjustSize(ComputedWidth, ComputedHeight);

        return (ComputedWidth, ComputedHeight);
    }

    /// <summary>
    ///     Determines the initial width based on fixed values, parent dimensions, or zero if unspecified.
    /// </summary>
    private int DetermineInitialWidth(int parentWidth)
    {
        if (FullWidth)
            return parentWidth;
        if (FixedWidth.HasValue)
            return FixedWidth.Value; // Fixed width is considered to include padding
        return 0; // Will be adjusted later based on children if needed
    }

    /// <summary>
    ///     Determines the initial height based on fixed values, parent dimensions, or zero if unspecified.
    /// </summary>
    private int DetermineInitialHeight(int parentHeight)
    {
        if (FullHeight)
            return parentHeight;
        if (FixedHeight.HasValue)
            return FixedHeight.Value; // Fixed height is considered to include padding
        return 0; // Will be adjusted later based on children if needed
    }

    /// <summary>
    ///     Measures children and adjusts element size if dimensions weren't explicitly set.
    /// </summary>
    private void MeasureChildrenAndAdjustSize(int availableWidth, int availableHeight)
    {
        // Adjust available space for children by removing padding
        int adjustedAvailableWidth = availableWidth > 0 ? Math.Max(0, availableWidth - 2 * Padding) : 0;
        int adjustedAvailableHeight = availableHeight > 0 ? Math.Max(0, availableHeight - 2 * Padding) : 0;

        // Compute dimensions for all children first, regardless of position mode
        foreach (var child in Children)
        {
            child.ComputeDimensions(adjustedAvailableWidth, adjustedAvailableHeight);
        }

        // Then only include flow children in parent size calculations
        var childrenDimensions = MeasureChildren(adjustedAvailableWidth, adjustedAvailableHeight);
        ChildrenWidth = childrenDimensions.width;
        ChildrenHeight = childrenDimensions.height;

        // If width/height weren't explicitly set, use children dimensions plus padding
        if (!FullWidth && !FixedWidth.HasValue)
            ComputedWidth = ChildrenWidth + 2 * Padding;

        if (!FullHeight && !FixedHeight.HasValue)
            ComputedHeight = ChildrenHeight + 2 * Padding;
    }

    /// <summary>
    ///     Measures children and returns their combined dimensions.
    ///     Can be overridden by subclasses to provide custom measurement logic.
    /// </summary>
    protected virtual (int width, int height) MeasureChildren(int availableWidth, int availableHeight)
    {
        var isColumn = ChildrenDirection == ChildrenDirection.Column;
        var primarySize = 0;
        var secondarySize = 0;

        foreach (var child in Children)
        {
            // Skip absolute positioned elements in flow layout calculations
            if (child.Position == PositionMode.Absolute)
                continue;

            var (childWidth, childHeight) = child.ComputeDimensions(availableWidth, availableHeight);

            if (isColumn)
            {
                // In column layout, primary is height (sum), secondary is width (max)
                primarySize += childHeight;
                secondarySize = Math.Max(secondarySize, childWidth);
            }
            else
            {
                // In row layout, primary is width (sum), secondary is height (max)
                primarySize += childWidth;
                secondarySize = Math.Max(secondarySize, childHeight);
            }
        }

        return isColumn ? (secondarySize, primarySize) : (primarySize, secondarySize);
    }

    public void ComputePositions(int originX, int originY)
    {
        // Set element position based on origin and any parent alignment that might be applied
        X = originX;
        Y = originY;

        // Position flow children according to layout rules
        PositionFlowChildren();

        // Position absolute children relative to this element
        PositionAbsoluteChildren();
    }

    private void PositionFlowChildren()
    {
        var isColumn = ChildrenDirection == ChildrenDirection.Column;

        // Get flow children and total size along primary axis
        var flowChildren = Children.Where(c => c.Position == PositionMode.Flow).ToList();
        var totalChildrenSize = 0;
        foreach (var child in flowChildren)
            totalChildrenSize += isColumn ? child.ComputedHeight : child.ComputedWidth;

        // Calculate spacing for SpaceBetween
        float spacing = 0;
        if (ChildrenJustify == ChildrenJustify.SpaceBetween && flowChildren.Count > 1)
        {
            var availableSpace = isColumn ? ComputedHeight - 2 * Padding : ComputedWidth - 2 * Padding;
            spacing = (availableSpace - totalChildrenSize) / (float)(flowChildren.Count - 1);
        }

        // Set starting position for primary axis (includes padding)
        var primaryPos = isColumn ? Y + Padding : X + Padding;

        // Adjust starting position based on justify
        if (ChildrenJustify == ChildrenJustify.End && flowChildren.Count > 0)
        {
            var availableSpace = isColumn ? ComputedHeight - 2 * Padding : ComputedWidth - 2 * Padding;
            primaryPos += availableSpace - totalChildrenSize;
        }
        // No adjustment needed for Start

        foreach (var child in flowChildren)
        {
            // Calculate secondary axis position (based on alignment)
            var secondaryPos = CalculateSecondaryPosition(child, isColumn);

            // Set child position
            if (isColumn)
                child.ComputePositions(secondaryPos, primaryPos);
            else
                child.ComputePositions(primaryPos, secondaryPos);

            // Move cursor by child size along primary axis
            primaryPos += isColumn ? child.ComputedHeight : child.ComputedWidth;

            // Add spacing if needed
            if (ChildrenJustify == ChildrenJustify.SpaceBetween && flowChildren.Count > 1) primaryPos += (int)spacing;
        }
    }

    private void PositionAbsoluteChildren()
    {
        // Position absolute children relative to this element's origin
        foreach (var child in Children.Where(c => c.Position == PositionMode.Absolute))
        {
            // For now, just position at 0,0 relative to parent
            child.ComputePositions(X, Y);
        }
    }

    private int CalculateSecondaryPosition(UIElement child, bool isColumn)
    {
        if (isColumn)
        {
            // Column layout: Secondary is X position
            // For Stretch, we modify the child's width first
            if (ChildrenAlign == ChildrenAlign.Stretch)
            {
                // Set the stretched width to the container width minus padding on both sides
                child.ComputedWidth = ComputedWidth - 2 * Padding;
                return X + Padding; // Position at start + padding
            }

            return ChildrenAlign switch
            {
                ChildrenAlign.Start => X + Padding,
                ChildrenAlign.Center => X + Padding + (ComputedWidth - 2 * Padding) / 2 - child.ComputedWidth / 2,
                ChildrenAlign.End => X + ComputedWidth - Padding - child.ComputedWidth,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        // Row layout: Secondary is Y position
        // For Stretch, we modify the child's height first
        if (ChildrenAlign == ChildrenAlign.Stretch)
        {
            // Set the stretched height to the container height minus padding on both sides
            child.ComputedHeight = ComputedHeight - 2 * Padding;
            return Y + Padding; // Position at start + padding
        }

        return ChildrenAlign switch
        {
            ChildrenAlign.Start => Y + Padding,
            ChildrenAlign.Center => Y + Padding + (ComputedHeight - 2 * Padding) / 2 - child.ComputedHeight / 2,
            ChildrenAlign.End => Y + ComputedHeight - Padding - child.ComputedHeight,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}