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
    private const float MinerMovementSpeed = 1.4f;
    private readonly int _cellTextureSizePx;
    private readonly CellState[,] _grid;
    private readonly int _minerTextureSizePx;

    private Vector2 _minerPos;
    private int _scaleMultiplier;

    public MiningState(int scaleMultiplier,
        int minerTextureSizePx, int cellTextureSizePx)
    {
        _grid = new CellState[Rows, Columns];
        InitializeGrid();
        MinerDirection = Direction.Right;
        _minerPos = new Vector2(0, 0);
        _scaleMultiplier = scaleMultiplier;
        _minerTextureSizePx = minerTextureSizePx;
        _cellTextureSizePx = cellTextureSizePx;
    }

    public Vector2 MinerPos => _minerPos;

    public Direction MinerDirection { get; private set; }

    private bool ApplyVectorToMinerPosIfNoCollisions(Vector2 vector)
    {
        var minerGridSize = (float)_minerTextureSizePx / _cellTextureSizePx;
        var newTopLeft = _minerPos + vector;
        var newTopRight = _minerPos + vector + new Vector2(minerGridSize, 0);
        var newBottomLeft = _minerPos + vector + new Vector2(0, minerGridSize);
        var newBottomRight = _minerPos + vector + new Vector2(minerGridSize, minerGridSize);

        foreach (var newPosCorner in new[] { newTopLeft, newTopRight, newBottomRight, newBottomLeft })
        {
            var gridX = (int)Math.Floor(newPosCorner.X);
            var gridY = (int)Math.Floor(newPosCorner.Y);

            // Out of bounds
            if (gridX < 0 || gridX >= Rows || gridY < 0 || gridY >= Columns) return false;

            // Collision
            if (_grid[gridX, gridY] != CellState.Empty) return false;
        }

        _minerPos = newTopLeft;

        return true;
    }

    private void InitializeGrid()
    {
        for (var row = 0; row < Rows; row++)
        for (var col = 0; col < Columns; col++)
            _grid[row, col] = row * col < 80 && row * col > 7 ? CellState.Rock : CellState.Empty;
    }

    public CellState GetCellState(int row, int column)
    {
        return _grid[row, column];
    }

    public void MoveMiner(Direction direction, int ellapsedGameTimeMs)
    {
        MinerDirection = direction;
        var distance = MinerMovementSpeed * (ellapsedGameTimeMs / 1000f);

        if (direction == Direction.Top) ApplyVectorToMinerPosIfNoCollisions(new Vector2(0, -distance));
        if (direction == Direction.Right) ApplyVectorToMinerPosIfNoCollisions(new Vector2(distance, 0));
        if (direction == Direction.Bottom) ApplyVectorToMinerPosIfNoCollisions(new Vector2(0, distance));
        if (direction == Direction.Left) ApplyVectorToMinerPosIfNoCollisions(new Vector2(-distance, 0));
        Console.WriteLine(_minerPos.ToString());
    }
}