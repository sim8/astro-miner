using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIState(BaseGame game)
{
    public UIElement Root { get; private set; }

    public UIElement GetTree()
    {
        var root = new UIElement(game.Textures)
        {
            FixedWidth = game.Graphics.GraphicsDevice.Viewport.Width,
            FixedHeight = game.Graphics.GraphicsDevice.Viewport.Height,
            ChildrenAlign = ChildrenAlign.Center,
            Children =
            [
                new UIElement(game.Textures)
                {
                    BackgroundColor = Color.Green,
                    ChildrenAlign = ChildrenAlign.Center,
                    Children =
                    [
                        new UIElement(game.Textures)
                        {
                            BackgroundColor = Color.LightGray,
                            FixedWidth = 200,
                            FixedHeight = 100
                        },

                        new UIElement(game.Textures)
                        {
                            BackgroundColor = Color.DarkBlue,
                            FixedWidth = 150,
                            FixedHeight = 75
                        }
                    ]
                }
            ]
        };

        return root;
    }

    public void Update(GameTime gameTime)
    {
        Root = GetTree();
        Root.FixedWidth = game.Graphics.GraphicsDevice.Viewport.Width;
        Root.FixedHeight = game.Graphics.GraphicsDevice.Viewport.Height;

        Root.ComputeDimensions();
        Root.ComputePositions(0, 0);
    }
}