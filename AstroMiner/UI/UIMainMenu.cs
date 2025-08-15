using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIMainMenu : UIScreen
{
    public UIMainMenu(BaseGame game) : base(game)
    {
        ChildrenDirection = ChildrenDirection.Column;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Center;
        BackgroundColor = Colors.VeryDarkBlue;
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
                FixedHeight = 20
            },
            new UITextElement(game)
            {
                Text = "NEW GAME",
                Color = Color.White,
                BackgroundColor = Colors.DarkBlue,
                Padding = 6,
                Scale = 3,
                OnClick = () =>
                {
                    game.StateManager.NewGame();
                }
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
                OnClick = () =>
                {
                    game.StateManager.LoadGame();
                }
            }
        ];
    }
}