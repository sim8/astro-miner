using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class PlayerEntity(GameState gameState, Vector2 pos) : MiningControllableEntity(gameState, pos)
{
    private bool _prevPressedPlaceDynamite;
    protected override float MaxSpeed => 1f;
    protected override int BoxSizePx { get; } = GameConfig.PlayerBoxSizePx;

    public override void Update(int elapsedMs, HashSet<MiningControls> activeMiningControls)
    {
        base.Update(elapsedMs, activeMiningControls);
        if (activeMiningControls.Contains(MiningControls.PlaceDynamite))
        {
            // Not continuous
            if (!_prevPressedPlaceDynamite)
            {
                var dynamiteEntity = new DynamiteEntity(gameState, CenterPosition);
                dynamiteEntity.SetPositionRelativeToDirectionalEntity(this, Direction.Top);
                gameState.ActiveEntitiesSortedByDistance.Add(dynamiteEntity);
                _prevPressedPlaceDynamite = true;
            }
        }
        else
        {
            _prevPressedPlaceDynamite = false;
        }
    }
}