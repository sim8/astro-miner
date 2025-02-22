using System;
using System.Collections.Generic;
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
        return (secondsPastThreshold >= seconds) && (secondsPastThreshold - gameTime.ElapsedGameTime.TotalSeconds < seconds);
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        if (GameState.AsteroidWorld.IsInMiner)
        {
            if (startedAt == -1)
            {
                startedAt = gameTime.TotalGameTime.TotalSeconds;
            }

            if (HasJustPassedSeconds(gameTime, 3))
            {
                Console.WriteLine("hello world");
            }
        }
        else
        {
            startedAt = -1;
        }
    }
}