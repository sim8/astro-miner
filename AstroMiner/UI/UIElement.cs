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

    private (int, int) ComputeChildDimensions(int parentWidth, int parentHeight)
    {
        // First compute our own dimensions to pass to children
        ComputedWidth = FullWidth ? parentWidth : FixedWidth ?? 0;
        ComputedHeight = FullHeight ? parentHeight : FixedHeight ?? 0;

        if (ChildrenDirection == ChildrenDirection.Column)
        {
            var maxWidth = 0;
            var totalHeight = 0;
            foreach (var child in Children)
            {
                var (childWidth, childHeight) = child.ComputeDimensions(ComputedWidth, ComputedHeight);
                maxWidth = Math.Max(maxWidth, childWidth);
                totalHeight += childHeight;
            }

            // If we didn't have fixed dimensions, use child dimensions
            if (!FullWidth && !FixedWidth.HasValue) ComputedWidth = maxWidth;
            if (!FullHeight && !FixedHeight.HasValue) ComputedHeight = totalHeight;

            return (maxWidth, totalHeight);
        }

        var totalWidth = 0;
        var maxHeight = 0;
        foreach (var child in Children)
        {
            var (childWidth, childHeight) = child.ComputeDimensions(ComputedWidth, ComputedHeight);
            totalWidth += childWidth;
            maxHeight = Math.Max(maxHeight, childHeight);
        }

        // If we didn't have fixed dimensions, use child dimensions
        if (!FullWidth && !FixedWidth.HasValue) ComputedWidth = totalWidth;
        if (!FullHeight && !FixedHeight.HasValue) ComputedHeight = maxHeight;

        return (totalWidth, maxHeight);
    }

    public (int, int) ComputeDimensions(int parentWidth, int parentHeight)
    {
        var (childrenWidth, childrenHeight) = ComputeChildDimensions(parentWidth, parentHeight);
        ChildrenWidth = childrenWidth;
        ChildrenHeight = childrenHeight;

        return (ComputedWidth, ComputedHeight);
    }

    public void ComputePositions(int originX, int originY)
    {
        // Set element position based on origin and any parent alignment that might be applied
        X = originX;
        Y = originY;

        // Position children within this element
        if (ChildrenDirection == ChildrenDirection.Column)
        {
            int totalChildrenHeight = 0;
            foreach (var child in Children)
            {
                totalChildrenHeight += child.ComputedHeight;
            }

            // Calculate spacing for SpaceBetween
            float spacing = 0;
            if (ChildrenJustify == ChildrenJustify.SpaceBetween && Children.Count > 1)
            {
                spacing = (ComputedHeight - totalChildrenHeight) / (float)(Children.Count - 1);
            }

            var cursorY = Y;

            // Adjust starting position based on justify
            if (ChildrenJustify == ChildrenJustify.End)
            {
                cursorY = Y + ComputedHeight - totalChildrenHeight;
            }
            else if (ChildrenJustify == ChildrenJustify.Start)
            {
                // No adjustment needed for Start
            }

            foreach (var child in Children)
            {
                var childX = ChildrenAlign switch
                {
                    ChildrenAlign.Start => X,
                    ChildrenAlign.Center => X + ComputedWidth / 2 - child.ComputedWidth / 2,
                    ChildrenAlign.End => X + ComputedWidth - child.ComputedWidth,
                    _ => throw new ArgumentOutOfRangeException()
                };
                child.ComputePositions(childX, cursorY);
                cursorY += child.ComputedHeight;

                if (ChildrenJustify == ChildrenJustify.SpaceBetween && Children.Count > 1)
                {
                    cursorY += (int)spacing;
                }
            }
        }
        else
        {
            int totalChildrenWidth = 0;
            foreach (var child in Children)
            {
                totalChildrenWidth += child.ComputedWidth;
            }

            // Calculate spacing for SpaceBetween
            float spacing = 0;
            if (ChildrenJustify == ChildrenJustify.SpaceBetween && Children.Count > 1)
            {
                spacing = (ComputedWidth - totalChildrenWidth) / (float)(Children.Count - 1);
            }

            var cursorX = X;

            // Adjust starting position based on justify
            if (ChildrenJustify == ChildrenJustify.End)
            {
                cursorX = X + ComputedWidth - totalChildrenWidth;
            }
            else if (ChildrenJustify == ChildrenJustify.Start)
            {
                // No adjustment needed for Start
            }

            foreach (var child in Children)
            {
                var childY = ChildrenAlign switch
                {
                    ChildrenAlign.Start => Y,
                    ChildrenAlign.Center => Y + ComputedHeight / 2 - child.ComputedHeight / 2,
                    ChildrenAlign.End => Y + ComputedHeight - child.ComputedHeight,
                    _ => throw new ArgumentOutOfRangeException()
                };
                child.ComputePositions(cursorX, childY);
                cursorX += child.ComputedWidth;

                if (ChildrenJustify == ChildrenJustify.SpaceBetween && Children.Count > 1)
                {
                    cursorX += (int)spacing;
                }
            }
        }
    }
}