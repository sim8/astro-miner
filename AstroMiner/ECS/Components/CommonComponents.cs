using System.Collections.Generic;
using System.Drawing;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace AstroMiner.ECS.Components;

public class PositionComponent : Component
{
    public int HeightPx;
    public bool IsCollideable;
    public Vector2 Position;
    public int WidthPx;
    public World World { get; set; }
    public bool IsOffAsteroid { get; set; }

    public float GridWidth => (float)WidthPx / GameConfig.CellTextureSizePx;
    public float GridHeight => (float)HeightPx / GameConfig.CellTextureSizePx;

    public Vector2 CenterPosition => Position + new Vector2(GridWidth / 2f, GridHeight / 2f);

    public float FrontY => Position.Y + GridHeight;

    public RectangleF Rectangle => new(Position.X, Position.Y, GridWidth, GridHeight);

    public void SetCenterPosition(Vector2 centerPos)
    {
        var posX = centerPos.X - GridWidth / 2;
        var posY = centerPos.Y - GridHeight / 2;
        Position = new Vector2(posX, posY);
    }
}

public class FuseComponent : Component
{
    public int MaxFuseTimeMs { get; set; }
    public int TimeToExplodeMs { get; set; }
    public float FusePercentLeft => TimeToExplodeMs / (float)MaxFuseTimeMs;
}

public class DirectionComponent : Component
{
    public Direction Direction { get; set; } = Direction.Top;
    public Vector2 DirectionalVector => DirectionHelpers.GetDirectionalVector(1f, Direction);
}

public class MovementComponent : Component
{
    // Movement constants
    public const float ExcessSpeedLossPerSecond = 3f;

    // State
    public bool IsUsingPortal { get; set; }
    public float CurrentSpeed { get; set; }
    public float MaxSpeed { get; set; }
    public int TimeToReachMaxSpeedMs { get; set; }
    public int TimeToStopMs { get; set; }
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

    // Lava state
    public int TimeOnLavaMs { get; set; }
    public bool IsOnLava { get; set; }

    public float HealthPercentage => CurrentHealth / MaxHealth;
    public float LavaTimePercentToTakingDamage => TimeOnLavaMs / (float)GameConfig.LavaDamageDelayMs;
}

// TODO rename Drill
public class MiningComponent : Component
{
    public readonly List<(int x, int y)> DrillingCells = new();
    public bool DrillingCellsMined { get; set; } = false;
    public int DrillingMs { get; set; } = 0;

    public (int x, int y)? DrillingPos { get; set; } = null;
    public int DrillingTotalTimeRequired { get; set; } = 0;

    public float DrillingWidth { get; set; } = 0f;
    public bool CanAddToInventory { get; set; } = true;
}

public class GrappleComponent : Component
{
    // Constants
    public const float ReelingBaseSpeed = 7f;
    public const float ReelingMaxSpeed = 11f;
    public const float GrapplesWidth = 0.4f;

    public int GrappleCooldownRemaining { get; set; } = 0;

    public Direction? GrappleDirection { get; set; } = null;
    public bool GrappleTargetIsValid { get; set; } = false;
    public bool PrevPressedUsedGrapple { get; set; } = false;
    public float GrapplePercentToTarget { get; set; } = 0f;
    public Vector2? GrappleTarget { get; set; } = null;

    // Derived
    public bool GrappleAvailable => GrappleCooldownRemaining == 0;
    public bool IsReelingIn => GrapplePercentToTarget == 1f && GrappleTargetIsValid;
}

public class DirectionalLightSourceComponent : Component
{
    public Vector2 TopOffset { get; set; }
    public Vector2 RightOffset { get; set; }
    public Vector2 BottomOffset { get; set; }
    public Vector2 LeftOffset { get; set; }

    public int SizePx { get; set; } = 512;
}

public class TextureComponent : Component
{
    public string TextureName { get; set; }
}

public class RadialLightSourceComponent : Component
{
    public Color Tint { get; set; }
    public int SizePx { get; set; }
    public float Opacity { get; set; } = 1f;
}

public class RenderLayerComponent : Component
{
    public EntityRenderLayer EntityRenderLayer { get; set; }
}