using System;
using AstroMiner.Definitions;
using AstroMiner.Model;

namespace AstroMiner;

public class Inventory(BaseGame game)
{
    public ItemType? SelectedItemType => game.Model.Inventory.SelectedIndex < game.Model.Inventory.Items.Count
            ? game.Model.Inventory.Items[game.Model.Inventory.SelectedIndex]?.Type
            : null;

    public void AddItem(ItemType type, int count = 1)
    {
        var existing = game.Model.Inventory.Items.Find(r => r != null && r.Type == type);
        if (existing != null)
            existing.Count += count;
        else
            game.Model.Inventory.Items.Add(new InventoryItem { Type = type, Count = count });
    }

    public InventoryItem GetItemAtIndex(int index)
    {
        return game.Model.Inventory.Items[index];
    }

    public void SellItem(int index)
    {
        var inventoryItem = game.Model.Inventory.Items[index];
        game.Model.Inventory.Credits += ItemTypes.GetConfig(inventoryItem.Type).SalePrice * inventoryItem.Count;
        game.Model.Inventory.Items.Remove(inventoryItem);
    }

    public bool BuyItem(ItemType itemType, int count = 1)
    {
        var itemConfig = ItemTypes.GetConfig(itemType);
        var totalCost = itemConfig.BuyPrice * count;

        // Check if item is purchasable and player has enough credits
        if (itemConfig.BuyPrice == -1 || game.Model.Inventory.Credits < totalCost)
            return false;

        // Deduct credits and add item
        game.Model.Inventory.Credits -= totalCost;
        AddItem(itemType, count);
        return true;
    }

    public void ConsumeSelectedItem()
    {
        var item = game.Model.Inventory.Items[game.Model.Inventory.SelectedIndex];
        if (item != null)
        {
            item.Count--;
            if (item.Count == 0)
                game.Model.Inventory.Items.Remove(item);
        }
        else
        {
            throw new Exception("No item selected");
        }
    }
}