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
    Quartz,
    ExplosiveRock
}

public enum FloorType
{
    Empty,
    Floor,
    Lava,
    LavaCracks,
    CollapsingLavaCracks
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
            { WallType.Quartz, new WallTypeConfig(true, ItemType.Quartz) },
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

    public static bool IsLavaLike(FloorType floorType)
    {
        return floorType is FloorType.Lava or FloorType.CollapsingLavaCracks;
    }
}