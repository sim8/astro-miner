using Microsoft.Xna.Framework;

namespace AstroMiner;

public class PlayerEntity(GridState gridState, Vector2 pos) : MiningControllableEntity(gridState, pos)
{
    protected override float MaxSpeed => 1f;
    public override int BoxSizePx { get; } = GameConfig.PlayerBoxSizePx;
}