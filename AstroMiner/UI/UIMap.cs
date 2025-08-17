namespace AstroMiner.UI;

public class UIMap : UIElement
{
    private readonly BaseGame _game;

    public UIMap(BaseGame game) : base(game)
    {
        _game = game;
        Children =
        [
            // Wrapper
            new UITextElement(game)
            {
                Text = "MAP GOES HERE"
            }
        ];
    }
}