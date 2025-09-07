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
        var explodedCells = GetCellsInRadius(position.X, position.Y, ExplodeRockRadius);

        foreach (var (x, y, distance) in explodedCells)
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

    // TODO move elsehwere. Can be static + use GameConfig.GridSize
    public List<(int x, int y, float distance)> GetCellsInRadius(float centerX, float centerY, float radius)
    {
        var cells = new List<(int x, int y, float distance)>();

        // Calculate the bounds to iterate over, based on the radius
        var startX = Math.Max(0, (int)Math.Floor(centerX - radius));
        var endX = Math.Min(game.StateManager.AsteroidWorld.Grid.Columns - 1, (int)Math.Ceiling(centerX + radius));
        var startY = Math.Max(0, (int)Math.Floor(centerY - radius));
        var endY = Math.Min(game.StateManager.AsteroidWorld.Grid.Rows - 1, (int)Math.Ceiling(centerY + radius));

        // Iterate through the grid cells within these bounds
        for (var i = startX; i <= endX; i++)
        for (var j = startY; j <= endY; j++)
        {
            // Calculate the distance from the center of the cell to the point
            var cellCenterX = i + 0.5f;
            var cellCenterY = j + 0.5f;
            var distance = (float)Math.Sqrt(Math.Pow(cellCenterX - centerX, 2) + Math.Pow(cellCenterY - centerY, 2));

            // If the distance is less than or equal to the radius, include the cell
            if (distance <= radius) cells.Add((i, j, distance));
        }

        return cells;
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