using System;
using System.Collections.Generic;

namespace AstroMiner.Definitions;

public enum WallType
{
    Empty,
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
    Empty,
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

public class WallTypeConfig(bool isMineable, ItemType? drop = null, int drillTimeMs = 300) // TODO make const
{
    public bool IsMineable { get; } = isMineable;
    public int DrillTimeMs { get; } = drillTimeMs;
    public ItemType? Drop { get; } = drop;
}

public static class WallTypes
{
    private static readonly IReadOnlyDictionary<WallType, WallTypeConfig> WallTypeConfigs =
        new Dictionary<WallType, WallTypeConfig>
        {
            { WallType.SolidRock, new WallTypeConfig(false) },
            { WallType.LooseRock, new WallTypeConfig(true, null, 1) },
            { WallType.Rock, new WallTypeConfig(true) },
            { WallType.Diamond, new WallTypeConfig(true, ItemType.Diamond, 1200) },
            { WallType.Ruby, new WallTypeConfig(true, ItemType.Ruby, 800) },
            { WallType.Gold, new WallTypeConfig(true, ItemType.Gold, 800) },
            { WallType.Nickel, new WallTypeConfig(true, ItemType.Nickel) },
            { WallType.ExplosiveRock, new WallTypeConfig(true) }
        };

    public static WallTypeConfig GetConfig(WallType wallType)
    {
        if (!WallTypeConfigs.TryGetValue(wallType, out var config))
            throw new ArgumentException($"No configuration found for WallType: {wallType}");

        return config;
    }
}

public static class FloorTypes
{
    // TODO move into config?
    public static bool IsFloorLikeTileset(FloorType floorType)
    {
        return floorType == FloorType.Floor || floorType == FloorType.LavaCracks;
    }
}