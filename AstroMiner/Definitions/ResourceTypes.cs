using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner.Definitions;

public enum ResourceType
{
    Ruby,
    Diamond,
    Gold,
    Nickel
}

public class ResourceTypeConfig(string name, int colIndex, int rowIndex)
{
    private readonly int colIndex = colIndex;
    private readonly int rowIndex = rowIndex;
    public string Name { get; } = name;

    public Rectangle GetSourceRect()
    {
        return new Rectangle(colIndex * 16, rowIndex * 16, 16, 16);
    }
}

public static class ResourceTypes
{
    private static readonly IReadOnlyDictionary<ResourceType, ResourceTypeConfig> AllResourceTypeConfig =
        new Dictionary<ResourceType, ResourceTypeConfig>
        {
            { ResourceType.Ruby, new ResourceTypeConfig("Ruby", 0, 0) },
            { ResourceType.Diamond, new ResourceTypeConfig("Diamond", 1, 0) },
            { ResourceType.Gold, new ResourceTypeConfig("Gold", 2, 0) },
            { ResourceType.Nickel, new ResourceTypeConfig("Nickel", 3, 0) }
        };

    public static ResourceTypeConfig GetConfig(ResourceType resourceType)
    {
        if (!AllResourceTypeConfig.TryGetValue(resourceType, out var config))
            throw new ArgumentException($"No configuration found for ResourceType: {resourceType}");

        return config;
    }
}