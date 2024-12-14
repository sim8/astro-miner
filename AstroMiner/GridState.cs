using System;

namespace AstroMiner;

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

    public void DemolishCell(int x, int y, bool addToInventory = false)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        if (grid[y, x] == CellState.Empty) return;

        if (grid[y, x] == CellState.Ruby && addToInventory) gameState.Inventory.NumRubies++;
        if (grid[y, x] == CellState.Diamond && addToInventory) gameState.Inventory.NumDiamonds++;

        grid[y, x] = CellState.Floor;
    }
}