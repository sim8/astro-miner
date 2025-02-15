using System;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS;

public class EntityFactories
{
    private readonly GameState _gameState;
    private readonly Ecs _ecs;

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
        positionComponent.BoxSizePx = GameConfig.PlayerBoxSizePx;
        positionComponent.IsCollideable = true;

        // Add direction component
        _ecs.AddComponent<DirectionComponent>(entityId);

        // Add movement component
        var movementComponent = _ecs.AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = 4f;  // From PlayerEntity
        movementComponent.TimeToReachMaxSpeedMs = 0;  // From ControllableEntity default
        movementComponent.TimeToStopMs = 0;  // From ControllableEntity default

        // Add health component
        var healthComponent = _ecs.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.PlayerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.PlayerMaxHealth;

        // Add mining component
        _ecs.AddComponent<MiningComponent>(entityId);

        // Add directional light source component
        var directionalLightSourceComponent = _ecs.AddComponent<DirectionalLightSourceComponent>(entityId);
        directionalLightSourceComponent.TopOffset = new Vector2(0.28f, -0.30f);
        directionalLightSourceComponent.RightOffset = new Vector2(0.32f, -0.30f);
        directionalLightSourceComponent.BottomOffset = new Vector2(0.24f, -0.30f);
        directionalLightSourceComponent.LeftOffset = new Vector2(0.26f, -0.28f);

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
        positionComponent.BoxSizePx = GameConfig.MinerBoxSizePx;
        positionComponent.IsCollideable = true;

        // Add direction component
        _ecs.AddComponent<DirectionComponent>(entityId);

        // Add movement component with miner-specific values
        var movementComponent = _ecs.AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = 4f;  // From MinerEntity
        movementComponent.TimeToReachMaxSpeedMs = 600;  // From MinerEntity
        movementComponent.TimeToStopMs = 400;  // From MinerEntity

        // Add health component
        var healthComponent = _ecs.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.MinerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.MinerMaxHealth;

        // Add mining component
        var miningComponent = _ecs.AddComponent<MiningComponent>(entityId);
        miningComponent.DrillingWidth = 0.9f;

        // Add grapple component
        _ecs.AddComponent<GrappleComponent>(entityId);

        // Add directional light source component
        var directionalLightSourceComponent = _ecs.AddComponent<DirectionalLightSourceComponent>(entityId);
        directionalLightSourceComponent.TopOffset = new Vector2(1.06f, 0.34f);
        directionalLightSourceComponent.RightOffset = new Vector2(0.70f, 0.66f);
        directionalLightSourceComponent.BottomOffset = new Vector2(0.12f, 0.58f);
        directionalLightSourceComponent.LeftOffset = new Vector2(0.48f, -0.28f);

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
        positionComponent.BoxSizePx = 4; // From DynamiteSystem
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
        positionComponent.BoxSizePx = 1; // From ExplosionSystem
        positionComponent.IsCollideable = false;

        _ecs.AddComponent<ExplosionComponent>(entityId);
        _ecs.AddComponent<ExplosionTag>(entityId);

        return entityId;
    }
}