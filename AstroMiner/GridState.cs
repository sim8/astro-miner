using System;
using System.Collections.Generic;

namespace AstroMiner;

public struct CellState(CellType type, bool hasLavaWell)
{
    public CellType type = type;
    public bool hasLavaWell = hasLavaWell;
    public int distanceToOutsideConnectedFloor = -1; // Initialized separately. -1 means unknown distance
}

public class GridState(GameState gameState, CellState[,] grid)
{
    public int Columns => grid.GetLength(0);
    public int Rows => grid.GetLength(1);

    public CellState GetCellState(int x, int y)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        return grid[y, x];
    }

    public CellType GetCellType(int x, int y)
    {
        return GetCellState(x, y).type;
    }

    public void DemolishCell(int x, int y, bool addToInventory = false)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        if (grid[y, x].type == CellType.Empty) return;

        if (grid[y, x].type == CellType.Ruby && addToInventory) gameState.Inventory.NumRubies++;
        if (grid[y, x].type == CellType.Diamond && addToInventory) gameState.Inventory.NumDiamonds++;

        grid[y, x].type = CellType.Floor;
    }

    public bool CellHasNeighbourOfType(int x, int y, CellType cellType)
    {
        int[] xOffsets = { -1, 0, 1, 1, 1, 0, -1, -1 };
        int[] yOffsets = { -1, -1, -1, 0, 1, 1, 1, 0 };

        for (var i = 0; i < xOffsets.Length; i++)
        {
            var newX = x + xOffsets[i];
            var newY = y + yOffsets[i];

            if (ViewHelpers.IsValidGridPosition(newX, newY) && grid[newY, newX].type == cellType) return true;
        }

        return false;
    }


    public void MarkOutsideConnectedFloors()
    {
        // We'll use a BFS from all edge empty cells.
        Queue<(int x, int y)> queue = new();
        var visited = new bool[GameConfig.GridSize, GameConfig.GridSize];

        // Enqueue all edge cells that are Empty
        // Top and bottom rows
        for (var x = 0; x < GameConfig.GridSize; x++)
        {
            if (grid[x, 0].type == CellType.Empty) continue;
            {
                queue.Enqueue((x, 0));
                visited[x, 0] = true;
            }
            if (grid[x, GameConfig.GridSize - 1].type == CellType.Empty)
            {
                queue.Enqueue((x, GameConfig.GridSize - 1));
                visited[x, GameConfig.GridSize - 1] = true;
            }
        }

        // Left and right columns
        for (var y = 0; y < GameConfig.GridSize; y++)
        {
            if (grid[0, y].type == CellType.Empty && !visited[0, y])
            {
                queue.Enqueue((0, y));
                visited[0, y] = true;
            }

            if (grid[GameConfig.GridSize - 1, y].type == CellType.Empty && !visited[GameConfig.GridSize - 1, y])
            {
                queue.Enqueue((GameConfig.GridSize - 1, y));
                visited[GameConfig.GridSize - 1, y] = true;
            }
        }

        // Directions for 4-way movement (up, down, left, right)
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        // BFS
        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();

            // If this cell is a floor cell, mark it as outside-connected
            if (grid[cx, cy].type == CellType.Floor) grid[cx, cy].distanceToOutsideConnectedFloor = 0;

            // Check neighbors
            for (var i = 0; i < 4; i++)
            {
                var nx = cx + dx[i];
                var ny = cy + dy[i];

                if (nx < 0 || nx >= GameConfig.GridSize || ny < 0 || ny >= GameConfig.GridSize)
                    continue; // Out of bounds

                if (visited[nx, ny])
                    continue; // Already visited

                // We only continue BFS through Empty or Floor cells
                if (grid[nx, ny].type == CellType.Empty || grid[nx, ny].type == CellType.Floor)
                {
                    visited[nx, ny] = true;
                    queue.Enqueue((nx, ny));
                }
            }
        }

        // Now isOutsideConnectedFloor[x, y] is true for all floor cells connected to the map edge via empty/floor cells.
    }

    public void ComputeDistancesToOutsideConnectedFloor(int maxDistance = int.MaxValue)
    {
        Queue<(int x, int y)> queue = new();

        // Enqueue all cells that are already known to be outside-connected floor
        // i.e. distanceToOutsideConnectedFloor == 0
        for (var x = 0; x < GameConfig.GridSize; x++)
        for (var y = 0; y < GameConfig.GridSize; y++)
            if (grid[x, y].distanceToOutsideConnectedFloor == 0)
                queue.Enqueue((x, y));

        int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
        int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            var currentDistance = grid[cx, cy].distanceToOutsideConnectedFloor;

            // If we've reached the maxDistance cap, don't spread further
            if (currentDistance >= maxDistance)
                continue;

            for (var i = 0; i < dx.Length; i++)
            {
                var nx = cx + dx[i];
                var ny = cy + dy[i];

                if (nx < 0 || nx >= GameConfig.GridSize || ny < 0 || ny >= GameConfig.GridSize)
                    continue;

                // Only proceed if this cell is not empty and has not been assigned a distance yet
                if (grid[nx, ny].type != CellType.Empty
                    && grid[nx, ny].distanceToOutsideConnectedFloor == -1)
                {
                    grid[nx, ny].distanceToOutsideConnectedFloor = currentDistance + 1;
                    queue.Enqueue((nx, ny));
                }
            }
        }
    }
}