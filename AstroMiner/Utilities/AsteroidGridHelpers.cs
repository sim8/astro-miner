using System;
using AstroMiner.Definitions;
using AstroMiner.Model;

namespace AstroMiner.Utilities;

public struct AsteroidBoundingBox
{
    public int X;
    public int Y;
    public int Right;
    public int Bottom;
    public int Width;
    public int Height;
}

public static class AsteroidGridHelpers
{
    /// <summary>
    ///     Centers the asteroid within the grid by finding its bounds and shifting it to be centered on both axes.
    /// </summary>
    /// <param name="grid">The grid containing the asteroid to center</param>
    /// <returns>A new grid with the asteroid centered</returns>
    public static CellState[,] CenterAsteroidInGrid(CellState[,] grid)
    {
        var boundingBox = GetAsteroidBoundingBoxWithinGrid(grid);
        var gridSize = grid.GetLength(0);
        var minRow = boundingBox.Y;
        var maxRow = boundingBox.Bottom;
        var minCol = boundingBox.X;
        var maxCol = boundingBox.Right;

        // If no asteroid was found, return the original grid
        if (minRow > maxRow || minCol > maxCol)
            return grid;

        // Calculate the center of the asteroid's bounding box
        var asteroidCenterRow = (minRow + maxRow) / 2.0;
        var asteroidCenterCol = (minCol + maxCol) / 2.0;

        // Calculate the center of the grid
        var gridCenterRow = (gridSize - 1) / 2.0;
        var gridCenterCol = (gridSize - 1) / 2.0;

        // Calculate the offset needed to center the asteroid
        var offsetRow = (int)Math.Round(gridCenterRow - asteroidCenterRow);
        var offsetCol = (int)Math.Round(gridCenterCol - asteroidCenterCol);

        // Create a new centered grid
        var centeredGrid = new CellState[gridSize, gridSize];

        // Initialize all cells as empty
        for (var row = 0; row < gridSize; row++)
        for (var col = 0; col < gridSize; col++)
            centeredGrid[row, col] = new CellState
            {
                WallType = WallType.Empty,
                FloorType = FloorType.Empty,
                Layer = AsteroidLayer.None
            };

        // Copy the asteroid with the calculated offset
        for (var row = 0; row < gridSize; row++)
        for (var col = 0; col < gridSize; col++)
        {
            var newRow = row + offsetRow;
            var newCol = col + offsetCol;

            // Only copy if the destination is within bounds
            if (newRow >= 0 && newRow < gridSize && newCol >= 0 && newCol < gridSize)
                centeredGrid[newRow, newCol] = grid[row, col];
        }

        return centeredGrid;
    }

    /// <summary>
    ///     Gets the bounding box of asteroid within grid
    /// </summary>
    /// <param name="grid">The grid containing the asteroid to center</param>
    /// <returns>A new grid with the asteroid centered</returns>
    public static AsteroidBoundingBox GetAsteroidBoundingBoxWithinGrid(CellState[,] grid)
    {
        var gridSize = grid.GetLength(0);
        var minRow = gridSize;
        var maxRow = -1;
        var minCol = gridSize;
        var maxCol = -1;

        // Find the bounding box of the asteroid (non-empty cells)
        for (var row = 0; row < gridSize; row++)
        for (var col = 0; col < gridSize; col++)
            if (grid[row, col].FloorType != FloorType.Empty)
            {
                minRow = Math.Min(minRow, row);
                maxRow = Math.Max(maxRow, row);
                minCol = Math.Min(minCol, col);
                maxCol = Math.Max(maxCol, col);
            }

        return new AsteroidBoundingBox
        {
            X = minCol,
            Y = minRow,
            Width = maxCol - minCol + 1,
            Height = maxRow - minRow + 1,
            Right = maxCol,
            Bottom = maxRow
        };
    }
}