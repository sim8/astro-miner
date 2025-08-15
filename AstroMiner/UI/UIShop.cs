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
    private InventoryItem GetSellConfirmationItem(BaseGame game)
    {
        return game.StateManager.Inventory.GetItemAtIndex(game.StateManager.Ui.State.sellConfirmationItemIndex);
    }
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
                        Text = "SELL " + GetSellConfirmationItem(game).Count + " " + GetSellConfirmationItem(game).Type.ToString().ToUpper() + " FOR " + ItemTypes.GetConfig(GetSellConfirmationItem(game).Type).Price * GetSellConfirmationItem(game).Count + " CREDITS",
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
                                OnClick = () => {
                                    game.StateManager.Inventory.SellItem(game.StateManager.Ui.State.sellConfirmationItemIndex);
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
}