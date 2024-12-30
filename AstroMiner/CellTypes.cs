using System;
using System.Collections.Generic;

namespace AstroMiner;

public enum CellType
{
    Empty,
    Floor,
    Lava,
    Rock,
    SolidRock,
    Diamond,
    Ruby,
    Gold,
    Nickel
}

public abstract class CellTypeConfig(bool isDestructible, bool isCollideable)
{
    public bool IsDestructible { get; } = isDestructible;
    public bool IsCollideable { get; } = isCollideable;

    public abstract bool IsMineable { get; }
}

public class MineableCellConfig(int drillTimeMs, ResourceType? drop = null) : CellTypeConfig(true, true)
{
    public int DrillTimeMs { get; } = drillTimeMs;
    public ResourceType? Drop { get; } = drop;

    public override bool IsMineable => true;
}

public class NonMineableCellConfig(bool isDestructible, bool isCollideable)
    : CellTypeConfig(isDestructible, isCollideable)
{
    public override bool IsMineable => false;
}

public static class CellTypes
{
    private static readonly int DefaultDrillTime = 600;

    private static readonly IReadOnlyDictionary<CellType, CellTypeConfig> AllCellTypeConfig =
        new Dictionary<CellType, CellTypeConfig>
        {
            { CellType.Empty, new NonMineableCellConfig(false, false) },
            { CellType.Floor, new NonMineableCellConfig(false, false) },
            { CellType.Lava, new NonMineableCellConfig(false, false) },
            { CellType.SolidRock, new NonMineableCellConfig(false, true) },
            { CellType.Rock, new MineableCellConfig(DefaultDrillTime) },
            { CellType.Diamond, new MineableCellConfig(1200, ResourceType.Diamond) },
            { CellType.Ruby, new MineableCellConfig(800, ResourceType.Ruby) },
            { CellType.Gold, new MineableCellConfig(800, ResourceType.Gold) },
            { CellType.Nickel, new MineableCellConfig(DefaultDrillTime, ResourceType.Nickel) }
        };

    public static CellTypeConfig GetConfig(CellType cellType)
    {
        if (!AllCellTypeConfig.TryGetValue(cellType, out var config))
            throw new ArgumentException($"No configuration found for CellType: {cellType}");

        return config;
    }
}