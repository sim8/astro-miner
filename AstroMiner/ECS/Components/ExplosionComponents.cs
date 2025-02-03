using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Components;

public class ExplosionComponent : Component
{
    // Animation state
    public const int AnimationTimeMs = 400;
    public int TimeSinceExplosionMs { get; set; }
    public bool HasExploded { get; set; }
    public float AnimationPercentage => TimeSinceExplosionMs / (float)AnimationTimeMs;

    // Effect properties
    public const float ExplodeRockRadius = 2.2f;
    public const float ExplosionRadius = 4f;
    public const int BoxSizePx = 1;
} 