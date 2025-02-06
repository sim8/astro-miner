using Microsoft.Xna.Framework;
using AstroMiner.Definitions;
using System.Drawing;
using AstroMiner.Entities;
using AstroMiner.Utilities;

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

public class MovementComponent : Component
{
    // Movement constants
    public const float ExcessSpeedLossPerSecond = 3f;

    // State
    public Direction Direction { get; set; } = Direction.Top;
    public float CurrentSpeed { get; set; }
    public float MaxSpeed { get; set; }
    public int TimeToReachMaxSpeedMs { get; set; }
    public int TimeToStopMs { get; set; }

    // Derived properties
    public Vector2 DirectionalVector => DirectionHelpers.GetDirectionalVector(1f, Direction);
    public Vector2 VelocityVector => DirectionalVector * CurrentSpeed;
}

public class HealthComponent : Component
{
    public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }
    public bool IsDead { get; set; }

    // Animation state
    public bool IsAnimatingDamage { get; set; }
    public int TimeSinceLastDamageMs { get; set; }
    public int TotalDamageAnimationTimeMs { get; set; }

    public float HealthPercentage => CurrentHealth / MaxHealth;
}