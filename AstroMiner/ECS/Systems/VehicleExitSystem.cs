using System.Collections.Generic;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class VehicleExitSystem(Ecs ecs, BaseGame game) : System(ecs, game)
{
    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        if (activeControls.Contains(MiningControls.ExitVehicle) && game.StateManager.AsteroidWorld.IsInMiner)
        {
            // Set player as active controllable entity
            Ecs.SetActiveControllableEntity(Ecs.PlayerEntityId.Value);

            var minerPosition = Ecs.GetComponent<PositionComponent>(Ecs.MinerEntityId.Value).Position;

            // Set player position to miner position
            var playerPosition = Ecs.GetComponent<PositionComponent>(Ecs.PlayerEntityId.Value);
            playerPosition.Position = minerPosition;
        }
    }
}