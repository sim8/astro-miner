using System;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS;

public class EntityFactories
{
    private readonly GameState _gameState;
    private readonly World _world;

    public EntityFactories(GameState gameState)
    {
        _gameState = gameState;
        _world = gameState.EcsWorld;
    }

    public int CreatePlayerEntity(Vector2 position)
    {
        var entityId = _world.CreateEntity();

        // Add position component
        var positionComponent = _world.AddComponent<PositionComponent>(entityId);
        positionComponent.Position = position;
        positionComponent.BoxSizePx = GameConfig.PlayerBoxSizePx;
        positionComponent.IsCollideable = true;

        // Add movement component
        var movementComponent = _world.AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = 4f;  // From PlayerEntity
        movementComponent.TimeToReachMaxSpeedMs = 0;  // From ControllableEntity default
        movementComponent.TimeToStopMs = 0;  // From ControllableEntity default

        // Add health component
        var healthComponent = _world.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.PlayerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.PlayerMaxHealth;

        // Add tag component for identification
        _world.AddComponent<PlayerTag>(entityId);

        // Store the player entity ID
        _world.SetPlayerEntityId(entityId);

        return entityId;
    }

    public int CreateMinerEntity(Vector2 position)
    {
        var entityId = _world.CreateEntity();

        // Add position component
        var positionComponent = _world.AddComponent<PositionComponent>(entityId);
        positionComponent.Position = position;
        positionComponent.BoxSizePx = GameConfig.MinerBoxSizePx;
        positionComponent.IsCollideable = true;

        // Add movement component with miner-specific values
        var movementComponent = _world.AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = 4f;  // From MinerEntity
        movementComponent.TimeToReachMaxSpeedMs = 600;  // From MinerEntity
        movementComponent.TimeToStopMs = 400;  // From MinerEntity

        // Add health component
        var healthComponent = _world.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.MinerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.MinerMaxHealth;

        // Add tag component for identification
        _world.AddComponent<MinerTag>(entityId);

        // Store the miner entity ID
        _world.SetMinerEntityId(entityId);

        return entityId;
    }

    public int CreateDynamiteEntity(Vector2 position)
    {
        var entityId = _world.CreateEntity();

        var positionComponent = _world.AddComponent<PositionComponent>(entityId);
        positionComponent.Position = position;
        positionComponent.BoxSizePx = 4; // From DynamiteSystem
        positionComponent.IsCollideable = false;

        var fuseComponent = _world.AddComponent<FuseComponent>(entityId);
        fuseComponent.MaxFuseTimeMs = 4000; // From DynamiteSystem
        fuseComponent.TimeToExplodeMs = 4000;

        // Add tag component for identification
        _world.AddComponent<DynamiteTag>(entityId);

        return entityId;
    }

    public int CreateExplosionEntity(Vector2 position)
    {
        var entityId = _world.CreateEntity();

        var positionComponent = _world.AddComponent<PositionComponent>(entityId);
        positionComponent.Position = position;
        positionComponent.BoxSizePx = 1; // From ExplosionSystem
        positionComponent.IsCollideable = false;

        _world.AddComponent<ExplosionComponent>(entityId);
        _world.AddComponent<ExplosionTag>(entityId);

        return entityId;
    }
}