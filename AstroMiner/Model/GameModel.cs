using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;

namespace AstroMiner.Model;

[Serializable]
public class GameModel
{
    public World ActiveWorld { get; set; }
    public long TotalPlaytimeMs { get; set; }
    public EcsModel Ecs { get; set; }
    public Launch Launch { get; set; }
}

[Serializable]
public class EcsModel
{
    public ComponentsByEntityId ComponentsByEntityId { get; set; }
    public int? ActiveControllableEntityId { get; set; }
    public int NextEntityId { get; set; }
    public int NextComponentId { get; set; }
    public int? PlayerEntityId { get; set; }

    public int? MinerEntityId { get; set; }
}

[Serializable]
public class ComponentsByEntityId
{
    public Dictionary<int, PositionComponent> Position { get; set; }
    public Dictionary<int, FuseComponent> Fuse { get; set; }
    public Dictionary<int, DirectionComponent> Direction { get; set; }
    public Dictionary<int, MovementComponent> Movement { get; set; }
    public Dictionary<int, HealthComponent> Health { get; set; }
    public Dictionary<int, MiningComponent> Mining { get; set; }
    public Dictionary<int, GrappleComponent> Grapple { get; set; }
    public Dictionary<int, DirectionalLightSourceComponent> DirectionalLightSource { get; set; }
    public Dictionary<int, TextureComponent> Texture { get; set; }
    public Dictionary<int, RadialLightSourceComponent> RadialLightSource { get; set; }
    public Dictionary<int, RenderLayerComponent> RenderLayer { get; set; }
    public Dictionary<int, ExplosionComponent> Explosion { get; set; }
    public Dictionary<int, DynamiteTag> DynamiteTag { get; set; }
    public Dictionary<int, PlayerTag> PlayerTag { get; set; }
    public Dictionary<int, MinerTag> MinerTag { get; set; }
    public Dictionary<int, ExplosionTag> ExplosionTag { get; set; }
}

[Serializable]
public class Launch
{
    public bool IsLaunching { get; set; }
    public int LaunchPadFrontEntityId { get; set; }
    public int LaunchPadRearEntityId { get; set; }
    public float MinerLaunchSpeed { get; set; }
}

public static class GameModelHelpers
{
    public static GameModel CreateNewGameModel()
    {
        return new GameModel
        {
            ActiveWorld = World.Home,
            TotalPlaytimeMs = 0,
            Ecs = new EcsModel
            {
                ActiveControllableEntityId = null,
                NextEntityId = 1,
                NextComponentId = 1,
                PlayerEntityId = null,
                MinerEntityId = null,
                ComponentsByEntityId = new ComponentsByEntityId
                {
                    Position = new Dictionary<int, PositionComponent>(),
                    Fuse = new Dictionary<int, FuseComponent>(),
                    Direction = new Dictionary<int, DirectionComponent>(),
                    Movement = new Dictionary<int, MovementComponent>(),
                    Health = new Dictionary<int, HealthComponent>(),
                    Mining = new Dictionary<int, MiningComponent>(),
                    Grapple = new Dictionary<int, GrappleComponent>(),
                    DirectionalLightSource = new Dictionary<int, DirectionalLightSourceComponent>(),
                    Texture = new Dictionary<int, TextureComponent>(),
                    RadialLightSource = new Dictionary<int, RadialLightSourceComponent>(),
                    RenderLayer = new Dictionary<int, RenderLayerComponent>(),
                    Explosion = new Dictionary<int, ExplosionComponent>(),
                    DynamiteTag = new Dictionary<int, DynamiteTag>(),
                    PlayerTag = new Dictionary<int, PlayerTag>(),
                    MinerTag = new Dictionary<int, MinerTag>(),
                    ExplosionTag = new Dictionary<int, ExplosionTag>()
                }
            },
            Launch = new Launch
            {
                LaunchPadFrontEntityId = -1,
                LaunchPadRearEntityId = -1,
                IsLaunching = false,
                MinerLaunchSpeed = 0f
            }
        };
    }
}