using System;
using System.Collections.Generic;

namespace AstroMiner.Definitions;

public enum ResourceType
{
    Ruby,
    Diamond,
    Gold,
    Nickel
}

public class ResourceTypeConfig(string name)
{
    public string Name { get; } = name;
}

public static class ResourceTypes
{
    private static readonly IReadOnlyDictionary<ResourceType, ResourceTypeConfig> AllResourceTypeConfig =
        new Dictionary<ResourceType, ResourceTypeConfig>
        {
            { ResourceType.Ruby, new ResourceTypeConfig("Ruby") },
            { ResourceType.Diamond, new ResourceTypeConfig("Diamond") },
            { ResourceType.Gold, new ResourceTypeConfig("Gold") },
            { ResourceType.Nickel, new ResourceTypeConfig("Nickel") }
        };

    public static ResourceTypeConfig GetConfig(ResourceType resourceType)
    {
        if (!AllResourceTypeConfig.TryGetValue(resourceType, out var config))
            throw new ArgumentException($"No configuration found for ResourceType: {resourceType}");

        return config;
    }
}