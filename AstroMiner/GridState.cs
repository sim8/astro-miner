using System;
using System.Collections.Generic;

namespace AstroMiner;

public class CellState(CellType type)
{
    public const int UninitializedOrAboveMax = -1;

    /**
     * -2: uninitialized or above max distance
     * -1: N/A (empty piece)
     * 0+ distance to floor with unbroken connection to edge
     */
    public int DistanceToExploredFloor = UninitializedOrAboveMax;

    public CellType Type = type;
}

/// <summary>
///     Grid state is primarily 2d array of cell types.
///     Cells can be "activated" to be called in Update loop
/// </summary>
public class GridState(GameState gameState, CellState[,] grid)
{
    private static readonly int[] NeighbourXOffsets = { -1, 0, 1, 1, 1, 0, -1, -1 };

    private static readonly int[] NeighbourYOffsets = { -1, -1, -1, 0, 1, 1, 1, 0 };

    // In future could be used for any active cell
    public readonly Dictionary<(int x, int y), ActiveExplosiveRockCell> _activeExplosiveRockCells = new();
    public int Columns => grid.GetLength(0);
    public int Rows => grid.GetLength(1);

    public void ActivateExplosiveRockCell(int x, int y, int timeToExplodeMs = 2000)
    {
        if (_activeExplosiveRockCells.ContainsKey((x, y)))
        {
            _activeExplosiveRockCells[(x, y)].TimeToExplodeMs =
                Math.Min(_activeExplosiveRockCells[(x, y)].TimeToExplodeMs, timeToExplodeMs);
            return;
        }

        _activeExplosiveRockCells.Add((x, y), new ActiveExplosiveRockCell(gameState, (x, y), timeToExplodeMs));
    }

    public void DeactivateExplosiveRockCell(int x, int y)
    {
        _activeExplosiveRockCells.Remove((x, y));
    }

    public bool ExplosiveRockCellIsActive(int x, int y)
    {
        return _activeExplosiveRockCells.ContainsKey((x, y));
    }

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

    public void MineCell(int x, int y, bool addToInventory = false)
    {
        var cellConfig = GetCellConfig(x, y);
        if (!cellConfig.IsDestructible) return;

        ClearCell(x, y);

        if (cellConfig is MineableCellConfig mineableConfig && addToInventory)
        {
            var drop = mineableConfig.Drop;
            if (drop.HasValue) gameState.Inventory.AddResource(drop.Value);
        }
    }

    public void ClearCell(int x, int y)
    {
        grid[y, x].Type = CellType.Floor;
        DeactivateExplosiveRockCell(x, y);
        MarkAllDistancesFromExploredFloor(x, y);

        MapNeighbors(x, y, (nx, ny) =>
        {
            if (grid[ny, nx].Type == CellType.ExplosiveRock) ActivateExplosiveRockCell(nx, ny);
        });
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

    // Used for Fog of War. Recursively marks cells as either:
    //   - Explored floor (is connected by floor to the player's position)
    //   - Distance from explored floor, up to a max value
    // From the starting coordinates, run a BFS until all neighbours are both initialized
    // and have correct values relative to the current cell. This means it can run on an
    // uninitialized grid, as well as from a just-cleared cell during gameplay.
    public void MarkAllDistancesFromExploredFloor(int playerX, int playerY)
    {
        // The BFS assumes all cells in the queue have the correct distance. If running at 
        // startup (x,y == 0,0), assume it's an empty square (-1); otherwise a cell has been
        // cleared and is now floor (0)
        grid[playerY, playerX].DistanceToExploredFloor = 0;

        Queue<(int x, int y)> queue = new();
        queue.Enqueue((playerX, playerY));

        // BFS
        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            var current = GetCellState(cx, cy);

            MapNeighbors(cx, cy, (nx, ny) =>
            {
                var neighbour = GetCellState(nx, ny);

                // Set as connected if current distance is 0 and cell is floor or lava
                if (!CellTypes.GetConfig(neighbour.Type).IsCollideable && neighbour.Type != CellType.Empty &&
                    current.DistanceToExploredFloor == 0 &&
                    neighbour.DistanceToExploredFloor != 0)
                {
                    neighbour.DistanceToExploredFloor = 0;
                    queue.Enqueue((nx, ny));
                }

                // Set distance from connected floor
                else if (current.DistanceToExploredFloor < GameConfig.MaxUnexploredCellsVisible + 1)
                {
                    var nextDistance = current.DistanceToExploredFloor + 1;
                    if (neighbour.DistanceToExploredFloor == CellState.UninitializedOrAboveMax ||
                        neighbour.DistanceToExploredFloor >
                        nextDistance)
                    {
                        neighbour.DistanceToExploredFloor = nextDistance;
                        queue.Enqueue((nx, ny));
                    }
                }
            });
        }
    }
}