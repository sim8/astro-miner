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

    public void Render(SpriteBatch spriteBatch)
    {
        if (BackgroundColor.HasValue)
            spriteBatch.Draw(textures["white"],
                new Rectangle(X, Y, ComputedWidth, ComputedHeight),
                BackgroundColor.Value);
        foreach (var child in Children) child.Render(spriteBatch);
    }

    protected virtual (int, int) ComputeChildDimensions(int parentWidth, int parentHeight)
    {
        // First compute our own dimensions to pass to children

        var isColumn = ChildrenDirection == ChildrenDirection.Column;
        var primarySize = 0;
        var secondarySize = 0;

        foreach (var child in Children)
        {
            var (childWidth, childHeight) = child.ComputeDimensions(ComputedWidth, ComputedHeight);

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

        // If we didn't have fixed dimensions, use child dimensions
        if (!FullWidth && !FixedWidth.HasValue)
            ComputedWidth = isColumn ? secondarySize : primarySize;

        if (!FullHeight && !FixedHeight.HasValue)
            ComputedHeight = isColumn ? primarySize : secondarySize;

        return isColumn ? (secondarySize, primarySize) : (primarySize, secondarySize);
    }

    public (int, int) ComputeDimensions(int parentWidth, int parentHeight)
    {
        // var fixedOrParentWidth = FixedWidth ?? (FullWidth ? parentWidth : null);
        // var fixedOrParentHeight = FixedHeight ?? (FullHeight ? parentHeight : null);
        //
        // var (childrenWidth, childrenHeight) = ComputeChildDimensions(fixedOrParentWidth ?? 0, fixedOrParentHeight ?? 0);
        // ChildrenWidth = childrenWidth;
        // ChildrenHeight = childrenHeight;
        // ComputedWidth = fixedOrParentWidth ?? ChildrenWidth;
        // ComputedHeight = fixedOrParentHeight ?? ChildrenHeight;

        ComputedWidth = FullWidth ? parentWidth : FixedWidth ?? 0;
        ComputedHeight = FullHeight ? parentHeight : FixedHeight ?? 0;

        var (childrenWidth, childrenHeight) = ComputeChildDimensions(ComputedWidth, ComputedHeight);
        ChildrenWidth = childrenWidth;
        ChildrenHeight = childrenHeight;

        return (ComputedWidth, ComputedHeight);

        return (ComputedWidth, ComputedHeight);
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