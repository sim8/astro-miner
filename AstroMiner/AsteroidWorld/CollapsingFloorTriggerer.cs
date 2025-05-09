using System;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.AsteroidWorld;

public class CollapsingFloorTriggerer(BaseGame game)
{
    private const int TriggerIntervalMs = 2222; // Interval to trigger MaybeCollapseFloors in milliseconds
    private const int Distance = 5;
    private readonly Random _random = new();
    private int _elapsedTimeAccumulator; // Accumulated elapsed time in milliseconds
    private int CollapseChance = 150;


    private void MaybeCollapseFloors()
    {
        var (playerX, playerY) =
            ViewHelpers.ToGridPosition(game.StateManager.Ecs.ActiveControllableEntityCenterPosition);

        // Iterate through cells within 10 rows/columns of player
        for (var y = Math.Max(0, playerY - Distance); y <= playerY + Distance; y++)
        for (var x = Math.Max(0, playerX - Distance); x <= playerX + Distance; x++)
            if (game.StateManager.AsteroidWorld.Grid.GetFloorType(x, y) == FloorType.LavaCracks &&
                _random.Next(150) == 0)
            {
                // Only collapse if there's a lava neighbor
                var hasLavaNeighbor =
                    game.StateManager.AsteroidWorld.Grid.CheckNeighbors(x, y, cell => cell.FloorType == FloorType.Lava);
                if (hasLavaNeighbor)
                    game.StateManager.AsteroidWorld.Grid.ActivateCollapsingFloorCell(x, y);
            }
    }

    public void Update(GameTime gameTime)
    {
        _elapsedTimeAccumulator += gameTime.ElapsedGameTime.Milliseconds;

        // Check if the interval has been reached or exceeded
        if (_elapsedTimeAccumulator >= TriggerIntervalMs)
        {
            MaybeCollapseFloors();
            _elapsedTimeAccumulator = 0; // Reset to 0
        }
    }
}