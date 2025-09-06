using System.Collections.Generic;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public sealed class UIMerchant : UIElement
{
    public UIMerchant(BaseGame game) : base(game)
    {
        Position = PositionMode.Absolute;
        FullWidth = true;
        FullHeight = true;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenDirection = ChildrenDirection.Column;
        ChildrenJustify = ChildrenJustify.Center;

        Children = game.StateManager.Ui.State.PurchaseConfirmationItemType == null
            ?
            [
                new UIScreen(game)
                {
                    Children =
                    [
                        new UITextElement(game)
                        {
                            Scale = 3,
                            Text = GetMerchantName(game)
                        },
                        new UIMerchantItemsList(game)
                    ]
                }
            ]
            : [new UIConfirmPurchase(game)];
    }

    private string GetMerchantName(BaseGame game)
    {
        if (game.StateManager.Ui.State.ActiveMerchantType == null)
            return "MERCHANT";

        var merchantConfig = Merchants.GetConfig(game.StateManager.Ui.State.ActiveMerchantType.Value);
        return merchantConfig.Name.ToUpper();
    }
}

public class UIMerchantItemsList : UIElement
{
    public UIMerchantItemsList(BaseGame game) : base(game)
    {
        ChildrenDirection = ChildrenDirection.Column;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Start;

        if (game.StateManager.Ui.State.ActiveMerchantType == null)
        {
            Children = [
                new UITextElement(game)
                {
                    Text = "NO MERCHANT ACTIVE",
                    Scale = 2
                }
            ];
            return;
        }

        var merchantConfig = Merchants.GetConfig(game.StateManager.Ui.State.ActiveMerchantType.Value);
        var merchantItems = new List<UIElement>();

        foreach (var itemType in merchantConfig.Items)
        {
            var itemConfig = ItemTypes.GetConfig(itemType);
            if (itemConfig.BuyPrice != -1) // Only show items that can be purchased
            {
                merchantItems.Add(new UIMerchantItemRow(game, itemType));
            }
        }

        if (merchantItems.Count == 0)
        {
            merchantItems.Add(new UITextElement(game)
            {
                Text = "NO ITEMS FOR SALE",
                Scale = 2
            });
        }

        Children = merchantItems;
    }
}

public class UIMerchantItemRow : UIElement
{
    public UIMerchantItemRow(BaseGame game, ItemType itemType) : base(game)
    {
        var itemConfig = ItemTypes.GetConfig(itemType);

        ChildrenDirection = ChildrenDirection.Row;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Start;
        BackgroundColor = Colors.DarkBlue;
        Padding = 10;

        OnClick = () =>
        {
            game.StateManager.Ui.State.PurchaseConfirmationItemType = itemType;
        };

        Children =
        [
            // Item image
            new UIImageElement(game, Tx.Icons, itemConfig.GetSourceRect())
            {
                FixedWidth = 32 * game.StateManager.Ui.UIScale,
                FixedHeight = 32 * game.StateManager.Ui.UIScale
            },
            // Spacing
            new UIElement(game)
            {
                FixedWidth = 20
            },
            // Item name
            new UITextElement(game)
            {
                Text = itemConfig.Name.ToUpper(),
                Scale = 1 * game.StateManager.Ui.UIScale,
                FixedWidth = 200
            },
            // Price
            new UITextElement(game)
            {
                Text = itemConfig.BuyPrice + " CREDITS",
                Scale = 1 * game.StateManager.Ui.UIScale,
                Color = Color.Yellow
            }
        ];
    }
}

public class UIConfirmPurchase : UIScreen
{
    public UIConfirmPurchase(BaseGame game) : base(game)
    {
        var itemType = game.StateManager.Ui.State.PurchaseConfirmationItemType!.Value;
        var itemConfig = ItemTypes.GetConfig(itemType);
        var hasEnoughCredits = game.Model.Inventory.Credits >= itemConfig.BuyPrice;

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
                        Text = "BUY " + itemConfig.Name.ToUpper() + " FOR " + itemConfig.BuyPrice + " CREDITS",
                        Scale = 4
                    },
                    new UIElement(game)
                    {
                        FixedHeight = 10
                    },
                    new UITextElement(game)
                    {
                        Text = "YOU HAVE " + game.Model.Inventory.Credits + " CREDITS",
                        Scale = 2,
                        Color = hasEnoughCredits ? Color.Green : Color.Red
                    },
                    new UIElement(game)
                    {
                        FixedHeight = 20
                    },
                    new UIElement(game)
                    {
                        ChildrenDirection = ChildrenDirection.Row,
                        Children =
                        [
                            new UITextElement(game)
                            {
                                Text = "BUY",
                                Color = hasEnoughCredits ? Color.White : Color.Gray,
                                BackgroundColor = hasEnoughCredits ? Color.Navy : Color.DarkGray,
                                Padding = 6,
                                Scale = 3,
                                OnClick = hasEnoughCredits ? () =>
                                {
                                    if (game.StateManager.Inventory.BuyItem(itemType))
                                    {
                                        game.StateManager.Ui.State.PurchaseConfirmationItemType = null;
                                    }
                                } : null
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
                                OnClick = () => { game.StateManager.Ui.State.PurchaseConfirmationItemType = null; }
                            }
                        ]
                    }
                ]
            }
        ];
    }
}