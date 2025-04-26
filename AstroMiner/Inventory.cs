using System.Collections.Generic;
using AstroMiner.Definitions;

namespace AstroMiner;

public class InventoryItem
{
    public ItemType Type { get; set; }
    public int Count { get; set; }
}

public class Inventory
{
    public readonly List<InventoryItem?> items = new();

    public Inventory()
    {
        AddItem(ItemType.Diamond, 2);
        AddItem(ItemType.Ruby);
        AddItem(ItemType.Nickel, 4);
    }

    public int selectedIndex { get; set; } = 0;

    public int numDynamite { get; set; } = 3; // TODO make these an item?

    public void AddItem(ItemType type, int count = 1)
    {
        var existing = items.Find(r => r != null && r.Type == type);
        if (existing != null)
            existing.Count += count;
        else
            items.Add(new InventoryItem { Type = type, Count = count });
    }
}