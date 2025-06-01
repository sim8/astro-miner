using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public sealed class UIShop : UIElement
{
    public UIShop(BaseGame game) : base(game)
    {
        Position = PositionMode.Absolute;
        FullWidth = true;
        FullHeight = true;
        Children =
        [
            game.StateManager.Ui.State.sellConfirmationItemIndex == -1
                ? new UIInventory(game,
                    selectedIndex => game.StateManager.Ui.State.sellConfirmationItemIndex = selectedIndex)
                : new UISaleConfirm(game)
        ];
    }
}

public class UISaleConfirm : UIScreen
{
    public UISaleConfirm(BaseGame game) : base(game)
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
                        Text = "SELL ITEM",
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
                                Text = "SELL",
                                Color = Color.White,
                                BackgroundColor = Color.Navy,
                                Padding = 6,
                                Scale = 3,
                                OnClick = () => { }
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
                                OnClick = () => { game.StateManager.Ui.State.sellConfirmationItemIndex = -1; }
                            }
                        ]
                    }
                ]
            }
        ];
    }
}