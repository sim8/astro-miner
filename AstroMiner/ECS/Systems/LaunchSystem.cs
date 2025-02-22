using System.Collections.Generic;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class LaunchSystem : System
{
    private double startedAt = -1;

    public LaunchSystem(Ecs ecs, GameState gameState) : base(ecs, gameState)
    {
    }

    private bool HasJustPassedSeconds(GameTime gameTime, double seconds)
    {
        var secondsPastThreshold = gameTime.TotalGameTime.TotalSeconds - startedAt;
        return secondsPastThreshold >= seconds &&
               secondsPastThreshold - gameTime.ElapsedGameTime.TotalSeconds < seconds;
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        if (GameState.AsteroidWorld.IsInMiner)
        {
            if (startedAt == -1) startedAt = gameTime.TotalGameTime.TotalSeconds;

            if (HasJustPassedSeconds(gameTime, 1))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(119, 97);
                var id = GameState.Ecs.Factories.CreateLaunchLightEntity(pos);
                // TODO add to list, remove entities on count
            }

            if (HasJustPassedSeconds(gameTime, 2))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(125, 97);
                var id = GameState.Ecs.Factories.CreateLaunchLightEntity(pos);
                // TODO add to list, remove entities on count
            }

            if (HasJustPassedSeconds(gameTime, 3))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(131, 97);
                var id = GameState.Ecs.Factories.CreateLaunchLightEntity(pos);
                // TODO add to list, remove entities on count
            }
        }
        else
        {
            startedAt = -1;
        }
    }
}