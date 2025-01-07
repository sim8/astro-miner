using System.Collections.Generic;
using AstroMiner.Definitions;

namespace AstroMiner;

public class Inventory
{
    public readonly List<(ResourceType Type, int Count)> resources = new();
    public int numDynamite { get; set; } = 3; // TODO make these a resource?

    public void AddResource(ResourceType type, int count = 1)
    {
        var existing = resources.FindIndex(r => r.Type == type);
        if (existing >= 0)
        {
            var current = resources[existing];
            resources[existing] = (current.Type, current.Count + count);
        }
        else
        {
            resources.Add((type, count));
        }
    }
}