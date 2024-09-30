using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class Entity
{
    public Vector2 Position { get; set; }
    protected virtual int BoxSizePx { get; } = 1;

    public float GridBoxSize => (float)BoxSizePx / GameConfig.CellTextureSizePx;

    public Vector2 CenterPosition => Position + new Vector2(GridBoxSize / 2f, GridBoxSize / 2f);

    public float FrontY => Position.Y + GridBoxSize;

    public float GetDistanceTo(Entity entity)
    {
        return Vector2.Distance(CenterPosition, entity.CenterPosition);
    }

    public virtual void Update(int elapsedMs, HashSet<MiningControls> activeMiningControls)
    {
    }

    public bool SetPositionRelativeToDirectionalEntity(MiningControllableEntity directionalEntity, Direction rotation,
        bool insideEdge = false)
    {
        var centerToCenterDistance =
            directionalEntity.GridBoxSize / 2 + (insideEdge ? -(GridBoxSize / 2) : GridBoxSize / 2);
        var actualDirection = directionalEntity.GetRotatedDirection(rotation);
        var newCenterPos = actualDirection switch
        {
            Direction.Top => directionalEntity.CenterPosition + new Vector2(0, -centerToCenterDistance),
            Direction.Right => directionalEntity.CenterPosition + new Vector2(centerToCenterDistance, 0),
            Direction.Bottom => directionalEntity.CenterPosition + new Vector2(0, centerToCenterDistance),
            Direction.Left => directionalEntity.CenterPosition + new Vector2(-centerToCenterDistance, 0),
            _ => directionalEntity.CenterPosition
        };

        var newPos = newCenterPos - new Vector2(GridBoxSize / 2, GridBoxSize / 2);

        Position = newPos;
        
        
        // TODO check collisions - set or return false
        return true;
    }
}