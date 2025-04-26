using System.Collections.Generic;
using AstroMiner.Definitions;

namespace AstroMiner;

public class InventoryItem
{
    public ResourceType Type { get; set; }
    public int Count { get; set; }
}

public class Inventory
{
    public readonly List<InventoryItem?> resources = new();
    public int selectedIndex { get; set; } = 0;

    public Inventory()
    {
        AddResource(ResourceType.Diamond, 2);
        AddResource(ResourceType.Ruby);
        AddResource(ResourceType.Nickel, 4);
    }

    public int numDynamite { get; set; } = 3; // TODO make these a resource?

    public void AddResource(ResourceType type, int count = 1)
    {
        var existing = resources.Find(r => r != null && r.Type == type);
        if (existing != null)
            existing.Count += count;
        else
            resources.Add(new InventoryItem { Type = type, Count = count });
    }
}