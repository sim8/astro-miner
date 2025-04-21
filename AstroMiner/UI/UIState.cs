using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIState(BaseGame game)
{
    public UIElement Root { get; private set; }

    public void Initialize()
    {
        Root = new UIElement(game);
        Root.FixedWidth = game.Graphics.GraphicsDevice.Viewport.Width;
        Root.FixedHeight = game.Graphics.GraphicsDevice.Viewport.Height;
        Root.ChildrenAlign = ChildrenAlign.Center;

        var container = new UIElement(game)
        {
            BackgroundColor = Color.Green,
            ChildrenAlign = ChildrenAlign.Center
        };


        // Add example UI elements
        container.Children.Add(new UIElement(game)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 200,
            FixedHeight = 100
        });

        container.Children.Add(new UIElement(game)
        {
            BackgroundColor = Color.DarkBlue,
            FixedWidth = 150,
            FixedHeight = 75
        });

        Root.Children.Add(container);
    }

    public void Update(GameTime gameTime)
    {
        Root.FixedWidth = game.Graphics.GraphicsDevice.Viewport.Width;
        Root.FixedHeight = game.Graphics.GraphicsDevice.Viewport.Height;

        Root.ComputeDimensions();
        Root.ComputePositions(0, 0);

        // TODO
        // 1. Update UI
        // 2. Calculate sizes
        // 3. Calculate position
    }
}