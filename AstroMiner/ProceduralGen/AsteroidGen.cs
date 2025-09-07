using System;
using AstroMiner.Definitions;
using AstroMiner.ECS.Systems;
using AstroMiner.Model;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ProceduralGen;

public static class AsteroidGen
{
    private const float LavaNoise2Start = 0.55f;
    private const float UnstableNoiseRange = 0.15f;

    public static (CellState[,], Vector2) InitializeGridAndStartingCenterPos(int gridSize, int seed)
    {
        var perlinNoise1 = new PerlinNoiseGenerator(seed);
        var perlinNoise2 = new PerlinNoiseGenerator(seed + 42);
        var grid = InitializeGrid(gridSize, perlinNoise1, perlinNoise2, seed);
        grid = AsteroidGridHelpers.CenterAsteroidInGrid(grid);
        return (grid, ClearAndGetStartingCenterPos(grid));
    }

    /// <summary>
    ///     Looks from the left side of the asteroid for the first column to contain 4 contiguous floored cells.
    ///     Clears a 2x4 landing area (current column and one to the right) and returns the centerPos of the 4 cells.
    /// </summary>
    /// <param name="grid">
    ///     2d array representing the roughly circular asteroid. FloorType.Empty means empty space around the
    ///     asteroid.
    /// </param>
    /// <returns>The starting centerPos for the miner</returns>
    /// <exception cref="Exception"></exception>
    private static Vector2 ClearAndGetStartingCenterPos(CellState[,] grid)
    {
        for (var col = 0; col < grid.GetLength(1); col++)
        {
            var flooredCellsInARow = 0;
            for (var row = 0; row < grid.GetLength(0); row++)
                if (grid[row, col].FloorType != FloorType.Empty)
                {
                    flooredCellsInARow++;
                }
                // Find first column which has >= 4 contiguous solid blocks as starting pos
                else if (flooredCellsInARow >= 4)
                {
                    var minerRowIndex = row - flooredCellsInARow / 2;

                    // Clear 2x4 landing area
                    for (var c = col; c <= col + 1; c++) // Columns: current column and one to the right
                        for (var r = minerRowIndex - 1; r <= minerRowIndex + 2; r++) // Rows: -1 to +2 from minerRowIndex
                        {
                            grid[r, c].FloorType = FloorType.Floor;
                            grid[r, c].WallType = WallType.Empty;
                        }

                    return new Vector2(col + 1, minerRowIndex);
                }
        }

        throw new Exception("No 4x1 solid blocks in grid");
    }

    /// <summary>
    ///     Calculate the average radius for a given angle, with the tail side being longer.
    /// </summary>
    /// <param name="angleDegrees">Angle in degrees (0-360)</param>
    /// <returns>Average radius for this angle</returns>
    private static double GetAverageRadiusForAngle(double angleDegrees)
    {
        // Target angle for the tail
        const double tailAngle = 270.0;

        // Calculate the angular distance from the tail angle
        var distanceFromTail = Math.Abs(angleDegrees - tailAngle);
        if (distanceFromTail > 180) distanceFromTail = 360 - distanceFromTail;

        // Only apply tail radius within the tail segment
        var halfTailSegment = GameConfig.AsteroidGen.TailSegmentAngleDegrees / 2.0;

        if (distanceFromTail > halfTailSegment)
            // Outside the tail segment - use normal radius
            return GameConfig.AsteroidGen.AverageRadius;

        // Within the tail segment - smooth transition from center to edge
        var influence = Math.Cos(Math.PI * distanceFromTail / (2 * halfTailSegment));

        // Interpolate between normal radius and tail radius
        return GameConfig.AsteroidGen.AverageRadius +
               influence * (GameConfig.AsteroidGen.AverageTailRadius - GameConfig.AsteroidGen.AverageRadius);
    }

    // TODO gross. Does it have to be separate from rest of the config?
    private static float GetStability(float distancePerc, float noise1Value, float noise2Value)
    {
        if (distancePerc is < GameConfig.AsteroidGen.CoreRadius or > GameConfig.AsteroidGen.MantleRadius)
            return 1f;

        var normalizedStability = 1f - Math.Clamp((noise2Value - (LavaNoise2Start - UnstableNoiseRange)) / UnstableNoiseRange, 0f, 1f);

        // Map from 0-1 range to CriticalStabilityThreshold-1 range while preserving distribution
        return CellStabilitySystem.CriticalStabilityThreshold + normalizedStability * (1f - CellStabilitySystem.CriticalStabilityThreshold);
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

        // Generate smooth radius values with angle-specific average radius
        var angle0 = 0.0; // Start at 0 degrees
        var averageRadius0 = GetAverageRadiusForAngle(angle0);
        radiusValues[0] = averageRadius0 +
            rand.NextDouble() * GameConfig.AsteroidGen.MaxDeviation * 2 - GameConfig.AsteroidGen.MaxDeviation;

        for (var i = 1; i < GameConfig.AsteroidGen.AngleSegments; i++)
        {
            // Calculate the angle for this segment
            var angle = i * 360.0 / GameConfig.AsteroidGen.AngleSegments;
            var averageRadius = GetAverageRadiusForAngle(angle);

            // Gradually change the radius to create smooth imperfections
            var delta = rand.NextDouble() * GameConfig.AsteroidGen.MaxDelta * 2 - GameConfig.AsteroidGen.MaxDelta;
            radiusValues[i] = radiusValues[i - 1] + delta;

            // Clamp the radius within [averageRadius - maxDeviation, averageRadius + maxDeviation]
            // Use the angle-specific average radius for clamping bounds
            radiusValues[i] = Math.Max(averageRadius - GameConfig.AsteroidGen.MaxDeviation,
                Math.Min(averageRadius + GameConfig.AsteroidGen.MaxDeviation, radiusValues[i]));
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
                var stability = GetStability(distancePerc, noise1Value, noise2Value);

                var layer = distance < radius * GameConfig.AsteroidGen.CoreRadius ? AsteroidLayer.Core :
                    distance < radius * GameConfig.AsteroidGen.MantleRadius ? AsteroidLayer.Mantle :
                    floorType != FloorType.Empty ? AsteroidLayer.Crust : AsteroidLayer.None;

                grid[x, y] = new CellState
                {
                    WallType = wallType,
                    FloorType = floorType,
                    Layer = layer,
                    Stability = stability
                };
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