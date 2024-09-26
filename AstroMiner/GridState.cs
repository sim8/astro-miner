using System;
using System.Collections.Generic;

namespace AstroMiner;

public class GridState
{
    private readonly List<Entity> _activeEntities;
    private readonly CellState[,] _grid;

    public GridState(CellState[,] grid)
    {
        _grid = grid;
    }

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
}