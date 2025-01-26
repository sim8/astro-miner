using System;
using AstroMiner.Definitions;
using AstroMiner.Utilities;

namespace AstroMiner;

public class CollapsingFloorTriggerer(GameState gameState)
{
    private const int TriggerIntervalMs = 2222; // Interval to trigger MaybeCollapseFloors in milliseconds
    private const int Distance = 5;
    private readonly Random _random = new();
    private int _elapsedTimeAccumulator; // Accumulated elapsed time in milliseconds
    private int CollapseChance = 150;


    private void MaybeCollapseFloors()
    {
        var (playerX, playerY) = ViewHelpers.ToGridPosition(gameState.Asteroid.ActiveControllableEntity.Position);

        // Iterate through cells within 10 rows/columns of player
        for (var y = Math.Max(0, playerY - Distance); y <= playerY + Distance; y++)
        for (var x = Math.Max(0, playerX - Distance); x <= playerX + Distance; x++)
            if (gameState.Asteroid.Grid.GetFloorType(x, y) == FloorType.LavaCracks && _random.Next(150) == 0)
            {
                // Only collapse if there's a lava neighbor
                var hasLavaNeighbor =
                    gameState.Asteroid.Grid.CheckNeighbors(x, y, cell => cell.FloorType == FloorType.Lava);
                if (hasLavaNeighbor)
                    gameState.Asteroid.Grid.ActivateCollapsingFloorCell(x, y);
            }
    }

    public void Update(int elapsedMs)
    {
        _elapsedTimeAccumulator += elapsedMs;

        // Check if the interval has been reached or exceeded
        if (_elapsedTimeAccumulator >= TriggerIntervalMs)
        {
            MaybeCollapseFloors();
            _elapsedTimeAccumulator = 0; // Reset to 0
        }
    }
}