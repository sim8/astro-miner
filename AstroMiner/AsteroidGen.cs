using System;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public static class AsteroidGen
{
    private const float NoiseScale = 0.4f;
    private const float InnerThreshold = 0.55f; // Adjust this value
    private const float OuterThreshold = 0.65f; // Adjust this value

    public static (CellState[,], Vector2) InitializeGridAndStartingPos(int gridSize, float minerSize)
    {
        var rnd = new Random();
        var seed = rnd.Next(1, 999);
        var perlinNoise = new PerlinNoiseGenerator(seed);
        var grid = InitializeGrid(gridSize, perlinNoise);
        return (grid, ClearAndGetStartingPos(grid, minerSize));
    }

    private static Vector2 ClearAndGetStartingPos(CellState[,] grid, float minerSize)
    {
        for (var row = grid.GetLength(0) - 1; row >= 0; row--)
        {
            var solidBlocksInARow = 0;
            for (var col = 0; col < grid.GetLength(1); col++)
                if (grid[row, col] != CellState.Empty)
                {
                    solidBlocksInARow++;
                }
                // Find first row which has >= 3 contiguous solid blocks as starting pos
                else if (solidBlocksInARow >= 3)
                {
                    var minerCellOffset = 0.5f - minerSize / 2;
                    var minerColIndex = col - solidBlocksInARow / 2;
                    // Clear 2x3 landing area
                    grid[row, minerColIndex - 1] = CellState.Floor;
                    grid[row, minerColIndex] = CellState.Floor;
                    grid[row, minerColIndex + 1] = CellState.Floor;
                    grid[row - 1, minerColIndex - 1] = CellState.Floor;
                    grid[row - 1, minerColIndex] = CellState.Floor;
                    grid[row - 1, minerColIndex + 1] = CellState.Floor;
                    return new Vector2(minerColIndex + minerCellOffset, row + minerCellOffset);
                }
        }

        throw new Exception("No 3x1 solid blocks in grid");
    }


    private static CellState[,] InitializeGrid(int gridSize, PerlinNoiseGenerator perlinNoise)
    {
        var grid = new CellState[gridSize, gridSize];
        var centerX = gridSize / 2;
        var centerY = gridSize / 2;
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
        for (var x = 0; x < gridSize; x++)
        for (var y = 0; y < gridSize; y++)
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
            {
                var xCoord = x * NoiseScale;
                var yCoord = y * NoiseScale;
                var noiseValue = perlinNoise.Noise(xCoord, yCoord);
                var threshold = distance < radius * 0.7 ? InnerThreshold : OuterThreshold;
                grid[x, y] = noiseValue > threshold ? CellState.SolidRock : CellState.Rock;
            }
            else if (distance <= radius + perimeterWidth)
            {
                grid[x, y] = CellState.Floor;
            }
            else
            {
                grid[x, y] = CellState.Empty;
            }
        }

        return grid;
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