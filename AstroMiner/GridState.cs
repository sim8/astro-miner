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

    // Used to determine which gradient to use on the overlay (creates effect
    // of fading darkness towards unexplored cells)
    public int GradientKey;

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

    public void DemolishCell(int x, int y, bool addToInventory = false)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        if (grid[y, x].Type == CellType.Empty) return;

        if (grid[y, x].Type == CellType.Ruby && addToInventory) gameState.Inventory.NumRubies++;
        if (grid[y, x].Type == CellType.Diamond && addToInventory) gameState.Inventory.NumDiamonds++;

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

        // Update gradients after distanceToOutsideConnectedFloor's stable
        HashSet<(int x, int y)> updateGradientsFor = new();

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
                else if ((neighbour.type == CellType.Floor || neighbour.type == CellType.Lava) &&
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
                        updateGradientsFor.Add((nx, ny));

                        queue.Enqueue((nx, ny));
                    }
                }
            });
        }

        foreach (var (cx, cy) in updateGradientsFor)
        {
            UpdateGradientKey(cx, cy);
            MapNeighbors(cx, cy, UpdateGradientKey);
        }
    }

    // Updates the gradient for a cell's overlay based on the distance of cells surrounding it
    private void UpdateGradientKey(int x, int y)
    {
        var currentCell = GetCellState(x, y);
        var currentDistance = currentCell.DistanceToOutsideConnectedFloor;

        var corners = new List<Corner>(4);

        // Pre-fetch neighbors for TopLeft checks
        var nTl1 = GetCellState(x - 1, y);
        var nTl2 = GetCellState(x - 1, y - 1);
        var nTl3 = GetCellState(x, y - 1);

        if ((nTl1 != null && nTl1.DistanceToOutsideConnectedFloor > currentDistance) ||
            (nTl2 != null && nTl2.DistanceToOutsideConnectedFloor > currentDistance) ||
            (nTl3 != null && nTl3.DistanceToOutsideConnectedFloor > currentDistance))
            corners.Add(Corner.TopLeft);

        // TopRight checks
        var nTr1 = GetCellState(x + 1, y);
        var nTr2 = GetCellState(x + 1, y - 1);
        var nTr3 = GetCellState(x, y - 1);

        if ((nTr1 != null && nTr1.DistanceToOutsideConnectedFloor > currentDistance) ||
            (nTr2 != null && nTr2.DistanceToOutsideConnectedFloor > currentDistance) ||
            (nTr3 != null && nTr3.DistanceToOutsideConnectedFloor > currentDistance))
            corners.Add(Corner.TopRight);

        // BottomLeft checks
        var nBl1 = GetCellState(x - 1, y);
        var nBl2 = GetCellState(x - 1, y + 1);
        var nBl3 = GetCellState(x, y + 1);

        if ((nBl1 != null && nBl1.DistanceToOutsideConnectedFloor > currentDistance) ||
            (nBl2 != null && nBl2.DistanceToOutsideConnectedFloor > currentDistance) ||
            (nBl3 != null && nBl3.DistanceToOutsideConnectedFloor > currentDistance))
            corners.Add(Corner.BottomLeft);

        // BottomRight checks
        var nBr1 = GetCellState(x + 1, y);
        var nBr2 = GetCellState(x + 1, y + 1);
        var nBr3 = GetCellState(x, y + 1);

        if ((nBr1 != null && nBr1.DistanceToOutsideConnectedFloor > currentDistance) ||
            (nBr2 != null && nBr2.DistanceToOutsideConnectedFloor > currentDistance) ||
            (nBr3 != null && nBr3.DistanceToOutsideConnectedFloor > currentDistance))
            corners.Add(Corner.BottomRight);

        currentCell.GradientKey = RampKeys.CreateKey(corners.ToArray());
    }
}