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

        // Add example UI elements
        Root.Children.Add(new UIElement(game)
        {
            BackgroundColor = Color.LightGray,
            FixedWidth = 200,
            FixedHeight = 100,
            X = 50,
            Y = 50
        });

        Root.Children.Add(new UIElement(game)
        {
            BackgroundColor = Color.DarkBlue,
            FixedWidth = 150,
            FixedHeight = 75,
            X = 300,
            Y = 150
        });
    }

    public void Update(GameTime gameTime)
    {
        Root.FixedWidth = game.Graphics.GraphicsDevice.Viewport.Width;
        Root.FixedHeight = game.Graphics.GraphicsDevice.Viewport.Height;
    }
}