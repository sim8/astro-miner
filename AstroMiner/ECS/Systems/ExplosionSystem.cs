using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class ExplosionSystem : System
{
    // Animation state
    public const int AnimationTimeMs = 400;

    // Effect properties
    public const float ExplodeRockRadius = 2.2f;
    public const float ExplosionRadius = 4f;
    public const int BoxSizePx = 1;

    public ExplosionSystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }

    private void ExplodeGrid(Vector2 position)
    {
        var gridPos = ViewHelpers.ToGridPosition(position);
        var explodedCells = AsteroidGridHelpers.GetCellsInRadius(position.X, position.Y, ExplodeRockRadius);
        foreach (var (x, y, _) in explodedCells)
            // If x,y = gridPos, already triggered explosion
            if (game.StateManager.AsteroidWorld.Grid.GetWallType(x, y) == WallType.ExplosiveRock && (x, y) != gridPos)
            {
                game.StateManager.AsteroidWorld.Grid.ActivateExplosiveRockCell(x, y, 300);
            }
            else
            {
                game.StateManager.AsteroidWorld.Grid.ClearWall(x, y);
                game.StateManager.AsteroidWorld.Grid.ActivateCollapsingFloorCell(x, y);
            }
    }

    private void CalculateEntityDamage(Vector2 explosionPosition)
    {
        foreach (var healthComponent in Ecs.GetAllComponents<HealthComponent>())
        {
            if (healthComponent.EntityId == Ecs.PlayerEntityId && game.StateManager.AsteroidWorld.IsInMiner) continue;

            if (healthComponent.IsDead) continue;

            var positionComponent = Ecs.GetComponent<PositionComponent>(healthComponent.EntityId);
            if (positionComponent == null) continue;

            var distance = Vector2.Distance(explosionPosition, positionComponent.CenterPosition);

            if (distance < ExplosionRadius)
            {
                var damagePercentage = 1f - distance / ExplosionRadius;
                var damage = (int)(GameConfig.ExplosionMaxDamage * damagePercentage);
                game.StateManager.Ecs.HealthSystem.TakeDamage(healthComponent.EntityId, damage);
            }
        }
    }

    public override void Update(GameTime gameTime, ActiveControls activeControls)
    {
        if (game.Model.ActiveWorld != World.Asteroid) return;

        foreach (var explosionComponent in Ecs.GetAllComponents<ExplosionComponent>())
        {
            var entityId = explosionComponent.EntityId;
            var positionComponent = Ecs.GetComponent<PositionComponent>(entityId);

            if (!explosionComponent.HasExploded)
            {
                ExplodeGrid(positionComponent.Position);
                CalculateEntityDamage(positionComponent.Position);
                explosionComponent.HasExploded = true;
            }
            else
            {
                explosionComponent.TimeSinceExplosionMs += gameTime.ElapsedGameTime.Milliseconds;
            }

            if (explosionComponent.TimeSinceExplosionMs >= AnimationTimeMs) Ecs.DestroyEntity(entityId);
        }
    }
}