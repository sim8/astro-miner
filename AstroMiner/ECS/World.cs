using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS;

/// <summary>
/// The World class manages all entities and their components.
/// It provides methods for creating/destroying entities and adding/removing components.
/// </summary>
public class World
{
    private readonly GameState _gameState;
    private int _nextEntityId = 1;
    private readonly Dictionary<int, HashSet<Component>> _entityComponents = new();
    private readonly Dictionary<Type, HashSet<Component>> _componentsByType = new();

    // Track special entities
    private int? _playerEntityId;
    private int? _minerEntityId;
    public int? PlayerEntityId => _playerEntityId;
    public int? MinerEntityId => _minerEntityId;

    // Track the currently active controllable entity
    private int? _activeControllableEntityId;
    public int? ActiveControllableEntityId => _activeControllableEntityId;

    public void SetActiveControllableEntity(int entityId)
    {
        if (!_entityComponents.ContainsKey(entityId))
            return;

        _activeControllableEntityId = entityId;
    }

    public void DeactivateControllableEntity()
    {
        _activeControllableEntityId = null;
    }

    public Vector2 ActiveControllableEntityCenterPosition => _activeControllableEntityId == null ? Vector2.Zero : GetComponent<PositionComponent>(_activeControllableEntityId.Value).CenterPosition;

    public bool ActiveControllableEntityIsDead => false;
    public bool ActiveControllableEntityIsOffAsteroid => false;


    public World(GameState gameState)
    {
        _gameState = gameState;
    }

    public bool GetIsActive(int entityId)
    {
        // If this is the active controllable entity, it's active
        if (entityId == _activeControllableEntityId)
            return true;

        // If it doesn't have a MovementComponent component, it's always active
        // TODO use better thing than Movement?
        var hasControllableComponent = HasComponent<MovementComponent>(entityId);
        return !hasControllableComponent;
    }

    public int CreatePlayerEntity(Vector2 position)
    {
        var entityId = CreateEntity();

        // Add position component
        var positionComponent = AddComponent<PositionComponent>(entityId);
        positionComponent.Position = position;
        positionComponent.BoxSizePx = GameConfig.PlayerBoxSizePx;

        // Add movement component
        var movementComponent = AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = 4f;  // From PlayerEntity
        movementComponent.TimeToReachMaxSpeedMs = 0;  // From ControllableEntity default
        movementComponent.TimeToStopMs = 0;  // From ControllableEntity default

        // Add tag component for identification
        AddComponent<PlayerTag>(entityId);

        // Store the player entity ID
        _playerEntityId = entityId;

        return entityId;
    }

    public int CreateMinerEntity(Vector2 position)
    {
        var entityId = CreateEntity();

        // Add position component
        var positionComponent = AddComponent<PositionComponent>(entityId);
        positionComponent.Position = position;
        positionComponent.BoxSizePx = GameConfig.MinerBoxSizePx;

        // Add movement component with miner-specific values
        var movementComponent = AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = 4f;  // From MinerEntity
        movementComponent.TimeToReachMaxSpeedMs = 600;  // From MinerEntity
        movementComponent.TimeToStopMs = 400;  // From MinerEntity

        // Add tag component for identification
        AddComponent<MinerTag>(entityId);

        // Store the miner entity ID
        _minerEntityId = entityId;

        return entityId;
    }

    public IEnumerable<int> GetAllEntityIds()
    {
        return _entityComponents.Keys;
    }

    public bool HasComponent<T>(int entityId) where T : Component
    {
        if (!_entityComponents.TryGetValue(entityId, out var components))
            return false;

        foreach (var component in components)
            if (component is T)
                return true;

        return false;
    }

    public int CreateEntity()
    {
        var entityId = _nextEntityId++;
        _entityComponents[entityId] = new HashSet<Component>();
        return entityId;
    }

    public void DestroyEntity(int entityId)
    {
        if (!_entityComponents.TryGetValue(entityId, out var components))
            return;

        foreach (var component in components)
        {
            _componentsByType[component.GetType()].Remove(component);
        }

        _entityComponents.Remove(entityId);

        // Clear entity references if they're being destroyed
        if (_activeControllableEntityId == entityId)
            _activeControllableEntityId = null;
        if (_playerEntityId == entityId)
            _playerEntityId = null;
        if (_minerEntityId == entityId)
            _minerEntityId = null;
    }

    public T AddComponent<T>(int entityId) where T : Component, new()
    {
        var component = new T { EntityId = entityId };

        if (!_entityComponents.ContainsKey(entityId))
            _entityComponents[entityId] = new HashSet<Component>();

        _entityComponents[entityId].Add(component);

        var type = typeof(T);
        if (!_componentsByType.ContainsKey(type))
            _componentsByType[type] = new HashSet<Component>();

        _componentsByType[type].Add(component);

        return component;
    }

    public T GetComponent<T>(int entityId) where T : Component
    {
        if (!_entityComponents.TryGetValue(entityId, out var components))
            return null;

        foreach (var component in components)
        {
            if (component is T typedComponent)
                return typedComponent;
        }

        return null;
    }

    public IEnumerable<T> GetAllComponents<T>() where T : Component
    {
        var type = typeof(T);
        if (!_componentsByType.TryGetValue(type, out var components))
            yield break;

        foreach (var component in components)
        {
            yield return (T)component;
        }
    }
}