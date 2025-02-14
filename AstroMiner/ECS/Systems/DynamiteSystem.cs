using System.Collections.Generic;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class DynamiteSystem : System
{
    private const int FuseTimeMs = 4000;
    private const int BoxSizePx = 4;

    public DynamiteSystem(Ecs ecs, GameState gameState) : base(ecs, gameState)
    {
    }

    public void PlaceDynamite()
    {
        if (GameState.Inventory.numDynamite <= 0) return;
        if (Ecs.PlayerEntityId == null) return;

        var playerPositionComponent = Ecs.GetComponent<PositionComponent>(Ecs.PlayerEntityId.Value);
        if (playerPositionComponent == null) return;

        GameState.Inventory.numDynamite--;
        var dynamiteEntity = Ecs.Factories.CreateDynamiteEntity(playerPositionComponent.CenterPosition);
        var dynamitePositionComponent = Ecs.GetComponent<PositionComponent>(dynamiteEntity);
        Ecs.MovementSystem.SetPositionRelativeToDirectionalEntity(dynamitePositionComponent, Ecs.PlayerEntityId.Value, Direction.Top);
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)

    {
        if (activeControls.Contains(MiningControls.PlaceDynamite) && !GameState.AsteroidWorld.IsInMiner)
        {
            PlaceDynamite();
        }
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