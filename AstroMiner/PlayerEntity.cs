using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class PlayerEntity(GameState gameState, Vector2 pos) : MiningControllableEntity(gameState, pos)
{
    protected override float MaxSpeed => 1f;
    public override int BoxSizePx { get; } = GameConfig.PlayerBoxSizePx;

    public override void Update(int elapsedMs, HashSet<MiningControls> activeMiningControls)
    {
        base.Update(elapsedMs, activeMiningControls);
        if (activeMiningControls.Contains(MiningControls.PlaceDynamite))
        {
            // do nothing
            //UseDrill(elapsedMs);
        }
    }
}