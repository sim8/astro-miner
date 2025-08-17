using AstroMiner.Definitions;
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
                BackgroundColor = Colors.VeryDarkBlue,
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
                                BackgroundColor = Colors.DarkBlue,
                                Padding = 6,
                                Scale = 3,
                                OnClick = () =>
                                {
                                    game.StateManager.Ui.State.ActiveScreen = null;
                                    game.StateManager.TransitionManager.FadeOut(1, () =>
                                    {
                                        game.StateManager.SetActiveWorldAndInitialize(World.Asteroid);
                                        game.StateManager.Ecs.EntityTransformSystem.MoveMinerAndPlayerToAsteroid();
                                        game.StateManager.TransitionManager.FadeIn();
                                    });
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
                                BackgroundColor = Colors.DarkBlue,
                                Padding = 6,
                                Scale = 3,
                                OnClick = () => { game.StateManager.Ui.State.ActiveScreen = null; }
                            }
                        ]
                    }
                ]
            }
        ];
    }
}