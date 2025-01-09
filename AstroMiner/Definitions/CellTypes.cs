using System;
using System.Collections.Generic;

namespace AstroMiner.Definitions;

public enum WallType
{
    Rock,
    SolidRock,
    LooseRock,
    Diamond,
    Ruby,
    Gold,
    Nickel,
    ExplosiveRock
}

public enum FloorType
{
    Floor,
    Lava,
    LavaCracks
}

public enum AsteroidLayer
{
    None,
    Crust,
    Mantle,
    Core
}

public class WallTypeConfig(bool isMineable, ResourceType? drop = null, int drillTimeMs = 300) // TODO make const
{
    public bool IsMineable { get; } = isMineable;
    public int DrillTimeMs { get; } = drillTimeMs;
    public ResourceType? Drop { get; } = drop;
}

public static class WallTypes
{
    private static readonly IReadOnlyDictionary<WallType, WallTypeConfig> WallTypeConfigs =
        new Dictionary<WallType, WallTypeConfig>
        {
            { WallType.SolidRock, new WallTypeConfig(false) },
            { WallType.LooseRock, new WallTypeConfig(true, null, 1) },
            { WallType.Rock, new WallTypeConfig(true) },
            { WallType.Diamond, new WallTypeConfig(true, ResourceType.Diamond, 1200) },
            { WallType.Ruby, new WallTypeConfig(true, ResourceType.Ruby, 800) },
            { WallType.Gold, new WallTypeConfig(true, ResourceType.Gold, 800) },
            { WallType.Nickel, new WallTypeConfig(true, ResourceType.Nickel) },
            { WallType.ExplosiveRock, new WallTypeConfig(true) }
        };

    public static WallTypeConfig GetConfig(WallType wallType)
    {
        if (!WallTypeConfigs.TryGetValue(wallType, out var config))
            throw new ArgumentException($"No configuration found for WallType: {wallType}");

        return config;
    }
}