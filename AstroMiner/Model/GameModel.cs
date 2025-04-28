using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AstroMiner.AsteroidWorld;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;

namespace AstroMiner.Model;

[Serializable]
public class GameModel
{
    public World ActiveWorld { get; set; }
    public long SavedTotalPlaytimeMs { get; set; }
    public EcsModel Ecs { get; set; }
    public LaunchModel Launch { get; set; }
    public InventoryModel Inventory { get; set; }
    public AsteroidModel Asteroid { get; set; }
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
public class InventoryItem
{
    public ItemType Type { get; set; }
    public int Count { get; set; }
}

[Serializable]
public class InventoryModel
{
    public List<InventoryItem?> Items { get; set; }
    public int SelectedIndex { get; set; }
}

[Serializable]
public class LaunchModel
{
    public bool IsLaunching { get; set; }
    public int LaunchPadFrontEntityId { get; set; }
    public int LaunchPadRearEntityId { get; set; }
    public float MinerLaunchSpeed { get; set; }
}

[Serializable]
public class CellState
{
    public const int UninitializedOrAboveMax = -1;

    public AsteroidLayer Layer { get; init; }

    /**
     * -1: uninitialized or above max distance
     * 0+ distance to floor with unbroken connection to edge
     */
    public int DistanceToExploredFloor { get; set; } = UninitializedOrAboveMax;

    public FloorType FloorType { get; set; }

    public float FogOpacity { get; set; } = 1f; // Assume fog

    public WallType WallType { get; set; }

    [JsonIgnore] public bool isEmpty => WallType == WallType.Empty && FloorType == FloorType.Empty;
}

[Serializable]
public class AsteroidModel
{
    public int Seed { get; set; }
    public long WillExplodeAt { get; set; }
    public CellState[,] Grid { get; set; }

    // TODO nice way to combine these + other effects?
    // Would be nice if cell classes had a nice deactive method
    public Dictionary<(int x, int y), ActiveCollapsingFloorCell> ActiveCollapsingFloorCells { get; init; }
    public Dictionary<(int x, int y), ActiveExplosiveRockCell> ActiveExplosiveRockCells { get; init; }
}

public static class GameModelHelpers
{
    public static GameModel CreateNewGameModel()
    {
        return new GameModel
        {
            ActiveWorld = World.Home,
            SavedTotalPlaytimeMs = 0,
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
            Launch = new LaunchModel
            {
                LaunchPadFrontEntityId = -1,
                LaunchPadRearEntityId = -1,
                IsLaunching = false,
                MinerLaunchSpeed = 0f
            },
            Inventory = new InventoryModel
            {
                Items =
                [
                    new InventoryItem
                    {
                        Count = 1,
                        Type = ItemType.Drill
                    },
                    new InventoryItem
                    {
                        Count = 3,
                        Type = ItemType.Dynamite
                    }
                ]
            },
            Asteroid = new AsteroidModel
            {
                ActiveExplosiveRockCells = new Dictionary<(int x, int y), ActiveExplosiveRockCell>(),
                ActiveCollapsingFloorCells = new Dictionary<(int x, int y), ActiveCollapsingFloorCell>()
            }
        };
    }
}