using System;
using System.Collections.Generic;

namespace AstroMiner;

public class CellState(CellType type)
{
    public const int DistanceUninitializedOrAboveMax = -2;
    public const int DistanceEmpty = -1;

    /**
     * -2: uninitialized or above max distance
     * -1: N/A (empty piece)
     * 0+ distance to floor with unbroken connection to edge
     */
    public int DistanceToOutsideConnectedFloor = DistanceUninitializedOrAboveMax;

    public CellType Type = type;
}

public class GridState(GameState gameState, CellState[,] grid)
{
    private static readonly int[] NeighbourXOffsets = { -1, 0, 1, 1, 1, 0, -1, -1 };
    private static readonly int[] NeighbourYOffsets = { -1, -1, -1, 0, 1, 1, 1, 0 };
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
        return GetCellState(x, y).Type;
    }

    public CellTypeConfig GetCellConfig(int x, int y)
    {
        return CellTypes.GetConfig(GetCellState(x, y).Type);
    }

    public void DemolishCell(int x, int y, bool addToInventory = false)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        var cellConfig = GetCellConfig(x, y);

        if (!cellConfig.IsDestructible) return;

        if (cellConfig is MineableCellConfig mineableConfig)
        {
            var drop = mineableConfig.Drop;
            if (drop.HasValue) gameState.Inventory.AddResource(drop.Value);
        }

        grid[y, x].Type = CellType.Floor;

        MarkAllDistancesFromOutsideConnectedFloors(x, y);
    }

    public bool CellHasNeighbourOfType(int x, int y, CellType cellType)
    {
        var hasNeighbourOfType = false;
        MapNeighbors(x, y, (nx, ny) =>
        {
            if (grid[ny, nx].Type == cellType) hasNeighbourOfType = true;
        });
        return hasNeighbourOfType;
    }

    private static void MapNeighbors(int cx, int cy, Action<int, int> neighborAction)
    {
        for (var i = 0; i < NeighbourXOffsets.Length; i++)
        {
            var nx = cx + NeighbourXOffsets[i];
            var ny = cy + NeighbourYOffsets[i];

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
        grid[y, x].DistanceToOutsideConnectedFloor = x == 0 && y == 0 ? CellState.DistanceEmpty : 0;

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
                if (neighbour.Type == CellType.Empty &&
                    neighbour.DistanceToOutsideConnectedFloor == CellState.DistanceUninitializedOrAboveMax)
                {
                    neighbour.DistanceToOutsideConnectedFloor = CellState.DistanceEmpty;
                    queue.Enqueue((nx, ny));
                }
                // Set floor to outside connected if it adjoins with an edge piece or another outside connected floor
                else if (!CellTypes.GetConfig(neighbour.Type).IsCollideable && neighbour.Type != CellType.Empty &&
                         (current.Type == CellType.Empty || current.DistanceToOutsideConnectedFloor == 0) &&
                         neighbour.DistanceToOutsideConnectedFloor != 0)
                {
                    neighbour.DistanceToOutsideConnectedFloor = 0;
                    queue.Enqueue((nx, ny));
                }
                // Set distance from connected floor. Skipped if current piece is empty,
                // and by this check current has to be either solid or an unconnected floor
                else if (neighbour.Type != CellType.Empty && current.Type != CellType.Empty &&
                         current.DistanceToOutsideConnectedFloor <
                         GameConfig.MaxUnexploredCellsVisible +
                         1) // add one to get gradients for furthest visible cells
                {
                    var nextDistance = current.DistanceToOutsideConnectedFloor + 1;
                    if (neighbour.DistanceToOutsideConnectedFloor == CellState.DistanceUninitializedOrAboveMax ||
                        neighbour.DistanceToOutsideConnectedFloor >
                        nextDistance)
                    {
                        neighbour.DistanceToOutsideConnectedFloor = nextDistance;
                        queue.Enqueue((nx, ny));
                    }
                }
            });
        }
    }
}