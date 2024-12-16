using System;
using System.Collections.Generic;

namespace AstroMiner;

public class CellState(CellType type, bool hasLavaWell)
{
    public const int DISTANCE_UNINITIALIZED = -2;
    public const int DISTANCE_NA = -1;

    /**
     * -2: uninitislized
     * -1: N/A or above max distance
     * 0+ distance
     */
    public int distanceToOutsideConnectedFloor = DISTANCE_UNINITIALIZED;

    public bool hasLavaWell = hasLavaWell;
    public CellType type = type;
}

public class GridState(GameState gameState, CellState[,] grid)
{
    private static readonly int[] neighbourXOffsets = { -1, 0, 1, 1, 1, 0, -1, -1 };
    private static readonly int[] neighbourYOffsets = { -1, -1, -1, 0, 1, 1, 1, 0 };
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

        MarkAllDistancesFromOutsideConnectedFloors(x, y);
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

    private static void MapNeighbors(int cx, int cy, Action<int, int> neighborAction)
    {
        for (var i = 0; i < neighbourXOffsets.Length; i++)
        {
            var nx = cx + neighbourXOffsets[i];
            var ny = cy + neighbourYOffsets[i];

            if (nx < 0 || nx >= GameConfig.GridSize || ny < 0 || ny >= GameConfig.GridSize)
                continue;

            neighborAction(nx, ny);
        }
    }

    // For the entire grid, either initializes distanceToOutsideConnectedFloor or ensures current values are correct.
    // From the starting coordinates, run a BFS until all neighbours have correct values relative to the current cell.
    // This means it can run on an uninitialized grid, as well as from a just-cleared cell during gameplay.
    // Assumes that:
    // - The asteroid has >= 1 floor piece on its perimeter
    // - Top left cell of grid is empty
    public void MarkAllDistancesFromOutsideConnectedFloors(int x = 0, int y = 0)
    {
        // The BFS assumes all cells in the queue have the correct distance. If running at 
        // startup (x,y == 0,0), assume it's an empty square (-1); otherwise a cell has been
        // cleared and is now floor (0)
        grid[y, x].distanceToOutsideConnectedFloor = x == 0 && y == 0 ? CellState.DISTANCE_NA : 0;

        Queue<(int x, int y)> queue = new();
        queue.Enqueue((x, y));

        // BFS
        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            var current = GetCellState(cx, cy);

            MapNeighbors(cx, cy, (nx, ny) =>
            {
                var neighbour = GetCellState(nx, ny);

                // Always initialize empty cells
                if (neighbour.type == CellType.Empty &&
                    neighbour.distanceToOutsideConnectedFloor == CellState.DISTANCE_UNINITIALIZED)
                {
                    neighbour.distanceToOutsideConnectedFloor = CellState.DISTANCE_NA;
                    queue.Enqueue((nx, ny));
                }

                // Set floor to outside connected if it adjoins with an edge piece or another outside connected floor
                else if (neighbour.type == CellType.Floor &&
                         (current.type == CellType.Empty || current.distanceToOutsideConnectedFloor == 0) &&
                         neighbour.distanceToOutsideConnectedFloor != 0)
                {
                    neighbour.distanceToOutsideConnectedFloor = 0;
                    queue.Enqueue((nx, ny));
                }
                // Set distance from connected floor. Skipped if current piece is empty,
                // and by this check current has to be either solid or an unconnected floor
                else if (neighbour.type != CellType.Empty && current.type != CellType.Empty &&
                         current.distanceToOutsideConnectedFloor <
                         GameConfig.MaxUnexploredCellsVisible)
                {
                    var nextDistance = current.distanceToOutsideConnectedFloor + 1;
                    if (neighbour.distanceToOutsideConnectedFloor == CellState.DISTANCE_UNINITIALIZED ||
                        neighbour.distanceToOutsideConnectedFloor >
                        nextDistance)
                    {
                        neighbour.distanceToOutsideConnectedFloor = nextDistance;
                        queue.Enqueue((nx, ny));
                    }
                }
            });
        }
    }
}