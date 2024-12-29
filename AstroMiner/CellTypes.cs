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

public class CellTypeConfig(bool isMineable, bool isCollideable)
{
    public readonly bool IsCollideable = isCollideable;
    public readonly bool IsMineable = isMineable;
}

public static class CellTypes
{
    private static readonly Dictionary<CellType, CellTypeConfig> AllCellTypeConfig =
        new()
        {
            { CellType.Empty, new CellTypeConfig(false, false) },
            { CellType.Floor, new CellTypeConfig(false, false) },
            { CellType.Lava, new CellTypeConfig(false, false) },
            { CellType.Rock, new CellTypeConfig(true, true) },
            { CellType.SolidRock, new CellTypeConfig(false, true) },
            { CellType.Diamond, new CellTypeConfig(true, true) },
            { CellType.Ruby, new CellTypeConfig(true, true) },
            { CellType.Gold, new CellTypeConfig(true, true) },
            { CellType.Nickel, new CellTypeConfig(true, true) }
        };

    public static CellTypeConfig GetConfig(CellType cellType)
    {
        return AllCellTypeConfig[cellType];
    }
}