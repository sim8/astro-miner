#nullable enable
using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIInventory : UIScreen
{
    public UIInventory(BaseGame game,
        Action<int>? onItemClick = null) : base(game)
    {
        Children =
        [
            new UIInventoryGrid(game, onItemClick)
        ];
    }
}

public class UIInventoryGrid : UIElement
{
    public UIInventoryGrid(BaseGame game,
        Action<int>? onItemClick = null) : base(game)
    {
        Children =
        [
            new UIInventoryRow(game, 0, 10, onItemClick),
            new UIInventoryRow(game, 10, 20, onItemClick),
            new UIInventoryRow(game, 20, 30, onItemClick)
        ];
    }
}

public sealed class UIInventoryFooter : UIElement
{
    public UIInventoryFooter(BaseGame game) : base(game)
    {
        Children =
        [
            new UIInventoryRow(game, 0, 10, null)
        ];
    }
}

public sealed class UIInventoryRow : UIElement
{
    public UIInventoryRow(BaseGame game, int inventoryStartIndex, int inventoryEndIndex, Action<int>? onItemClick) :
        base(game)
    {
        ChildrenDirection = ChildrenDirection.Row;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Start;
        Children = CreateInventoryItems(game, inventoryStartIndex, inventoryEndIndex, onItemClick);
    }

    private List<UIElement> CreateInventoryItems(BaseGame game, int startIndex, int endIndex, Action<int>? onItemClick)
    {
        var items = new List<UIElement>();
        var inventory = game.Model.Inventory.Items;

        for (var i = startIndex; i < endIndex; i++)
            if (i < inventory.Count && inventory[i] != null)
            {
                var item = inventory[i];
                var itemConfig = ItemTypes.GetConfig(item.Type);
                items.Add(new UIInventoryItem(game, itemConfig.GetSourceRect(), item.Count, i, onItemClick));
            }
            else
            {
                items.Add(new UIInventoryItemEmpty(game));
            }

        return items;
    }
}

public sealed class UIInventoryItemEmpty : UIElement
{
    public UIInventoryItemEmpty(BaseGame game) : base(game)
    {
        FixedWidth = 32 * game.StateManager.Ui.UIScale;
        FixedHeight = 32 * game.StateManager.Ui.UIScale;
        BackgroundColor = Colors.DarkBlue;
    }
}

public sealed class UIInventoryItem : UIElement
{
    public UIInventoryItem(BaseGame game, Rectangle sourceRect, int count, int inventoryIndex,
        Action<int>? onItemClick) : base(game)
    {
        FixedWidth = 32 * game.StateManager.Ui.UIScale;
        FixedHeight = 32 * game.StateManager.Ui.UIScale;
        BackgroundColor = Colors.DarkBlue;
        OnClick = () =>
        {
            if (onItemClick != null)
                onItemClick(inventoryIndex);
            else
                game.Model.Inventory.SelectedIndex = inventoryIndex;
        };
        Children =
            UIHelpers.FilterNull([
                inventoryIndex == game.Model.Inventory.SelectedIndex
                    ? new UIElement(game)
                    {
                        Position = PositionMode.Absolute,
                        FullHeight = true,
                        FullWidth = true,
                        BackgroundColor = Colors.LightBlue
                    }
                    : null,
                new UIImageElement(game, Tx.Icons, sourceRect)
                {
                    Padding = 2 * game.StateManager.Ui.UIScale,
                    FixedWidth = 32 * game.StateManager.Ui.UIScale,
                    FixedHeight = 32 * game.StateManager.Ui.UIScale
                },
                count > 0
                    ? new UITextElement(game)
                    {
                        Position = PositionMode.Absolute,
                        Text = count.ToString(),
                        Scale = 1 * game.StateManager.Ui.UIScale,
                        Padding = 2 * game.StateManager.Ui.UIScale
                    }
                    : null
            ]);
    }
}