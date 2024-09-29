using System;

namespace AstroMiner;

public class GridState
{
    private readonly CellState[,] _grid;

    public GridState(CellState[,] grid)
    {
        _grid = grid;
    }

    public int Columns => _grid.GetLength(0);
    public int Rows => _grid.GetLength(1);

    public CellState GetCellState(int x, int y)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        return _grid[y, x];
    }

    public void SetCellState(int x, int y, CellState newState)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        _grid[y, x] = newState;
    }

    public void DemolishCell(int x, int y)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        if (_grid[y, x] == CellState.Empty) return;
        _grid[y, x] = CellState.Floor;
    }
}