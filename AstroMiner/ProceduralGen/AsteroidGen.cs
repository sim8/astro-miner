using System;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.ProceduralGen;

public static class AsteroidGen
{
    public static (CellState[,], Vector2) InitializeGridAndStartingPos(int gridSize, int seed)
    {
        var perlinNoise1 = new PerlinNoiseGenerator(seed);
        var perlinNoise2 = new PerlinNoiseGenerator(seed + 42);
        var grid = InitializeGrid(gridSize, perlinNoise1, perlinNoise2, seed);
        return (grid, ClearAndGetStartingPos(grid));
    }

    private static Vector2 ClearAndGetStartingPos(CellState[,] grid)
    {
        for (var row = grid.GetLength(0) - 1; row >= 0; row--)
        {
            var flooredCellsInARow = 0;
            for (var col = 0; col < grid.GetLength(1); col++)
                if (grid[row, col].FloorType != null)
                {
                    flooredCellsInARow++;
                }
                // Find first row which has >= 4 contiguous solid blocks as starting pos
                else if (flooredCellsInARow >= 4)
                {
                    var minerCellOffset = 1f - GameConfig.MinerSize / 2;
                    var minerColIndex = col - flooredCellsInARow / 2 - 1; // -1 to account for miner being 2x cell size

                    // Clear 2x4 landing area
                    for (var r = row - 1; r <= row; r++) // Rows: one above and the current row
                    for (var c = minerColIndex - 1; c <= minerColIndex + 2; c++) // Columns: -1 to +2 from minerColIndex
                    {
                        grid[r, c].FloorType = FloorType.Floor;
                        grid[r, c].WallType = null;
                    }


                    return new Vector2(minerColIndex + minerCellOffset, row - 1 + minerCellOffset);
                }
        }

        throw new Exception("No 3x1 solid blocks in grid");
    }

    private static CellState[,] InitializeGrid(int gridSize, PerlinNoiseGenerator perlinNoise1,
        PerlinNoiseGenerator perlinNoise2, int seed)
    {
        var grid = new CellState[gridSize, gridSize];
        var centerX = gridSize / 2;
        var centerY = gridSize / 2;
        var radiusValues = new double[GameConfig.AsteroidGen.AngleSegments];
        var perimeterWidths = new int[GameConfig.AsteroidGen.AngleSegments]; // New array for perimeter widths
        var rand = new Random(seed);

        // Generate smooth radius values
        radiusValues[0] = GameConfig.AsteroidGen.AverageRadius +
            rand.NextDouble() * GameConfig.AsteroidGen.MaxDeviation * 2 - GameConfig.AsteroidGen.MaxDeviation;

        for (var i = 1; i < GameConfig.AsteroidGen.AngleSegments; i++)
        {
            // Gradually change the radius to create smooth imperfections
            var delta = rand.NextDouble() * GameConfig.AsteroidGen.MaxDelta * 2 - GameConfig.AsteroidGen.MaxDelta;
            radiusValues[i] = radiusValues[i - 1] + delta;

            // Clamp the radius within [averageRadius - maxDeviation, averageRadius + maxDeviation]
            radiusValues[i] = Math.Max(GameConfig.AsteroidGen.AverageRadius - GameConfig.AsteroidGen.MaxDeviation,
                Math.Min(GameConfig.AsteroidGen.AverageRadius + GameConfig.AsteroidGen.MaxDeviation, radiusValues[i]));
        }

        // Optionally smooth the radius values further
        radiusValues = SmoothRadiusValues(radiusValues, 5);

        // Generate perimeter widths for each angle segment
        for (var i = 0; i < GameConfig.AsteroidGen.AngleSegments; i++)
            perimeterWidths[i] = rand.Next(2, 6); // Random integer between 0 and 3 inclusive

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

            var index = (int)Math.Round(angle * GameConfig.AsteroidGen.AngleSegments / 360.0) %
                        GameConfig.AsteroidGen.AngleSegments;
            var radius = radiusValues[index];
            var perimeterWidth = perimeterWidths[index];


            var distancePerc = (float)(distance / (radius + perimeterWidth));
            var noise1Value = perlinNoise1.Noise(x * GameConfig.AsteroidGen.Perlin1NoiseScale,
                y * GameConfig.AsteroidGen.Perlin1NoiseScale);
            var noise2Value = perlinNoise2.Noise(x * GameConfig.AsteroidGen.Perlin2NoiseScale,
                y * GameConfig.AsteroidGen.Perlin2NoiseScale);

            var (wallType, floorType) = CellGenRules.EvaluateRules(distancePerc, noise1Value, noise2Value);

            var layer = distance < radius * GameConfig.AsteroidGen.CoreRadius ? AsteroidLayer.Core :
                distance < radius * GameConfig.AsteroidGen.MantleRadius ? AsteroidLayer.Mantle :
                floorType != null ? AsteroidLayer.Crust : AsteroidLayer.None;

            grid[x, y] = new CellState(wallType, floorType, layer);
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