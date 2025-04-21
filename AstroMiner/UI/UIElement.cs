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

    public int X { get; set; }
    public int Y { get; set; }
    public List<UIElement> Children { get; } = new();

    public void Render(SpriteBatch spriteBatch)
    {
        if (BackgroundColor.HasValue)
            spriteBatch.Draw(game.Textures["white"],
                new Rectangle(X, Y, ComputedWidth, ComputedHeight),
                BackgroundColor.Value);

        if (BackgroundColor.HasValue && ComputedHeight == 0)
        {
            var wow = 2;
        }
        // if (BackgroundColor.HasValue) Console.WriteLine("Rendering size: " + ComputedWidth + "," + ComputedHeight);

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
            var cursorX = originX;
            var cursorY = originY;
            foreach (var child in Children)
            {
                child.ComputePositions(cursorX, cursorY);
                cursorY += child.ComputedHeight;
            }
        }
        else
        {
            Console.WriteLine("UNHANDLED");
        }
    }
}