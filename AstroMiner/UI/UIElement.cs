using System;
using System.Collections.Generic;
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
    End
}

public class UIElement(Dictionary<string, Texture2D> textures)
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
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            var child = Children[i];
            if (child.Contains(x, y) && child.HandleClick(x, y))
            {
                return true; // Click was handled by a child
            }
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
            spriteBatch.Draw(textures["white"],
                new Rectangle(X, Y, ComputedWidth, ComputedHeight),
                BackgroundColor.Value);
        foreach (var child in Children) child.Render(spriteBatch);
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
            return FixedWidth.Value;
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
            return FixedHeight.Value;
        return 0; // Will be adjusted later based on children if needed
    }

    /// <summary>
    ///     Measures children and adjusts element size if dimensions weren't explicitly set.
    /// </summary>
    private void MeasureChildrenAndAdjustSize(int availableWidth, int availableHeight)
    {
        var childrenDimensions = MeasureChildren(availableWidth, availableHeight);
        ChildrenWidth = childrenDimensions.width;
        ChildrenHeight = childrenDimensions.height;

        // If width/height weren't explicitly set, use children dimensions
        var isColumn = ChildrenDirection == ChildrenDirection.Column;

        if (!FullWidth && !FixedWidth.HasValue)
            ComputedWidth = ChildrenWidth;

        if (!FullHeight && !FixedHeight.HasValue)
            ComputedHeight = ChildrenHeight;
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

        var isColumn = ChildrenDirection == ChildrenDirection.Column;

        // Get total size of children along primary axis
        var totalChildrenSize = 0;
        foreach (var child in Children) totalChildrenSize += isColumn ? child.ComputedHeight : child.ComputedWidth;

        // Calculate spacing for SpaceBetween
        float spacing = 0;
        if (ChildrenJustify == ChildrenJustify.SpaceBetween && Children.Count > 1)
        {
            var availableSpace = isColumn ? ComputedHeight : ComputedWidth;
            spacing = (availableSpace - totalChildrenSize) / (float)(Children.Count - 1);
        }

        // Set starting position for primary axis
        var primaryPos = isColumn ? Y : X;

        // Adjust starting position based on justify
        if (ChildrenJustify == ChildrenJustify.End)
        {
            var availableSpace = isColumn ? ComputedHeight : ComputedWidth;
            primaryPos += availableSpace - totalChildrenSize;
        }
        // No adjustment needed for Start

        foreach (var child in Children)
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
            if (ChildrenJustify == ChildrenJustify.SpaceBetween && Children.Count > 1) primaryPos += (int)spacing;
        }
    }

    private int CalculateSecondaryPosition(UIElement child, bool isColumn)
    {
        if (isColumn)
            // Column layout: Secondary is X position
            return ChildrenAlign switch
            {
                ChildrenAlign.Start => X,
                ChildrenAlign.Center => X + ComputedWidth / 2 - child.ComputedWidth / 2,
                ChildrenAlign.End => X + ComputedWidth - child.ComputedWidth,
                _ => throw new ArgumentOutOfRangeException()
            };

        // Row layout: Secondary is Y position
        return ChildrenAlign switch
        {
            ChildrenAlign.Start => Y,
            ChildrenAlign.Center => Y + ComputedHeight / 2 - child.ComputedHeight / 2,
            ChildrenAlign.End => Y + ComputedHeight - child.ComputedHeight,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}