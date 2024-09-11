using System;

namespace AstroMiner;

public enum CellState
{
    Empty,
    Rock
}

public enum Direction
{
    Top,
    Right,
    Bottom,
    Left
}

public class MiningState
{
    public const int Columns = 10;
    public const int Rows = 10;
    private CellState[,] _grid;
    private ObjectPosition _minerPos;

    public ObjectPosition MinerPos
    {
        get => _minerPos;
    }
    
    private Direction _minerDirection;
    
    public Direction MinerDirection
    {
        get => _minerDirection;
        private set => _minerDirection = value;
    }
    
    public MiningState()
    {
        _grid = new CellState[Rows, Columns];
        InitializeGrid();
        _minerDirection = Direction.Right;
        _minerPos = new ObjectPosition(0, 0);
    }

    private void InitializeGrid()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                _grid[row, col] = row * col < 80 && row * col > 20 ? CellState.Rock : CellState.Empty;
            }
        }
    }
    
    public CellState GetCellState(int row, int column)
    {
        return _grid[row, column];
    }
}