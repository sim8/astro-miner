using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class DynamiteSystem : System
{
    public DynamiteSystem(Ecs ecs, AstroMinerGame game) : base(ecs, game)
    {
    }

    public void PlaceDynamite()
    {
        if (game.State.Inventory.numDynamite <= 0) return;
        if (Ecs.PlayerEntityId == null) return;

        var playerPositionComponent = Ecs.GetComponent<PositionComponent>(Ecs.PlayerEntityId.Value);
        if (playerPositionComponent == null) return;

        game.State.Inventory.numDynamite--;
        var dynamiteEntity = Ecs.Factories.CreateDynamiteEntity(playerPositionComponent.CenterPosition);
        var dynamitePositionComponent = Ecs.GetComponent<PositionComponent>(dynamiteEntity);
        Ecs.MovementSystem.SetPositionRelativeToDirectionalEntity(dynamitePositionComponent, Ecs.PlayerEntityId.Value,
            Direction.Top);
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)

    {
        if (game.State.ActiveWorld != World.Asteroid) return;

        if (activeControls.Contains(MiningControls.PlaceDynamite) && !game.State.AsteroidWorld.IsInMiner)
            PlaceDynamite();
        foreach (var fuseComponent in Ecs.GetAllComponents<FuseComponent>())
        {
            fuseComponent.TimeToExplodeMs -= gameTime.ElapsedGameTime.Milliseconds;

            if (fuseComponent.TimeToExplodeMs <= 0)
            {
                var positionComponent = Ecs.GetComponent<PositionComponent>(fuseComponent.EntityId);
                game.State.Ecs.Factories.CreateExplosionEntity(positionComponent.CenterPosition);
                Ecs.DestroyEntity(fuseComponent.EntityId);
            }
        }
    }
}