using System;
using Microsoft.Xna.Framework;

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
    private const float MinerMovementSpeed = 0.5f;
    private readonly CellState[,] _grid;

    private Vector2 _minerPos;

    public MiningState()
    {
        _grid = new CellState[Rows, Columns];
        InitializeGrid();
        MinerDirection = Direction.Right;
        _minerPos = new Vector2(0, 0);
    }

    public Vector2 MinerPos => _minerPos;

    public Direction MinerDirection { get; private set; }

    private void InitializeGrid()
    {
        for (var row = 0; row < Rows; row++)
        for (var col = 0; col < Columns; col++)
            _grid[row, col] = row * col < 80 && row * col > 20 ? CellState.Rock : CellState.Empty;
    }

    public CellState GetCellState(int row, int column)
    {
        return _grid[row, column];
    }

    public void AttemptMove(Direction direction, int ellapsedGameTimeMs)
    {
        MinerDirection = direction;
        var distance = MinerMovementSpeed * (ellapsedGameTimeMs / 1000f);
        if (direction == Direction.Top) _minerPos += new Vector2(0, -distance);
        if (direction == Direction.Right) _minerPos += new Vector2(distance, 0);
        if (direction == Direction.Bottom) _minerPos += new Vector2(0, distance);
        if (direction == Direction.Left) _minerPos += new Vector2(-distance, 0);
        Console.WriteLine(_minerPos.ToString());
    }
}