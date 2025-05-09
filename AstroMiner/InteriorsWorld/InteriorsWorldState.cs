using System.Collections.Generic;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.InteriorsWorld;

public class InteriorsWorldState(BaseGame game) : BaseWorldState(game)
{
    public WorldCellType[,] Grid;

    public override void Initialize()
    {
        base.Initialize();
        Grid = WorldGrid.GetWorldGrid(game.Model.ActiveWorld);
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