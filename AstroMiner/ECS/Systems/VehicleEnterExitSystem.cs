using System;
using System.Collections.Generic;
using System.Linq;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Entities;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class VehicleEnterExitSystem : System
{

    public VehicleEnterExitSystem(World world, GameState gameState) : base(world, gameState)
    {
    }


    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        if (activeControls.Contains(MiningControls.EnterOrExit))
        {
            if (GameState.AsteroidWorld.IsInMiner)
            {

                // Set player as active controllable entity
                World.SetActiveControllableEntity(World.PlayerEntityId.Value);

                var minerPosition = World.GetComponent<PositionComponent>(World.MinerEntityId.Value).Position;

                // Set player position to miner position
                var playerPosition = World.GetComponent<PositionComponent>(World.PlayerEntityId.Value);
                playerPosition.Position = minerPosition;
            }
            else
            {
                if (EntityHelpers.GetDistanceBetween(World, World.PlayerEntityId.Value, World.MinerEntityId.Value) < GameConfig.MinEmbarkingDistance)
                {
                    // Set miner as active controllable entity
                    World.SetActiveControllableEntity(World.MinerEntityId.Value);
                }
            }
            // TODO: Disembark?
            // ActiveControllableEntity.Disembark();
        }
    }
}