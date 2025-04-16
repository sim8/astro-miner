using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS;

public class EntityFactories
{
    private readonly Ecs _ecs;
    private readonly GameState _gameState;

    public EntityFactories(Ecs ecs, GameState gameState)
    {
        _gameState = gameState;
        _ecs = ecs;
    }

    public int CreatePlayerEntity(Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        // Add position component
        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.Position = position;
        positionComponent.World = _gameState.ActiveWorld;
        positionComponent.WidthPx = GameConfig.PlayerBoxSizePx;
        positionComponent.HeightPx = GameConfig.PlayerBoxSizePx;
        positionComponent.IsCollideable = true;

        // Add direction component
        _ecs.AddComponent<DirectionComponent>(entityId);

        // Add movement component
        var movementComponent = _ecs.AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = GameConfig.Speeds.Running;
        movementComponent.TimeToReachMaxSpeedMs = 0; // From ControllableEntity default
        movementComponent.TimeToStopMs = 0; // From ControllableEntity default

        // Add tag component for identification
        _ecs.AddComponent<PlayerTag>(entityId);

        // Store the player entity ID
        _ecs.SetPlayerEntityId(entityId);

        return entityId;
    }

    public int CreateMinerEntity(Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        // Add position component
        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = _gameState.ActiveWorld;
        positionComponent.Position = position;
        positionComponent.WidthPx = GameConfig.MinerBoxSizePx;
        positionComponent.HeightPx = GameConfig.MinerBoxSizePx;
        positionComponent.IsCollideable = true;

        // Add direction component
        _ecs.AddComponent<DirectionComponent>(entityId);

        // Add tag component for identification
        _ecs.AddComponent<MinerTag>(entityId);

        // Store the miner entity ID
        _ecs.SetMinerEntityId(entityId);

        return entityId;
    }

    public int CreateDynamiteEntity(Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = _gameState.ActiveWorld;
        positionComponent.Position = position;
        positionComponent.WidthPx = 4; // From DynamiteSystem
        positionComponent.HeightPx = 4; // From DynamiteSystem
        positionComponent.IsCollideable = false;

        var fuseComponent = _ecs.AddComponent<FuseComponent>(entityId);
        fuseComponent.MaxFuseTimeMs = 4000; // From DynamiteSystem
        fuseComponent.TimeToExplodeMs = 4000;

        // Add tag component for identification
        _ecs.AddComponent<DynamiteTag>(entityId);

        return entityId;
    }

    public int CreateExplosionEntity(Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = _gameState.ActiveWorld;
        positionComponent.Position = position;
        positionComponent.WidthPx = 1; // From ExplosionSystem
        positionComponent.HeightPx = 1; // From ExplosionSystem
        positionComponent.IsCollideable = false;

        _ecs.AddComponent<ExplosionComponent>(entityId);
        _ecs.AddComponent<ExplosionTag>(entityId);

        return entityId;
    }

    public int CreateLaunchLightEntity(Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = _gameState.ActiveWorld;
        positionComponent.Position = position;
        positionComponent.WidthPx = 5;
        positionComponent.HeightPx = 5;
        positionComponent.IsCollideable = false;

        var textureComponent = _ecs.AddComponent<TextureComponent>(entityId);
        textureComponent.TextureName = "launch-light";

        var radialLightSourceComponent = _ecs.AddComponent<RadialLightSourceComponent>(entityId);
        radialLightSourceComponent.Tint = new Color(226, 86, 86);
        radialLightSourceComponent.SizePx = 128;
        radialLightSourceComponent.Opacity = 0.3f;

        var renderLayerComponent = _ecs.AddComponent<RenderLayerComponent>(entityId);
        renderLayerComponent.EntityRenderLayer = EntityRenderLayer.BehindEntities;

        return entityId;
    }

    public int CreateLaunchPadFrontEntity(Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = _gameState.ActiveWorld;
        positionComponent.Position = position;
        positionComponent.WidthPx = 76;
        positionComponent.HeightPx = 63;
        positionComponent.IsCollideable = false;

        var textureComponent = _ecs.AddComponent<TextureComponent>(entityId);
        textureComponent.TextureName = "launch-pad-front";

        var renderLayerComponent = _ecs.AddComponent<RenderLayerComponent>(entityId);
        renderLayerComponent.EntityRenderLayer = EntityRenderLayer.BehindEntities;

        return entityId;
    }

    public int CreateLaunchPadRearEntity(Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = _gameState.ActiveWorld;
        positionComponent.Position = position;
        positionComponent.WidthPx = 76;
        positionComponent.HeightPx = 31;
        positionComponent.IsCollideable = false;

        var textureComponent = _ecs.AddComponent<TextureComponent>(entityId);
        textureComponent.TextureName = "launch-pad-rear";

        var renderLayerComponent = _ecs.AddComponent<RenderLayerComponent>(entityId);
        renderLayerComponent.EntityRenderLayer = EntityRenderLayer.BehindWorld;

        return entityId;
    }
}