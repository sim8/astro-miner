using AstroMiner.ECS.Systems;

namespace AstroMiner.ECS.Components;

public class ExplosionComponent : Component
{
    public int TimeSinceExplosionMs { get; set; }
    public bool HasExploded { get; set; }
    public float AnimationPercentage => TimeSinceExplosionMs / (float)ExplosionSystem.AnimationTimeMs;
} 