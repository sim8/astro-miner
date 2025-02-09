using Microsoft.Xna.Framework;

namespace AstroMiner.AsteroidWorld;

public class ActiveExplosiveRockCell(GameState gameState, (int x, int y) gridPos, int timeToExplodeMs = 3000)
{
    private readonly (int X, int Y) Position = gridPos;
    public int TimeToExplodeMs = timeToExplodeMs;

    public void Update(GameTime gameTime)
    {
        TimeToExplodeMs -= gameTime.ElapsedGameTime.Milliseconds;
        if (TimeToExplodeMs <= 0)
        {
            var explosionPos = new Vector2(Position.X + 0.5f, Position.Y + 0.5f);
            gameState.Ecs.Factories.CreateExplosionEntity(explosionPos);
            gameState.AsteroidWorld.Grid.DeactivateExplosiveRockCell(Position.X, Position.Y);
        }
    }
}