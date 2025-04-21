using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class VehicleEnterExitSystem : System
{
    public VehicleEnterExitSystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }


    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        if (activeControls.Contains(MiningControls.EnterOrExit))
        {
            if (game.StateManager.AsteroidWorld.IsInMiner)
            {
                // Set player as active controllable entity
                Ecs.SetActiveControllableEntity(Ecs.PlayerEntityId.Value);

                var minerPosition = Ecs.GetComponent<PositionComponent>(Ecs.MinerEntityId.Value).Position;

                // Set player position to miner position
                var playerPosition = Ecs.GetComponent<PositionComponent>(Ecs.PlayerEntityId.Value);
                playerPosition.Position = minerPosition;
            }
            else
            {
                if (EntityHelpers.GetDistanceBetween(Ecs, Ecs.PlayerEntityId.Value, Ecs.MinerEntityId.Value) <
                    GameConfig.MinEmbarkingDistance)
                    // Set miner as active controllable entity
                    Ecs.SetActiveControllableEntity(Ecs.MinerEntityId.Value);
            }
            // TODO: Disembark?
            // ActiveControllableEntity.Disembark();
        }
    }
}