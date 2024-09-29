using Microsoft.Xna.Framework;

namespace AstroMiner;

public class PlayerEntity(GameState gameState, Vector2 pos) : MiningControllableEntity(gameState, pos)
{
    protected override float MaxSpeed => 1f;
    public override int BoxSizePx { get; } = GameConfig.PlayerBoxSizePx;
}