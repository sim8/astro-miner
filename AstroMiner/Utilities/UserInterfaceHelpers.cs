using System.Collections.Generic;
using AstroMiner.AsteroidWorld;
using AstroMiner.Definitions;

namespace AstroMiner;

public static class UserInterfaceHelpers
{
    public static List<(int x, int y)> GetAsteroidEdgeCells(GridState gridState)
    {
        var edgeCells = new List<(int x, int y)>();
        for (var x = 0; x < GameConfig.GridSize; x++)
        for (var y = 0; y < GameConfig.GridSize; y++)
            if (!gridState.GetCellState(x, y).isEmpty &&
                gridState.CheckNeighbors(x, y, n => n.isEmpty))
                edgeCells.Add((x, y));

        return edgeCells;
    }
}