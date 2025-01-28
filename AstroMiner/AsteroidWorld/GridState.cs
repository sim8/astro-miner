#nullable enable
using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.AsteroidWorld;

public class CellState(WallType wallType, FloorType floorType, AsteroidLayer layer)
{
    public const int UninitializedOrAboveMax = -1;

    public readonly AsteroidLayer Layer = layer;

    /**
     * -1: uninitialized or above max distance
     * 0+ distance to floor with unbroken connection to edge
     */
    public int DistanceToExploredFloor = UninitializedOrAboveMax;

    public FloorType FloorType = floorType;

    public float FogOpacity = 1f; // Assume fog

    public WallType WallType = wallType;

    public bool isEmpty => WallType == WallType.Empty && FloorType == FloorType.Empty;
}

/// <summary>
///     Grid state is primarily 2d array of cell types.
///     Cells can be "activated" to be called in Update loop
/// </summary>
public class GridState(GameState gameState, CellState[,] grid)
{
    private static readonly CellState _outOfBoundsCell = new(WallType.Empty, FloorType.Empty, AsteroidLayer.None);

    private static readonly (int[], int[]) Neighbour8Offsets = (
        new[] { -1, 0, 1, 1, 1, 0, -1, -1 },
        new[] { -1, -1, -1, 0, 1, 1, 1, 0 }
    );

    private static readonly (int[], int[]) Neighbour4Offsets = (
        new[] { 0, 1, 0, -1 },
        new[] { -1, 0, 1, 0 }
    );

    // TODO nice way to combine these + other effects?
    // Would be nice if cell classes had a nice deactive method
    public readonly Dictionary<(int x, int y), ActiveCollapsingFloorCell> _activeCollapsingFloorCells = new();
    public readonly Dictionary<(int x, int y), ActiveExplosiveRockCell> _activeExplosiveRockCells = new();
    public int Columns => grid.GetLength(0);
    public int Rows => grid.GetLength(1);

    public void ActivateExplosiveRockCell(int x, int y, int timeToExplodeMs = 1100)
    {
        if (_activeExplosiveRockCells.ContainsKey((x, y)))
        {
            _activeExplosiveRockCells[(x, y)].TimeToExplodeMs =
                Math.Min(_activeExplosiveRockCells[(x, y)].TimeToExplodeMs, timeToExplodeMs);
            return;
        }

        _activeExplosiveRockCells.Add((x, y), new ActiveExplosiveRockCell(gameState, (x, y), timeToExplodeMs));
    }

    public void ActivateCollapsingFloorCell(int x, int y)
    {
        // Only spread if neighbor has empty wall
        if (_activeCollapsingFloorCells.ContainsKey((x, y)) || GetFloorType(x, y) != FloorType.LavaCracks ||
            GetWallType(x, y) != WallType.Empty) return;

        _activeCollapsingFloorCells.Add((x, y), new ActiveCollapsingFloorCell(gameState, (x, y)));
    }

    public void DeactivateExplosiveRockCell(int x, int y)
    {
        _activeExplosiveRockCells.Remove((x, y));
    }

    public void DeactiveCollapsingFloorCell(int x, int y)
    {
        _activeCollapsingFloorCells.Remove((x, y));
    }

    public bool ExplosiveRockCellIsActive(int x, int y)
    {
        return _activeExplosiveRockCells.ContainsKey((x, y));
    }


    public CellState GetCellState(Vector2 position)
    {
        var (x, y) = ViewHelpers.ToGridPosition(position);
        return GetCellState(x, y);
    }

    public CellState GetCellState(int x, int y)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            return _outOfBoundsCell;
        return grid[y, x];
    }

    public WallType GetWallType(int x, int y)
    {
        return GetCellState(x, y).WallType;
    }

    public FloorType GetFloorType(int x, int y)
    {
        return GetCellState(x, y).FloorType;
    }

    public WallTypeConfig? GetWallTypeConfig(int x, int y)
    {
        var wallType = GetCellState(x, y).WallType;
        return wallType != WallType.Empty ? WallTypes.GetConfig(wallType) : null;
    }

    public void MineWall(int x, int y, bool addToInventory = false)
    {
        var wallConfig = GetWallTypeConfig(x, y);
        if (wallConfig is not { IsMineable: true }) return;


        ClearWall(x, y);

        if (wallConfig.Drop.HasValue && addToInventory) gameState.Inventory.AddResource(wallConfig.Drop.Value);
    }

    public void ClearWall(int x, int y)
    {
        grid[y, x].WallType = WallType.Empty;
        DeactivateExplosiveRockCell(x, y);
        MarkAllDistancesFromExploredFloor(x, y);
    }

    public bool CheckNeighbors(int x, int y, Func<CellState, bool> cb)
    {
        var neighborPassesCheck = false;
        Map4Neighbors(x, y, (nx, ny) =>
        {
            if (cb(grid[ny, nx])) neighborPassesCheck = true;
        });
        return neighborPassesCheck;
    }

    public static void Map8Neighbors(int cx, int cy, Action<int, int> neighborAction)
    {
        MapNeighbors(Neighbour8Offsets, cx, cy, neighborAction);
    }

    public static void Map4Neighbors(int cx, int cy, Action<int, int> neighborAction)
    {
        MapNeighbors(Neighbour4Offsets, cx, cy, neighborAction);
    }

    private static void MapNeighbors((int[], int[]) neighborOffsets, int cx, int cy, Action<int, int> neighborAction)
    {
        var (neighborXOffsets, neighborYOffsets) = neighborOffsets;
        for (var i = 0; i < neighborXOffsets.Length; i++)
        {
            var nx = cx + neighborXOffsets[i];
            var ny = cy + neighborYOffsets[i];

            if (nx < 0 || nx >= GameConfig.GridSize || ny < 0 || ny >= GameConfig.GridSize)
                continue;

            neighborAction(nx, ny);
        }
    }

    private void UpdateCellDistanceToExploredFloor(int x, int y, int value, bool isInitializing)
    {
        var cellState = GetCellState(x, y);

        if (value < GameConfig.ShowGradientsAtDistance)
        {
            if (isInitializing)
                cellState.FogOpacity = 0f;
            else if (cellState.FogOpacity > 0f) gameState.Asteroid.FogAnimationManager.AddFadingCell(x, y);
        }


        cellState.DistanceToExploredFloor = value;
    }

    // Used for Fog of War. Recursively marks cells as either:
    //   - Explored floor (is connected by floor to the player's position)
    //   - Distance from explored floor, up to a max value
    // From the starting coordinates, run a BFS until all neighbours are both initialized
    // and have correct values relative to the current cell. This means it can run on an
    // uninitialized grid, as well as from a just-cleared cell during gameplay.
    public void MarkAllDistancesFromExploredFloor(int playerX, int playerY, bool isInitializing = false)
    {
        // The BFS assumes all cells in the queue have the correct distance. If running at 
        // startup (x,y == 0,0), assume it's an empty square (-1); otherwise a cell has been
        // cleared and is now floor (0)
        UpdateCellDistanceToExploredFloor(playerX, playerY, 0, isInitializing);

        Queue<(int x, int y)> queue = new();
        queue.Enqueue((playerX, playerY));

        // BFS
        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            var current = GetCellState(cx, cy);

            Map8Neighbors(cx, cy, (nx, ny) =>
            {
                var neighbour = GetCellState(nx, ny);

                // Set as connected if current distance is 0 and cell is floor or lava
                if (neighbour.WallType == WallType.Empty && neighbour.FloorType != FloorType.Empty &&
                    current.DistanceToExploredFloor == 0 &&
                    neighbour.DistanceToExploredFloor != 0)
                {
                    UpdateCellDistanceToExploredFloor(nx, ny, 0, isInitializing);
                    queue.Enqueue((nx, ny));
                }

                // Set distance from connected floor
                else if (current.DistanceToExploredFloor < GameConfig.MaxUnexploredCellsVisible + 1)
                {
                    var nextDistance = current.DistanceToExploredFloor + 1;
                    if (neighbour.DistanceToExploredFloor == CellState.UninitializedOrAboveMax ||
                        neighbour.DistanceToExploredFloor > nextDistance)
                    {
                        UpdateCellDistanceToExploredFloor(nx, ny, nextDistance, isInitializing);
                        queue.Enqueue((nx, ny));
                    }
                }
            });
        }
    }
}