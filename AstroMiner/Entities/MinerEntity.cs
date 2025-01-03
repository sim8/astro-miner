using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class MinerEntity(GameState gameState) : MiningControllableEntity(gameState)
{
    protected override bool CanAddToInventory { get; } = false;
    protected override float MaxSpeed => 3.2f;
    protected override int TimeToReachMaxSpeedMs { get; } = 1200;
    protected override float MaxHealth => GameConfig.MinerMaxHealth;
    protected override int TimeToStopMs { get; } = 400;
    protected override int BoxSizePx { get; } = GameConfig.MinerBoxSizePx;

    protected override float DrillingWidth { get; } = 0.9f;

    public override Vector2 GetDirectionalLightSource()
    {
        return Direction switch
        {
            Direction.Top => Position + new Vector2(1.06f, 0.34f),
            Direction.Right => Position + new Vector2(0.70f, 0.66f),
            Direction.Bottom => Position + new Vector2(0.12f, 0.58f),
            Direction.Left => Position + new Vector2(0.48f, -0.28f),
            _ => Position
        };
    }

    protected override void OnDead()
    {
        var explosionEntity = new ExplosionEntity(gameState, CenterPosition);
        gameState.ActivateEntity(explosionEntity);
    }
}