using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class DynamiteEntity : Entity
{
    private const int FuseTimeMs = 4000;
    private readonly GameState _gameState;

    public DynamiteEntity(GameState gameState, Vector2 pos)
    {
        TimeToExplodeMs = FuseTimeMs;
        Position = pos;
        _gameState = gameState;
    }

    protected override int BoxSizePx { get; } = 4;
    public override bool CanCollide { get; } = false;

    public float FusePercentLeft => TimeToExplodeMs / (float)FuseTimeMs;

    public int TimeToExplodeMs { get; private set; }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeMiningControls)
    {
        TimeToExplodeMs -= gameTime.ElapsedGameTime.Milliseconds;
        if (TimeToExplodeMs <= 0)
        {
            var explosionEntity = new ExplosionEntity(_gameState, CenterPosition);
            _gameState.AsteroidWorld.ActivateEntity(explosionEntity);
            _gameState.AsteroidWorld.DeactivateEntity(this);
        }
    }
}