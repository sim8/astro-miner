using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UILaunchConsole : UIScreen
{
    public UILaunchConsole(BaseGame game) : base(game)
    {
        ChildrenDirection = ChildrenDirection.Column;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Center;
        BackgroundColor = Color.Black;
        Children =
        [
            new UITextElement(game)
            {
                Text = "LAUNCH CONSOLE",
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
                Text = "LAUNCH",
                Color = Color.White,
                BackgroundColor = Color.Navy,
                Padding = 6,
                Scale = 3,
                OnClick = () =>
                {
                    // game.StateManager.NewGame();
                }
            },
            new UIElement(game)
            {
                FixedHeight = 10
            },
            new UITextElement(game)
            {
                Text = "CANCEL",
                Color = Color.White,
                BackgroundColor = Color.Navy,
                Padding = 6,
                Scale = 3,
                OnClick = () =>
                {
                    game.StateManager.Ui.State.IsLaunchConsoleOpen = false;
                }
            }
        ];
    }
}