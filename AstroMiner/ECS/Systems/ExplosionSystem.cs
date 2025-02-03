using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Entities;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class ExplosionSystem : System
{
    public ExplosionSystem(World world, GameState gameState) : base(world, gameState)
    {
    }

    public int CreateExplosion(Vector2 position)
    {
        var entityId = World.CreateEntity();
        
        var positionComponent = World.AddComponent<PositionComponent>(entityId);
        positionComponent.Position = position;
        positionComponent.BoxSizePx = ExplosionComponent.BoxSizePx;
        
        World.AddComponent<ExplosionComponent>(entityId);
        World.AddComponent<ExplosionTag>(entityId);
        
        return entityId;
    }

    private void ExplodeGrid(Vector2 position)
    {
        var gridPos = ViewHelpers.ToGridPosition(position);
        var explodedCells = GetCellsInRadius(position.X, position.Y, ExplosionComponent.ExplodeRockRadius);
        
        foreach (var (x, y) in explodedCells)
        {
            // If x,y = gridPos, already triggered explosion
            if (GameState.AsteroidWorld.Grid.GetWallType(x, y) == WallType.ExplosiveRock && (x, y) != gridPos)
            {
                GameState.AsteroidWorld.Grid.ActivateExplosiveRockCell(x, y, 300);
            }
            else
            {
                GameState.AsteroidWorld.Grid.ClearWall(x, y);
                GameState.AsteroidWorld.Grid.ActivateCollapsingFloorCell(x, y);
            }
        }
    }

    private void CalculateEntityDamage(Vector2 explosionPosition, MiningControllableEntity entity)
    {
        var distance = Vector2.Distance(explosionPosition, entity.CenterPosition);

        if (distance < ExplosionComponent.ExplosionRadius)
        {
            var damagePercentage = 1f - distance / ExplosionComponent.ExplosionRadius;
            var damage = (int)(GameConfig.ExplosionMaxDamage * damagePercentage);
            entity.TakeDamage(damage);
        }
    }

    private List<(int x, int y)> GetCellsInRadius(float centerX, float centerY, float radius)
    {
        var cells = new List<(int x, int y)>();

        // Calculate the bounds to iterate over, based on the radius
        var startX = Math.Max(0, (int)Math.Floor(centerX - radius));
        var endX = Math.Min(GameState.AsteroidWorld.Grid.Columns - 1, (int)Math.Ceiling(centerX + radius));
        var startY = Math.Max(0, (int)Math.Floor(centerY - radius));
        var endY = Math.Min(GameState.AsteroidWorld.Grid.Rows - 1, (int)Math.Ceiling(centerY + radius));

        // Iterate through the grid cells within these bounds
        for (var i = startX; i <= endX; i++)
        for (var j = startY; j <= endY; j++)
        {
            // Calculate the distance from the center of the cell to the point
            var cellCenterX = i + 0.5f;
            var cellCenterY = j + 0.5f;
            var distance = (float)Math.Sqrt(Math.Pow(cellCenterX - centerX, 2) + Math.Pow(cellCenterY - centerY, 2));

            // If the distance is less than or equal to the radius, include the cell
            if (distance <= radius) cells.Add((i, j));
        }

        return cells;
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var explosionComponent in World.GetAllComponents<ExplosionComponent>())
        {
            var entityId = explosionComponent.EntityId;
            var positionComponent = World.GetComponent<PositionComponent>(entityId);

            if (!explosionComponent.HasExploded)
            {
                ExplodeGrid(positionComponent.Position);
                CalculateEntityDamage(positionComponent.Position, GameState.AsteroidWorld.Miner);
                if (!GameState.AsteroidWorld.IsInMiner)
                    CalculateEntityDamage(positionComponent.Position, GameState.AsteroidWorld.Player);
                explosionComponent.HasExploded = true;
            }
            else
            {
                explosionComponent.TimeSinceExplosionMs += gameTime.ElapsedGameTime.Milliseconds;
            }

            if (explosionComponent.TimeSinceExplosionMs >= ExplosionComponent.AnimationTimeMs)
            {
                World.DestroyEntity(entityId);
            }
        }
    }
} 