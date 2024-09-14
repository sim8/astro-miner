using System;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public enum CellState
{
    Empty,
    Floor,
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
    public const int GridSize = 40;
    private const float DrillDistance = 0.2f;
    private const float MinerMovementSpeed = 9f;
    private readonly int _cellTextureSizePx;
    private readonly CellState[,] _grid;
    private readonly float _minerGridSize;
    private readonly int _minerTextureSizePx;

    public MiningState(int minerTextureSizePx, int cellTextureSizePx)
    {
        _grid = new CellState[GridSize, GridSize];
        InitializeGrid();
        MinerDirection = Direction.Right;
        MinerPos = new Vector2(0, 0);
        _minerTextureSizePx = minerTextureSizePx;
        _cellTextureSizePx = cellTextureSizePx;
        _minerGridSize = (float)_minerTextureSizePx / _cellTextureSizePx;
    }

    public Vector2 MinerPos { get; private set; }

    public Direction MinerDirection { get; private set; }

    private bool ApplyVectorToMinerPosIfNoCollisions(Vector2 vector)
    {
        var newTopLeft = MinerPos + vector;
        var newTopRight = MinerPos + vector + new Vector2(_minerGridSize, 0);
        var newBottomLeft = MinerPos + vector + new Vector2(0, _minerGridSize);
        var newBottomRight = MinerPos + vector + new Vector2(_minerGridSize, _minerGridSize);

        foreach (var newPosCorner in new[] { newTopLeft, newTopRight, newBottomRight, newBottomLeft })
            try
            {
                if (GetCellState(newPosCorner) != CellState.Empty) return false;
            }
            catch (Exception)
            {
                return false; // out of bounds
            }

        MinerPos = newTopLeft;
        return true;
    }

    private void InitializeGrid()
    {
        var centerX = GridSize / 2;
        var centerY = GridSize / 2;
        double averageRadius = 15;
        double maxDeviation = 3; // Adjusted for larger imperfections
        var maxDelta = 1; // Adjusted for smoother transitions

        var angleSegments = 90; // Adjusted for larger-scale variations
        var radiusValues = new double[angleSegments];
        var perimeterWidths = new int[angleSegments]; // New array for perimeter widths
        var rand = new Random();

        // Generate smooth radius values
        radiusValues[0] = averageRadius + rand.NextDouble() * maxDeviation * 2 - maxDeviation;

        for (var i = 1; i < angleSegments; i++)
        {
            // Gradually change the radius to create smooth imperfections
            var delta = rand.NextDouble() * maxDelta * 2 - maxDelta;
            radiusValues[i] = radiusValues[i - 1] + delta;

            // Clamp the radius within [averageRadius - maxDeviation, averageRadius + maxDeviation]
            radiusValues[i] = Math.Max(averageRadius - maxDeviation,
                Math.Min(averageRadius + maxDeviation, radiusValues[i]));
        }

        // Optionally smooth the radius values further
        radiusValues = SmoothRadiusValues(radiusValues, 5);

        // Generate perimeter widths for each angle segment
        for (var i = 0; i < angleSegments; i++)
            perimeterWidths[i] = rand.Next(0, 4); // Random integer between 0 and 3 inclusive

        // Optionally smooth the perimeter widths
        perimeterWidths = SmoothPerimeterWidths(perimeterWidths, 2);

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

            var index = (int)Math.Round(angle * angleSegments / 360.0) % angleSegments;
            var radius = radiusValues[index];
            var perimeterWidth = perimeterWidths[index];

            if (distance <= radius)
                _grid[x, y] = CellState.Rock;
            else if (distance <= radius + perimeterWidth)
                _grid[x, y] = CellState.Floor;
            else
                _grid[x, y] = CellState.Empty;
        }
    }

    // Function to smooth radius values
    private static double[] SmoothRadiusValues(double[] values, int smoothingFactor)
    {
        var length = values.Length;
        var smoothedValues = new double[length];

        for (var i = 0; i < length; i++)
        {
            double sum = 0;
            var count = 0;

            for (var j = -smoothingFactor; j <= smoothingFactor; j++)
            {
                var index = (i + j + length) % length;
                sum += values[index];
                count++;
            }

            smoothedValues[i] = sum / count;
        }

        return smoothedValues;
    }

    // Function to smooth perimeter widths
    private static int[] SmoothPerimeterWidths(int[] values, int smoothingFactor)
    {
        var length = values.Length;
        var smoothedValues = new int[length];

        for (var i = 0; i < length; i++)
        {
            var sum = 0;
            var count = 0;

            for (var j = -smoothingFactor; j <= smoothingFactor; j++)
            {
                var index = (i + j + length) % length;
                sum += values[index];
                count++;
            }

            smoothedValues[i] = (int)Math.Round((double)sum / count);
        }

        return smoothedValues;
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
    }

    public void UseDrill(int ellapsedGameTimeMs)
    {
        Vector2 drillPos;
        if (MinerDirection == Direction.Top) drillPos = MinerPos + new Vector2(_minerGridSize / 2, -DrillDistance);
        else if (MinerDirection == Direction.Right)
            drillPos = MinerPos + new Vector2(_minerGridSize + DrillDistance, _minerGridSize / 2);
        else if (MinerDirection == Direction.Bottom)
            drillPos = MinerPos + new Vector2(_minerGridSize / 2, _minerGridSize + DrillDistance);
        else drillPos = MinerPos + new Vector2(-DrillDistance, _minerGridSize / 2);

        if (GetCellState(drillPos) == CellState.Rock) SetCellState(drillPos, CellState.Empty);
    }
}