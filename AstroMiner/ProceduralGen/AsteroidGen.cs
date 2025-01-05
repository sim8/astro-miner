using System;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.ProceduralGen;

public static class AsteroidGen
{
    private const float MantleRadius = 0.7f;
    private const float LavaRadius = MantleRadius - 0.1f;
    private const float DiamondsRadius = 0.15f;
    private const float CoreRadius = 0.27f;
    private const float OuterWallRadius = 0.75f;

    private const int AverageRadius = 80;
    private const int MaxDeviation = 12; // Adjusted for larger imperfections
    private const double MaxDelta = 9; // Adjusted for smoother transitions
    private const int AngleSegments = 140; // Adjusted for larger-scale variations


    // Two different layers of Perlin noise
    // Layer 1 - Default layer, more granular
    // Layer 2 - Lower frequency - used to define larger areas
    //   - Lava lakes + adjoining floor
    //   - Gold zones (near lava)
    //   - Explosive rock regions

    private const float Perlin1NoiseScale = 0.22f;

    // Lower frequency for bigger, cleaner lakes
    private const float Perlin2NoiseScale = 0.14f;

    // Perlin noise 1
    private static readonly (float, float) MantleSolidRockRange = (0.65f, 1);
    private static readonly (float, float) CoreSolidRockRange = (0.56f, 1);
    private static readonly (float, float) LavaFloorPerimeterRange1 = (0.4f, 0.6f);
    private static readonly (float, float) DiamondRange = (0.7f, 1);
    private static readonly (float, float) RubyRange = (0.410f, 0.415f);

    private static readonly (float, float) GoldRange1 = (0.42f, 0.43f);

    private static readonly (float, float) ExplosiveRockRange = (0.38f, 0.39f);

    // Perlin noise 2
    private static readonly (float, float) LavaRange = (0.55f, 1);
    private static readonly (float, float) LavaFloorPerimeterRange2 = (0.42f, 0.55f);
    private static readonly (float, float) GoldRange2 = (0.5f, 0.58f);

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
            var solidBlocksInARow = 0;
            for (var col = 0; col < grid.GetLength(1); col++)
                if (grid[row, col].Type != CellType.Empty)
                {
                    solidBlocksInARow++;
                }
                // Find first row which has >= 4 contiguous solid blocks as starting pos
                else if (solidBlocksInARow >= 4)
                {
                    var minerCellOffset = 1f - GameConfig.MinerSize / 2;
                    var minerColIndex = col - solidBlocksInARow / 2 - 1; // -1 to account for miner being 2x cell size
                    // Clear 2x4 landing area
                    grid[row, minerColIndex - 1].Type = CellType.Floor;
                    grid[row, minerColIndex].Type = CellType.Floor;
                    grid[row, minerColIndex + 1].Type = CellType.Floor;
                    grid[row, minerColIndex + 2].Type = CellType.Floor;
                    grid[row - 1, minerColIndex - 1].Type = CellType.Floor;
                    grid[row - 1, minerColIndex].Type = CellType.Floor;
                    grid[row - 1, minerColIndex + 1].Type = CellType.Floor;
                    grid[row - 1, minerColIndex + 2].Type = CellType.Floor;
                    return new Vector2(minerColIndex + minerCellOffset, row - 1 + minerCellOffset);
                }
        }

        throw new Exception("No 3x1 solid blocks in grid");
    }

    private static bool NoiseValWithinRange(float noiseValue, (float, float) range)
    {
        return noiseValue >= range.Item1 && noiseValue < range.Item2;
    }


    private static CellState[,] InitializeGrid(int gridSize, PerlinNoiseGenerator perlinNoise1,
        PerlinNoiseGenerator perlinNoise2, int seed)
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

            var index = (int)Math.Round(angle * AngleSegments / 360.0) % AngleSegments;
            var radius = radiusValues[index];
            var perimeterWidth = perimeterWidths[index];


            CellType cellType;

            var floorRange = GetFloorRange(distance, radius);

            if (distance <= radius)
            {
                var noise1ValueD = perlinNoise1.Noise(x * Perlin1NoiseScale, y * Perlin1NoiseScale);
                var noise2ValueD = perlinNoise2.Noise(x * Perlin2NoiseScale, y * Perlin2NoiseScale);
                var withinMantle = distance < radius * MantleRadius;
                var withinCore = distance < radius * CoreRadius;
                var isMantle = withinMantle && !withinCore;
                var isLavaDistance = distance < radius * LavaRadius && !withinCore;

                if (isLavaDistance && NoiseValWithinRange(noise2ValueD, LavaRange))
                    cellType = CellType.Lava;
                else if (isLavaDistance && NoiseValWithinRange(noise1ValueD, LavaFloorPerimeterRange1) &&
                         NoiseValWithinRange(noise2ValueD, LavaFloorPerimeterRange2))
                    cellType = CellType.Floor;
                // Use both ranges for gold - near lake but still high-ish frequency of noise1
                else if (isLavaDistance && NoiseValWithinRange(noise2ValueD, GoldRange2) &&
                         NoiseValWithinRange(noise1ValueD, GoldRange1))
                    cellType = CellType.Gold;

                else if (withinCore &&
                         NoiseValWithinRange(noise1ValueD, ExplosiveRockRange))
                    cellType = CellType.ExplosiveRock;
                else if ((withinMantle && NoiseValWithinRange(noise1ValueD, MantleSolidRockRange)) ||
                         (withinCore && NoiseValWithinRange(noise1ValueD, CoreSolidRockRange)))
                    cellType = distance < radius * DiamondsRadius && NoiseValWithinRange(noise1ValueD, DiamondRange)
                        ? CellType.Diamond
                        : CellType.SolidRock;
                else if (distance > radius * OuterWallRadius && NoiseValWithinRange(noise1ValueD,
                             (floorRange.Item2 + 0.1f, floorRange.Item2 + 0.102f)))
                    cellType = CellType.Nickel;
                // Widen floor range relative to closeness to edge TODO make ramp clearer, especially near edges
                else if (NoiseValWithinRange(noise1ValueD, floorRange))
                    cellType = CellType.Floor;
                else
                    cellType = NoiseValWithinRange(noise1ValueD, RubyRange) ? CellType.Ruby : CellType.Rock;
            }
            else if (distance <= radius + perimeterWidth)
            {
                cellType = CellType.Floor;
            }
            else
            {
                cellType = CellType.Empty;
            }

            var layer = distance < radius * CoreRadius ? AsteroidLayer.Core :
                distance < radius * MantleRadius ? AsteroidLayer.Mantle :
                cellType != CellType.Empty ? AsteroidLayer.Crust : AsteroidLayer.None;

            var distancePerc = (float)(distance / (radius + perimeterWidth));
            var noise1Value = perlinNoise1.Noise(x * Perlin1NoiseScale, y * Perlin1NoiseScale);
            var noise2Value = perlinNoise2.Noise(x * Perlin2NoiseScale, y * Perlin2NoiseScale);

            cellType = CellGenRules.EvaluateRules(distancePerc, noise1Value, noise2Value);

            grid[x, y] = new CellState(cellType, layer);
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
        var widenBy = Math.Max(distanceFromCenterPercentage - 0.6f, 0) / 1.6f;
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