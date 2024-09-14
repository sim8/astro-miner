using System;

namespace AstroMiner;

public static class AsteroidGen
{
    public static CellState[,] InitializeGridAndStartingPos(int gridSize)
    {
        return InitializeGrid(gridSize);
    }

    private static CellState[,] InitializeGrid(int gridSize)
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
                grid[x, y] = CellState.Rock;
            else if (distance <= radius + perimeterWidth)
                grid[x, y] = CellState.Floor;
            else
                grid[x, y] = CellState.Empty;
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