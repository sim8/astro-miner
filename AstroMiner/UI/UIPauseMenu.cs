using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIPauseMenu : UIScreen
{
    public UIPauseMenu(BaseGame game) : base(game)
    {
        ChildrenDirection = ChildrenDirection.Column;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Center;
        BackgroundColor = Colors.VeryDarkBlue;
        Children =
        [
            new UITextElement(game)
            {
                Text = "PAUSED",
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
                Text = "CONTINUE",
                Color = Color.White,
                BackgroundColor = Colors.DarkBlue,
                Padding = 6,
                Scale = 3,
                OnClick = () => { game.StateManager.Ui.State.IsInPauseMenu = false; }
            },
            new UIElement(game)
            {
                FixedHeight = 10
            },
            new UITextElement(game)
            {
                Text = "QUICK SAVE",
                Color = Color.White,
                BackgroundColor = Colors.DarkBlue,
                Padding = 6,
                Scale = 3,
                OnClick = () => { game.StateManager.SaveGame(); }
            },
            new UIElement(game)
            {
                FixedHeight = 10
            },
            new UITextElement(game)
            {
                Text = "EXIT TO MAIN MENU",
                Color = Color.White,
                BackgroundColor = Colors.DarkBlue,
                Padding = 6,
                Scale = 3,
                OnClick = () =>
                {
                    game.StateManager.Ui.State.IsInMainMenu = true;
                    game.StateManager.Ui.State.IsInPauseMenu = false;
                }
            }
        ];
    }
}