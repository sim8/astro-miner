namespace AstroMiner.UI;

public class UIMap : UIElement
{
    private const int MAP_SCREEN_BASE_X_PADDING = 60;
    private const int MAP_SCREEN_BASE_Y_PADDING = 100;
    private readonly BaseGame _game;

    public UIMap(BaseGame game) : base(game)
    {
        _game = game;
        Children =
        [
            // Wrapper
            new UITextElement(game)
            {
                FixedWidth = game.Graphics.GraphicsDevice.Viewport.Width -
                             MAP_SCREEN_BASE_X_PADDING * game.StateManager.Ui.UIScale,
                FixedHeight = game.Graphics.GraphicsDevice.Viewport.Height -
                              MAP_SCREEN_BASE_Y_PADDING * game.StateManager.Ui.UIScale,
                BackgroundColor = Colors.VeryDarkBlue,
                Text = "MAP GOES HERE"
            }
        ];
    }
}