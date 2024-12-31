using Microsoft.Xna.Framework;

namespace AstroMiner;

public class ActiveExplosiveRockCell
{
    private readonly GameState _gameState;
    private readonly (int X, int Y) Position;
    private int TimeToExplodeMs;

    public ActiveExplosiveRockCell(GameState gameState, (int x, int y) gridPos)
    {
        TimeToExplodeMs = 3000;
        Position = gridPos;
        _gameState = gameState;
    }

    public void Update(int elapsedMs)
    {
        TimeToExplodeMs -= elapsedMs;
        if (TimeToExplodeMs <= 0)
        {
            var explosionPos = new Vector2(Position.X + 0.5f, Position.Y + 0.5f);
            var explosionEntity = new ExplosionEntity(_gameState, explosionPos);
            _gameState.ActivateEntity(explosionEntity);
            _gameState.Grid.DeactivateCell(Position.X, Position.Y);
        }
    }
}