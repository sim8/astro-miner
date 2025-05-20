using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UILaunchConsole : UIScreen
{
    public UILaunchConsole(BaseGame game) : base(game)
    {
        Children =
        [
            new UIElement(game)
            {
                BackgroundColor = Colors.LightBlue,
                Padding = 20,
                Children =
                [
                    new UITextElement(game)
                    {
                        Text = "LAUNCH CONSOLE",
                        Scale = 4
                    },
                    new UIElement(game)
                    {
                        FixedHeight = 30
                    },
                    new UIElement(game)
                    {
                        ChildrenDirection = ChildrenDirection.Row,
                        Children =
                        [
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
                                FixedWidth = 10
                            },
                            new UITextElement(game)
                            {
                                Text = "CANCEL",
                                Color = Color.White,
                                BackgroundColor = Color.Navy,
                                Padding = 6,
                                Scale = 3,
                                OnClick = () => { game.StateManager.Ui.State.IsLaunchConsoleOpen = false; }
                            }
                        ]
                    }
                ]
            }
        ];
    }
}