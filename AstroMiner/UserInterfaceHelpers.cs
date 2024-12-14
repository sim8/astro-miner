using System.Collections.Generic;

namespace AstroMiner;

public static class UserInterfaceHelpers
{
    public static List<(int x, int y)> GetAsteroidEdgeCells(GridState gridState)
    {
        var edgeCells = new List<(int x, int y)>();
        for (var x = 0; x < GameConfig.GridSize; x++)
        for (var y = 0; y < GameConfig.GridSize; y++)
            if (gridState.GetCellState(x, y) != CellState.Empty &&
                gridState.CellHasNeighbourOfType(x, y, CellState.Empty))
                edgeCells.Add((x, y));

        return edgeCells;
    }
}