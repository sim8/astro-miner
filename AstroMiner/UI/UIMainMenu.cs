using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public class UIMainMenu : UIScreen
{
    private readonly BaseGame _game;

    public UIMainMenu(BaseGame game) : base(game)
    {
        _game = game;
        ChildrenDirection = ChildrenDirection.Column;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Center;
        BackgroundColor = Color.Transparent;
        Children =
        [
            new UITextElement(game)
            {
                Text = "GEM CARAVAN",
                Color = Color.White,
                Padding = 6,
                Scale = 4
            },
            new UIElement(game)
            {
                FixedHeight = 40
            },
            new UITextElement(game)
            {
                Text = "NEW GAME",
                Color = Color.White,
                BackgroundColor = Colors.DarkBlue,
                Padding = 6,
                Scale = 3,
                OnClick = () => { game.StateManager.NewGame(); }
            },
            new UIElement(game)
            {
                FixedHeight = 10
            },
            new UITextElement(game)
            {
                Text = "LOAD GAME",
                Color = Color.White,
                BackgroundColor = Colors.DarkBlue,
                Padding = 6,
                Scale = 3,
                OnClick = () => { game.StateManager.LoadGame(); }
            }
        ];
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        var screenDestRect = new Rectangle(X, Y, ComputedWidth, ComputedHeight);
        spriteBatch.Draw(_game.Textures["white"], screenDestRect, Colors.VeryDarkBlue);

        _game.StateManager.Ui.State.StarBackground.Render(spriteBatch, _game.Textures);

        base.Render(spriteBatch);
    }
}