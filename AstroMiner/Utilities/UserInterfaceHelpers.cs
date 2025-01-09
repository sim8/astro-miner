using System.Collections.Generic;
using AstroMiner.Definitions;

namespace AstroMiner;

public static class UserInterfaceHelpers
{
    public static List<(int x, int y)> GetAsteroidEdgeCells(GridState gridState)
    {
        var edgeCells = new List<(int x, int y)>();
        for (var x = 0; x < GameConfig.GridSize; x++)
        for (var y = 0; y < GameConfig.GridSize; y++)
            if (gridState.GetWallType(x, y) != null &&
                gridState.CellHasNeighborWithWallType(x, y, null))
                edgeCells.Add((x, y));

        return edgeCells;
    }
}