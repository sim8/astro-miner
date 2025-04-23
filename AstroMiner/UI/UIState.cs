using Microsoft.Xna.Framework;
using System;

namespace AstroMiner.UI;

public class UIState(BaseGame game)
{
    public UIElement Root { get; private set; }

    public UIElement GetTree()
    {
        var root = new UIElement(game.Textures)
        {
            FullWidth = true,
            FullHeight = true,
            ChildrenDirection = ChildrenDirection.Column,
            ChildrenAlign = ChildrenAlign.Center,
            ChildrenJustify = ChildrenJustify.SpaceBetween,
            Children =
            [
                new UIElement(game.Textures)
                {
                    BackgroundColor = Color.Green,
                    FullWidth = true,
                    ChildrenAlign = ChildrenAlign.Start,
                    ChildrenDirection = ChildrenDirection.Row,
                    ChildrenJustify = ChildrenJustify.SpaceBetween,
                    Children =
                    [
                        new UIElement(game.Textures)
                        {
                            BackgroundColor = Color.Yellow,
                            FixedWidth = 200,
                            FixedHeight = 100
                        },

                        new UITextElement(game.Textures)
                        {
                            Text = "DEBUG",
                            Scale = 4,
                            Color = Color.Aqua,
                            BackgroundColor = Color.Red,
                            OnClick = () => Console.WriteLine("Debug")
                        }
                    ]
                },
                new UIElement(game.Textures)
                {
                    BackgroundColor = Color.Gold,
                    FixedWidth = 200,
                    FixedHeight = 100
                }
            ]
        };

        return root;
    }

    public void Update(GameTime gameTime)
    {
        Root = GetTree();

        Root.ComputeDimensions(game.Graphics.GraphicsDevice.Viewport.Width,
            game.Graphics.GraphicsDevice.Viewport.Height);
        Root.ComputePositions(0, 0);
    }

    /// <summary>
    /// Handles mouse clicks and routes them to the appropriate UI element
    /// </summary>
    /// <param name="x">The x-coordinate of the mouse click</param>
    /// <param name="y">The y-coordinate of the mouse click</param>
    public void OnMouseClick(int x, int y)
    {
        if (Root != null)
        {
            Root.HandleClick(x, y);
        }
    }
}