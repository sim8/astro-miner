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

public class CellTypeConfig(bool isMineable, bool isCollideable, int drillTime = -1) // TODO extend this for mineable?
{
    public readonly int DrillTime = drillTime;
    public readonly bool IsCollideable = isCollideable;
    public readonly bool IsMineable = isMineable;
}

public static class CellTypes
{
    public static readonly int DefaultDrillTime = 600;

    private static readonly Dictionary<CellType, CellTypeConfig> AllCellTypeConfig =
        new()
        {
            { CellType.Empty, new CellTypeConfig(false, false) },
            { CellType.Floor, new CellTypeConfig(false, false) },
            { CellType.Lava, new CellTypeConfig(false, false) },
            { CellType.Rock, new CellTypeConfig(true, true, DefaultDrillTime) },
            { CellType.SolidRock, new CellTypeConfig(false, true) },
            { CellType.Diamond, new CellTypeConfig(true, true, 1200) },
            { CellType.Ruby, new CellTypeConfig(true, true, 800) },
            { CellType.Gold, new CellTypeConfig(true, true, 800) },
            { CellType.Nickel, new CellTypeConfig(true, true, DefaultDrillTime) }
        };

    public static CellTypeConfig GetConfig(CellType cellType)
    {
        return AllCellTypeConfig[cellType];
    }
}