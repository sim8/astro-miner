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

    public DynamiteSystem(Ecs ecs, GameState gameState) : base(ecs, gameState)
    {
    }

    public int CreateDynamite(Vector2 position)
    {
        return Ecs.Factories.CreateDynamiteEntity(position);
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        foreach (var fuseComponent in Ecs.GetAllComponents<FuseComponent>())
        {
            fuseComponent.TimeToExplodeMs -= gameTime.ElapsedGameTime.Milliseconds;

            if (fuseComponent.TimeToExplodeMs <= 0)
            {
                var positionComponent = Ecs.GetComponent<PositionComponent>(fuseComponent.EntityId);
                GameState.Ecs.Factories.CreateExplosionEntity(positionComponent.CenterPosition);
                Ecs.DestroyEntity(fuseComponent.EntityId);
            }
        }
    }
}