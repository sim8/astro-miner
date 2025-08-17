using System;
using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIInGameMenu : UIScreen
{
    private readonly BaseGame _game;

    public UIInGameMenu(BaseGame game) : base(game)
    {
        _game = game;
        Children =
        [
            // Wrapper
            new UIElement(game)
            {
                Padding = 10,
                ChildrenDirection = ChildrenDirection.Column,
                ChildrenAlign = ChildrenAlign.Stretch,
                Children =
                [
                    // Header
                    new UIElement(game)
                    {
                        ChildrenDirection = ChildrenDirection.Row,
                        ChildrenJustify = ChildrenJustify.Start,
                        Children =
                        [
                            new UITextElement(game)
                            {
                                Text = "INVENTORY",
                                Color = Color.White,
                                BackgroundColor = Colors.DarkBlue,
                                Padding = 6,
                                Scale = 3,
                                OnClick = () =>
                                {
                                    game.StateManager.Ui.State.ActiveInGameMenuSubScreen =
                                        InGameMenuSubScreen.Inventory;
                                }
                            },
                            new UIElement(game)
                            {
                                FixedWidth = 10
                            },
                            new UITextElement(game)
                            {
                                Text = "MAP",
                                Color = Color.White,
                                BackgroundColor = Colors.DarkBlue,
                                Padding = 6,
                                Scale = 3,
                                OnClick = () =>
                                {
                                    game.StateManager.Ui.State.ActiveInGameMenuSubScreen = InGameMenuSubScreen.Map;
                                }
                            }
                        ]
                    },
                    // Content
                    GetActiveSubScreen()
                ]
            }
        ];
    }

    private UIElement? GetActiveSubScreen()
    {
        return _game.StateManager.Ui.State.ActiveInGameMenuSubScreen switch
        {
            InGameMenuSubScreen.Inventory => new UIInventoryGrid(_game),
            InGameMenuSubScreen.Map => new UIMap(_game),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}