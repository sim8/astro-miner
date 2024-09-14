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
    public const int GridSize = 30;
    private const float DrillDistance = 0.2f;
    private const float MinerMovementSpeed = 9f;
    private readonly int _cellTextureSizePx;
    private readonly CellState[,] _grid;
    private readonly float _minerGridSize;
    private readonly int _minerTextureSizePx;

    private Vector2 _minerPos;

    public MiningState(int minerTextureSizePx, int cellTextureSizePx)
    {
        _grid = new CellState[GridSize, GridSize];
        InitializeGrid();
        MinerDirection = Direction.Right;
        _minerPos = new Vector2(0, 0);
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
            catch (Exception)
            {
                return false; // out of bounds
            }

        _minerPos = newTopLeft;
        return true;
    }

    private void InitializeGrid()
    {
        var centerX = GridSize / 2;
        var centerY = GridSize / 2;
        double averageRadius = 10;
        double maxDeviation = 4;
        var maxDelta = 0.5; // Max change in radius between adjacent angles

        var angleSegments = 90;
        var radiusValues = new double[angleSegments];
        var rand = new Random();

        // Generate smooth radius values to ensure the asteroid is connected
        radiusValues[0] = averageRadius + rand.NextDouble() * maxDeviation * 2 - maxDeviation;

        for (var i = 1; i < angleSegments; i++)
        {
            // Gradually change the radius to create smooth imperfections
            var delta = rand.NextDouble() * maxDelta * 2 - maxDelta;
            radiusValues[i] = radiusValues[i - 1] + delta;

            // Clamp the radius within [averageRadius - maxDeviation, averageRadius + maxDeviation]
            if (radiusValues[i] > averageRadius + maxDeviation)
                radiusValues[i] = averageRadius + maxDeviation;
            if (radiusValues[i] < averageRadius - maxDeviation)
                radiusValues[i] = averageRadius - maxDeviation;
        }

        // Populate the grid
        for (var x = 0; x < GridSize; x++)
        for (var y = 0; y < GridSize; y++)
        {
            double dx = x - centerX;
            double dy = y - centerY;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            // Calculate the angle and adjust it to be between 0 and 360 degrees
            var angle = Math.Atan2(dy, dx) * (180 / Math.PI);
            if (angle < 0) angle += 360;

            var index = (int)Math.Round(angle) % angleSegments;
            var radius = radiusValues[index];

            _grid[x, y] = distance <= radius ? CellState.Rock : CellState.Empty;
        }
    }

    public CellState GetCellState(int row, int column)
    {
        if (row < 0 || row >= GridSize || column < 0 || column >= GridSize) throw new IndexOutOfRangeException();
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