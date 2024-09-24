using Microsoft.Xna.Framework;

namespace AstroMiner;

public class MinerEntity(GridState gridState, Vector2 pos) : MiningControllableEntity(gridState, pos)
{
    protected override float MaxSpeed => 1.5f;
    protected override int TimeToReachMaxSpeedMs { get; } = 500;
    public override int BoxSizePx { get; } = GameConfig.MinerBoxSizePx;
}