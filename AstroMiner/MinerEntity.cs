using Microsoft.Xna.Framework;

namespace AstroMiner;

public class MinerEntity(GameState gameState, Vector2 pos) : MiningControllableEntity(gameState, pos)
{
    protected override float MaxSpeed => 1.6f;
    protected override int TimeToReachMaxSpeedMs { get; } = 1200;
    protected override int TimeToStopMs { get; } = 400;
    protected override int BoxSizePx { get; } = GameConfig.MinerBoxSizePx;

    public override Vector2 GetDirectionalLightSource()
    {
        return Direction switch
        {
            Direction.Top => Position + new Vector2(0.53f, 0.17f),
            Direction.Right => Position + new Vector2(0.35f, 0.33f),
            Direction.Bottom => Position + new Vector2(0.06f, 0.29f),
            Direction.Left => Position + new Vector2(0.24f, -0.14f),
            _ => Position
        };
    }
}