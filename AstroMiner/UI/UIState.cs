using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIState(BaseGame game)
{
    public UIElement Root { get; private set; }

    public UIElement GetTree()
    {
        var root = new UIElement(game.Textures);
        root.FixedWidth = game.Graphics.GraphicsDevice.Viewport.Width;
        root.FixedHeight = game.Graphics.GraphicsDevice.Viewport.Height;
        root.ChildrenAlign = ChildrenAlign.Center;

        var container = new UIElement(game.Textures)
        {
            BackgroundColor = Color.Green,
            ChildrenAlign = ChildrenAlign.Center
        };

        container.Children.Add(new UIElement(game.Textures)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 200,
            FixedHeight = 100
        });

        container.Children.Add(new UIElement(game.Textures)
        {
            BackgroundColor = Color.DarkBlue,
            FixedWidth = 150,
            FixedHeight = 75
        });

        root.Children.Add(container);

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