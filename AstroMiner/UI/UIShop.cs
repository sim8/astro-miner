using AstroMiner.Definitions;
using AstroMiner.Model;
using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public sealed class UIShop : UIElement
{
    public UIShop(BaseGame game) : base(game)
    {
        Position = PositionMode.Absolute;
        FullWidth = true;
        FullHeight = true;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenDirection = ChildrenDirection.Column;
        ChildrenJustify = ChildrenJustify.Center;

        Children = game.StateManager.Ui.State.sellConfirmationItemIndex == -1
            ?
            [
                new UIScreen(game)
                {
                    Children =
                    [
                        new UITextElement(game)
                        {
                            Scale = 3,
                            Text = "SELL ITEMS"
                        },
                        new UIInventoryGrid(game,
                            selectedIndex => game.StateManager.Ui.State.sellConfirmationItemIndex = selectedIndex)
                    ]
                }
            ]
            : [new UISaleConfirm(game)];
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
                        Text = "SELL " + GetSellConfirmationItem(game).Count + " " +
                               GetSellConfirmationItem(game).Type.ToString().ToUpper() + " FOR " +
                               ItemTypes.GetConfig(GetSellConfirmationItem(game).Type).Price *
                               GetSellConfirmationItem(game).Count + " CREDITS",
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
                                OnClick = () =>
                                {
                                    game.StateManager.Inventory.SellItem(game.StateManager.Ui.State
                                        .sellConfirmationItemIndex);
                                    game.StateManager.Ui.State.sellConfirmationItemIndex = -1;
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
                                OnClick = () => { game.StateManager.Ui.State.sellConfirmationItemIndex = -1; }
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private InventoryItem GetSellConfirmationItem(BaseGame game)
    {
        return game.StateManager.Inventory.GetItemAtIndex(game.StateManager.Ui.State.sellConfirmationItemIndex);
    }
}