using System;

namespace AstroMiner;

public struct CellState
{
    public CellType type;
    public bool hasLavaWell;

    public CellState(CellType type, bool hasLavaWell)
    {
        this.type = type;
        this.hasLavaWell = hasLavaWell;
    }
}

public class GridState(GameState gameState, CellState[,] grid)
{
    public int Columns => grid.GetLength(0);
    public int Rows => grid.GetLength(1);

    public CellState GetCellState(int x, int y)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        return grid[y, x];
    }

    public CellType GetCellType(int x, int y)
    {
        return GetCellState(x, y).type;
    }

    public void DemolishCell(int x, int y, bool addToInventory = false)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        if (grid[y, x].type == CellType.Empty) return;

        if (grid[y, x].type == CellType.Ruby && addToInventory) gameState.Inventory.NumRubies++;
        if (grid[y, x].type == CellType.Diamond && addToInventory) gameState.Inventory.NumDiamonds++;

        grid[y, x].type = CellType.Floor;
    }

    public bool CellHasNeighbourOfType(int x, int y, CellType cellType)
    {
        int[] xOffsets = { -1, 0, 1, 1, 1, 0, -1, -1 };
        int[] yOffsets = { -1, -1, -1, 0, 1, 1, 1, 0 };

        for (var i = 0; i < xOffsets.Length; i++)
        {
            var newX = x + xOffsets[i];
            var newY = y + yOffsets[i];

            if (ViewHelpers.IsValidGridPosition(newX, newY) && grid[newY, newX].type == cellType) return true;
        }

        return false;
    }
}