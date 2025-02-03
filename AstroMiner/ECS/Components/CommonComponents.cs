using Microsoft.Xna.Framework;
using AstroMiner.Definitions;
using System.Drawing;
using AstroMiner.Entities;

namespace AstroMiner.ECS.Components;

public class PositionComponent : Component
{
    public int BoxSizePx;
    public Vector2 Position;

    public float GridBoxSize => (float)BoxSizePx / GameConfig.CellTextureSizePx;

    public Vector2 CenterPosition => Position + new Vector2(GridBoxSize / 2f, GridBoxSize / 2f);

    public float FrontY => Position.Y + GridBoxSize;

    public RectangleF Rectangle => new(Position.X, Position.Y, GridBoxSize, GridBoxSize);

    public bool SetPositionRelativeToDirectionalEntity(ControllableEntity directionalEntity, Direction rotation,
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

        Position = newCenterPos - new Vector2(GridBoxSize / 2, GridBoxSize / 2);
        return true; // TODO: Check collisions
    }
}

public class FuseComponent : Component
{
    public int MaxFuseTimeMs { get; set; }
    public int TimeToExplodeMs { get; set; }
    public float FusePercentLeft => TimeToExplodeMs / (float)MaxFuseTimeMs;
} 