using AstroMiner.Definitions;
using AstroMiner.Model;

namespace AstroMiner;

public class Inventory(BaseGame game)
{
    public ItemType? SelectedItemType => game.Model.Inventory.Items.Count > 0
        ? game.Model.Inventory.Items[game.Model.Inventory.SelectedIndex]?.Type
        : null;

    public int numDynamite { get; set; } = 3; // TODO make these an item?

    public void AddItem(ItemType type, int count = 1)
    {
        var existing = game.Model.Inventory.Items.Find(r => r != null && r.Type == type);
        if (existing != null)
            existing.Count += count;
        else
            game.Model.Inventory.Items.Add(new InventoryItem { Type = type, Count = count });
    }
}