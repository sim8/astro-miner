using System;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS;

public class EntityFactories
{
    private readonly Ecs _ecs;

    public EntityFactories(Ecs ecs)
    {
        _ecs = ecs;
    }

    public int CreatePlayerEntity(Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        // Add position component
        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.Position = position;
        positionComponent.BoxSizePx = GameConfig.PlayerBoxSizePx;
        positionComponent.IsCollideable = true;

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
        positionComponent.Position = position;
        positionComponent.BoxSizePx = GameConfig.MinerBoxSizePx;
        positionComponent.IsCollideable = true;

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
        positionComponent.Position = position;
        positionComponent.BoxSizePx = 1; // From ExplosionSystem
        positionComponent.IsCollideable = false;

        _ecs.AddComponent<ExplosionComponent>(entityId);
        _ecs.AddComponent<ExplosionTag>(entityId);

        return entityId;
    }
}