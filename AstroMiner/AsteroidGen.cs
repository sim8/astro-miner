using System;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public static class AsteroidGen
{
    private const float NoiseScale = 0.22f;
    private const float DiamondsRadius = 0.2f;
    private const float AsteroidCoreRadius = 0.6f;

    private const int AverageRadius = 30;
    private const int MaxDeviation = 12; // Adjusted for larger imperfections
    private const double MaxDelta = 9; // Adjusted for smoother transitions
    private const int AngleSegments = 140; // Adjusted for larger-scale variations
    private static readonly (float, float) SolidRockRange = (0.55f, 1);
    private static readonly (float, float) DiamondRange = (0.65f, 1);
    private static readonly (float, float) RubyRange = (0.41f, 0.42f);

    public static (CellState[,], Vector2) InitializeGridAndStartingPos(int gridSize, int seed)
    {
        var perlinNoise = new PerlinNoiseGenerator(seed);
        var grid = InitializeGrid(gridSize, perlinNoise, seed);
        return (grid, ClearAndGetStartingPos(grid));
    }

    private static Vector2 ClearAndGetStartingPos(CellState[,] grid)
    {
        for (var row = grid.GetLength(0) - 1; row >= 0; row--)
        {
            var solidBlocksInARow = 0;
            for (var col = 0; col < grid.GetLength(1); col++)
                if (grid[row, col].type != CellType.Empty)
                {
                    solidBlocksInARow++;
                }
                // Find first row which has >= 4 contiguous solid blocks as starting pos
                else if (solidBlocksInARow >= 4)
                {
                    var minerCellOffset = 1f - GameConfig.MinerSize / 2;
                    var minerColIndex = col - solidBlocksInARow / 2 - 1; // -1 to account for miner being 2x cell size
                    // Clear 2x4 landing area
                    grid[row, minerColIndex - 1].type = CellType.Floor;
                    grid[row, minerColIndex].type = CellType.Floor;
                    grid[row, minerColIndex + 1].type = CellType.Floor;
                    grid[row, minerColIndex + 2].type = CellType.Floor;
                    grid[row - 1, minerColIndex - 1].type = CellType.Floor;
                    grid[row - 1, minerColIndex].type = CellType.Floor;
                    grid[row - 1, minerColIndex + 1].type = CellType.Floor;
                    grid[row - 1, minerColIndex + 2].type = CellType.Floor;
                    return new Vector2(minerColIndex + minerCellOffset, row - 1 + minerCellOffset);
                }
        }

        throw new Exception("No 3x1 solid blocks in grid");
    }

    private static bool NoiseValWithinRange(float noiseValue, (float, float) range)
    {
        return noiseValue >= range.Item1 && noiseValue < range.Item2;
    }


    private static CellState[,] InitializeGrid(int gridSize, PerlinNoiseGenerator perlinNoise, int seed)
    {
        var grid = new CellState[gridSize, gridSize];
        var centerX = gridSize / 2;
        var centerY = gridSize / 2;
        var radiusValues = new double[AngleSegments];
        var perimeterWidths = new int[AngleSegments]; // New array for perimeter widths
        var rand = new Random(seed);

        // Generate smooth radius values
        radiusValues[0] = AverageRadius + rand.NextDouble() * MaxDeviation * 2 - MaxDeviation;

        for (var i = 1; i < AngleSegments; i++)
        {
            // Gradually change the radius to create smooth imperfections
            var delta = rand.NextDouble() * MaxDelta * 2 - MaxDelta;
            radiusValues[i] = radiusValues[i - 1] + delta;

            // Clamp the radius within [averageRadius - maxDeviation, averageRadius + maxDeviation]
            radiusValues[i] = Math.Max(AverageRadius - MaxDeviation,
                Math.Min(AverageRadius + MaxDeviation, radiusValues[i]));
        }

        // Optionally smooth the radius values further
        radiusValues = SmoothRadiusValues(radiusValues, 5);

        // Generate perimeter widths for each angle segment
        for (var i = 0; i < AngleSegments; i++)
            perimeterWidths[i] = rand.Next(0, 4); // Random integer between 0 and 3 inclusive

        // Optionally smooth the perimeter widths
        perimeterWidths = SmoothPerimeterWidths(perimeterWidths, 2);

        // Populate the grid
        for (var x = 0; x < gridSize; x++)
        for (var y = 0; y < gridSize; y++)
        {
            double dx = x - centerX;
            double dy = y - centerY;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            // Calculate the angle and adjust it to be between 0 and 360 degrees
            var angle = Math.Atan2(dy, dx) * (180 / Math.PI);
            if (angle < 0) angle += 360;

            var index = (int)Math.Round(angle * AngleSegments / 360.0) % AngleSegments;
            var radius = radiusValues[index];
            var perimeterWidth = perimeterWidths[index];


            CellType cellType;

            if (distance <= radius)
            {
                var xCoord = x * NoiseScale;
                var yCoord = y * NoiseScale;
                var noiseValue = perlinNoise.Noise(xCoord, yCoord);
                if (distance < radius * AsteroidCoreRadius && NoiseValWithinRange(noiseValue, SolidRockRange))
                    cellType = distance < radius * DiamondsRadius && NoiseValWithinRange(noiseValue, DiamondRange)
                        ? CellType.Diamond
                        : CellType.SolidRock;

                // Widen floor range relative to closeness to edge TODO make ramp clearer, especially near edges
                else if (NoiseValWithinRange(noiseValue, GetFloorRange(distance, radius)))
                    cellType = CellType.Floor;
                else
                    cellType = NoiseValWithinRange(noiseValue, RubyRange) ? CellType.Ruby : CellType.Rock;
            }
            else if (distance <= radius + perimeterWidth)
            {
                cellType = CellType.Floor;
            }
            else
            {
                cellType = CellType.Empty;
            }

            grid[x, y] = new CellState(cellType);
        }

        return grid;
    }

    // The floorspace should gradually disappear from outside -> inside.
    // Make it appear natural by narrowing noise range for floor
    private static (float, float) GetFloorRange(double distanceFromCenter, double radius)
    {
        // these are the min range
        var baseLowerBound = 0.28f;
        var baseUpperBound = 0.32f;

        var distanceFromCenterPercentage = (float)distanceFromCenter / (float)radius;

        // transform distanceFromCenter to a float that's 0 for most distances then ramping up a small amount
        var widenBy = Math.Max(distanceFromCenterPercentage - 0.5f, 0) / 1.6f;
        return (baseLowerBound - widenBy, baseUpperBound + widenBy);
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
}