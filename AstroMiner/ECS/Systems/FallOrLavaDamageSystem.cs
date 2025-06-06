using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class FallOrLavaDamageSystem : System
{
    public FallOrLavaDamageSystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        if (game.Model.ActiveWorld != World.Asteroid) return;

        foreach (var healthComponent in Ecs.GetAllComponents<HealthComponent>())
        {
            if (healthComponent.IsDead) continue;

            var positionComponent = Ecs.GetComponent<PositionComponent>(healthComponent.EntityId);
            if (positionComponent == null) continue;

            if (healthComponent.EntityId == Ecs.PlayerEntityId && game.StateManager.AsteroidWorld.IsInMiner) continue;

            var (topLeftX, topLeftY) = ViewHelpers.ToGridPosition(positionComponent.Position);
            var (bottomRightX, bottomRightY) = ViewHelpers.ToGridPosition(positionComponent.Position +
                                                                          new Vector2(positionComponent.GridWidth,
                                                                              positionComponent.GridHeight));

            var allCellsAreEmpty = true;
            var someCellsAreLava = false;

            for (var x = topLeftX; x <= bottomRightX; x++)
            for (var y = topLeftY; y <= bottomRightY; y++)
            {
                var floorType = game.StateManager.AsteroidWorld.Grid.GetFloorType(x, y);
                if (floorType != FloorType.Empty) allCellsAreEmpty = false;
                if (floorType == FloorType.Lava) someCellsAreLava = true;
            }

            healthComponent.IsOnLava = someCellsAreLava;

            if (someCellsAreLava)
            {
                healthComponent.TimeOnLavaMs += gameTime.ElapsedGameTime.Milliseconds;
                if (healthComponent.TimeOnLavaMs >= GameConfig.LavaDamageDelayMs)
                    game.StateManager.Ecs.HealthSystem.TakeDamage(healthComponent.EntityId,
                        (float)GameConfig.LavaDamagePerSecond / 1000 * gameTime.ElapsedGameTime.Milliseconds);
            }
            else if (healthComponent.TimeOnLavaMs > 0)
            {
                healthComponent.TimeOnLavaMs =
                    Math.Max(0, healthComponent.TimeOnLavaMs - gameTime.ElapsedGameTime.Milliseconds);
            }

            if (allCellsAreEmpty) positionComponent.IsOffAsteroid = true;
        }
    }
}