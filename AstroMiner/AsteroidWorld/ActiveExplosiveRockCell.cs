using System;
using Microsoft.Xna.Framework;

namespace AstroMiner.AsteroidWorld;

[Serializable]
public class ActiveExplosiveRockCell(BaseGame game, (int x, int y) gridPos, int timeToExplodeMs = 3000)
{
    private readonly (int X, int Y) Position = gridPos;
    public int TimeToExplodeMs = timeToExplodeMs;

    public void Update(GameTime gameTime)
    {
        TimeToExplodeMs -= gameTime.ElapsedGameTime.Milliseconds;
        if (TimeToExplodeMs <= 0)
        {
            var explosionPos = new Vector2(Position.X + 0.5f, Position.Y + 0.5f);
            game.StateManager.Ecs.Factories.CreateExplosionEntity(explosionPos);
            game.StateManager.AsteroidWorld.Grid.DeactivateExplosiveRockCell(Position.X, Position.Y);
        }
    }
}