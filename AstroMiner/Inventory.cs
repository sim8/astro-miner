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
    public readonly List<InventoryItem?> resources = new();

    public Inventory()
    {
        AddResource(ItemType.Diamond, 2);
        AddResource(ItemType.Ruby);
        AddResource(ItemType.Nickel, 4);
    }

    public int selectedIndex { get; set; } = 0;

    public int numDynamite { get; set; } = 3; // TODO make these a resource?

    public void AddResource(ItemType type, int count = 1)
    {
        var existing = resources.Find(r => r != null && r.Type == type);
        if (existing != null)
            existing.Count += count;
        else
            resources.Add(new InventoryItem { Type = type, Count = count });
    }
}