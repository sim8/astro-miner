using AstroMiner.Definitions;

namespace AstroMiner.StaticWorld;

public class StaticWorldState(BaseGame game) : BaseWorldState(game)
{
    public WorldCellType[,] Grid;

    public override void Initialize()
    {
        base.Initialize();
        Grid = StaticWorlds.StaticWorldConfigs[game.Model.ActiveWorld].World;
    }

    public override bool CellIsCollideable(int x, int y)
    {
        // TODO centralize out of bounds checks
        if (x < 0 || x >= Grid.GetLength(1) || y < 0 ||
            y >= Grid.GetLength(0)) return false;
        return Grid[y, x] == WorldCellType.Filled;
    }

    public override bool CellIsPortal(int x, int y)
    {
        // TODO centralize out of bounds checks
        if (x < 0 || x >= Grid.GetLength(1) || y < 0 ||
            y >= Grid.GetLength(0)) return false;
        return Grid[y, x] == WorldCellType.Portal;
    }

    public override (int, int) GetGridSize()
    {
        return (Grid.GetLength(1), Grid.GetLength(0));
    }
}