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

public class UIElement(BaseGame game)
{
    public Color? BackgroundColor { get; set; }
    public int? FixedWidth { get; set; }
    public int? FixedHeight { get; set; }

    public int ChildrenWidth { get; set; }
    public int ChildrenHeight { get; set; }

    public int ComputedWidth => FixedWidth ?? ChildrenWidth;
    public int ComputedHeight => FixedHeight ?? ChildrenHeight;

    public ChildrenDirection ChildrenDirection { get; set; } = ChildrenDirection.Column;
    public ChildrenJustify ChildrenJustify { get; set; } = ChildrenJustify.Start;
    public ChildrenAlign ChildrenAlign { get; set; } = ChildrenAlign.Start;

    public int X { get; set; }
    public int Y { get; set; }
    public List<UIElement> Children { get; } = new();

    public void Render(SpriteBatch spriteBatch)
    {
        if (BackgroundColor.HasValue)
            spriteBatch.Draw(game.Textures["white"],
                new Rectangle(X, Y, ComputedWidth, ComputedHeight),
                BackgroundColor.Value);
        foreach (var child in Children) child.Render(spriteBatch);
    }

    private (int, int) ComputeChildDimensions()
    {
        if (ChildrenDirection == ChildrenDirection.Column)
        {
            var maxWidth = 0;
            var totalHeight = 0;
            foreach (var child in Children)
            {
                var (childWidth, childHeight) = child.ComputeDimensions();
                maxWidth = Math.Max(maxWidth, childWidth);
                totalHeight += childHeight;
            }

            return (maxWidth, totalHeight);
        }

        var totalWidth = 0;
        var maxHeight = 0;
        foreach (var child in Children)
        {
            var (childWidth, childHeight) = child.ComputeDimensions();
            totalWidth += childWidth;
            maxHeight = Math.Max(maxHeight, childHeight);
        }

        return (totalWidth, maxHeight);
    }

    public (int, int) ComputeDimensions()
    {
        var (width, height) = ComputeChildDimensions();
        ChildrenWidth = width;
        ChildrenHeight = height;

        return (FixedWidth ?? width, FixedHeight ?? height);
    }

    public void ComputePositions(int originX, int originY)
    {
        X = originX;
        Y = originY;

        if (ChildrenDirection == ChildrenDirection.Column)
        {
            // var cursorX = ChildrenAlign switch
            // {
            //     ChildrenAlign.Start => originX,
            //     ChildrenAlign.Center => originX + ComputedWidth / 2 - ChildrenWidth / 2,
            //     ChildrenAlign.End => originX + ComputedWidth - ChildrenWidth,
            //     _ => throw new ArgumentOutOfRangeException()
            // };
            var cursorY = originY;
            foreach (var child in Children)
            {
                var childX = ChildrenAlign switch
                {
                    ChildrenAlign.Start => originX,
                    ChildrenAlign.Center => originX + ComputedWidth / 2 - child.ComputedWidth / 2,
                    ChildrenAlign.End => originX + ComputedWidth - child.ComputedWidth,
                    _ => throw new ArgumentOutOfRangeException()
                };
                child.ComputePositions(childX, cursorY);
                cursorY += child.ComputedHeight;
            }
        }
        else
        {
            Console.WriteLine("UNHANDLED");
        }
    }
}