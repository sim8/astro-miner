using Microsoft.Xna.Framework;

namespace AstroMiner;

public class MinerEntity(GridState gridState, Vector2 pos) : MiningControllableEntity(gridState, pos)
{
    protected override float MaxSpeed => 1.6f;
    protected override int TimeToReachMaxSpeedMs { get; } = 800;
    protected override int TimeToStopMs { get; } = 400;
    public override int BoxSizePx { get; } = GameConfig.MinerBoxSizePx;
}