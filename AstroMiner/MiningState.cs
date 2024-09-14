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
    private const float DrillDistance = 0.2f;
    private const float MinerMovementSpeed = 1.6f;
    private readonly int _cellTextureSizePx;
    private readonly CellState[,] _grid;
    private readonly float _minerGridSize;
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
        _minerGridSize = (float)_minerTextureSizePx / _cellTextureSizePx;
    }

    public Vector2 MinerPos => _minerPos;

    public Direction MinerDirection { get; private set; }

    private bool ApplyVectorToMinerPosIfNoCollisions(Vector2 vector)
    {
        var newTopLeft = _minerPos + vector;
        var newTopRight = _minerPos + vector + new Vector2(_minerGridSize, 0);
        var newBottomLeft = _minerPos + vector + new Vector2(0, _minerGridSize);
        var newBottomRight = _minerPos + vector + new Vector2(_minerGridSize, _minerGridSize);

        foreach (var newPosCorner in new[] { newTopLeft, newTopRight, newBottomRight, newBottomLeft })
            try
            {
                if (GetCellState(newPosCorner) != CellState.Empty) return false;
            }
            catch (Exception e)
            {
                return false; // out of bounds
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
        if (row < 0 || row >= Rows || column < 0 || column >= Columns) throw new IndexOutOfRangeException();
        return _grid[row, column];
    }

    public CellState GetCellState(Vector2 vector)
    {
        var gridX = (int)Math.Floor(vector.X);
        var gridY = (int)Math.Floor(vector.Y);
        return GetCellState(gridX, gridY);
    }

    public void SetCellState(Vector2 vector, CellState newState)
    {
        var gridX = (int)Math.Floor(vector.X);
        var gridY = (int)Math.Floor(vector.Y);
        _grid[gridX, gridY] = newState;
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

    public void UseDrill(int ellapsedGameTimeMs)
    {
        Vector2 drillPos;
        if (MinerDirection == Direction.Top) drillPos = _minerPos + new Vector2(_minerGridSize / 2, -DrillDistance);
        else if (MinerDirection == Direction.Right)
            drillPos = _minerPos + new Vector2(_minerGridSize + DrillDistance, _minerGridSize / 2);
        else if (MinerDirection == Direction.Bottom)
            drillPos = _minerPos + new Vector2(_minerGridSize / 2, _minerGridSize + DrillDistance);
        else drillPos = _minerPos + new Vector2(-DrillDistance, _minerGridSize / 2);

        if (GetCellState(drillPos) == CellState.Rock) SetCellState(drillPos, CellState.Empty);
    }
}