using System;
using System.Collections.Generic;
using AstroMiner.ECS.Components;
using AstroMiner.Entities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class DynamiteSystem : System
{
    private const int FuseTimeMs = 4000;
    private const int BoxSizePx = 4;

    public DynamiteSystem(World world, GameState gameState) : base(world, gameState)
    {
    }

    public int CreateDynamite(Vector2 position)
    {
        var entityId = World.CreateEntity();
        
        var positionComponent = World.AddComponent<PositionComponent>(entityId);
        positionComponent.Position = position;
        positionComponent.BoxSizePx = BoxSizePx;
        
        var fuseComponent = World.AddComponent<FuseComponent>(entityId);
        fuseComponent.MaxFuseTimeMs = FuseTimeMs;
        fuseComponent.TimeToExplodeMs = FuseTimeMs;
        
        // Add tag component for identification
        World.AddComponent<DynamiteTag>(entityId);
        
        return entityId;
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        foreach (var fuseComponent in World.GetAllComponents<FuseComponent>())
        {
            fuseComponent.TimeToExplodeMs -= gameTime.ElapsedGameTime.Milliseconds;
            
            if (fuseComponent.TimeToExplodeMs <= 0)
            {
                var positionComponent = World.GetComponent<PositionComponent>(fuseComponent.EntityId);
                GameState.ExplosionSystem.CreateExplosion(positionComponent.CenterPosition);
                World.DestroyEntity(fuseComponent.EntityId);
            }
        }
    }
} 