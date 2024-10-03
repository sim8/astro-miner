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
    
    public override Vector2 GetDirectionalLightSource()
    {
        return Direction switch
        {
            Direction.Top => Position + new Vector2(0.14f, -0.15f),
            Direction.Right => Position + new Vector2(0.16f, -0.15f),
            Direction.Bottom => Position + new Vector2(0.12f, -0.15f),
            Direction.Left => Position + new Vector2(0.13f, -0.14f),
            _ => Position
        };
    }
}