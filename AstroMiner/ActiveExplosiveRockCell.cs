using AstroMiner.entities;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class ActiveExplosiveRockCell(GameState gameState, (int x, int y) gridPos, int timeToExplodeMs = 3000)
{
    private readonly (int X, int Y) Position = gridPos;
    public int TimeToExplodeMs = timeToExplodeMs;

    public void Update(int elapsedMs)
    {
        TimeToExplodeMs -= elapsedMs;
        if (TimeToExplodeMs <= 0)
        {
            var explosionPos = new Vector2(Position.X + 0.5f, Position.Y + 0.5f);
            var explosionEntity = new ExplosionEntity(gameState, explosionPos);
            gameState.ActivateEntity(explosionEntity);
            gameState.Grid.DeactivateExplosiveRockCell(Position.X, Position.Y);
        }
    }
}