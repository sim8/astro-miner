using System;
using System.Collections.Generic;
using System.Linq;
using AstroMiner.ECS.Components;
using AstroMiner.ECS.Systems;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS;

/// <summary>
///     The World class manages all entities and their components.
///     It provides methods for creating/destroying entities and adding/removing components.
/// </summary>
public class Ecs
{
    private readonly Dictionary<Type, HashSet<Component>> _componentsByType = new();
    private readonly Dictionary<int, HashSet<Component>> _entityComponents = new();
    private readonly GameState _gameState;

    // Track the currently active controllable entity
    private int? _activeControllableEntityId;
    private int _nextEntityId = 1;

    // Track special entities

    public Ecs(GameState gameState)
    {
        _gameState = gameState;
        Factories = new EntityFactories(this, gameState);

        // Initialize systems
        DynamiteSystem = new DynamiteSystem(this, gameState);
        ExplosionSystem = new ExplosionSystem(this, gameState);
        MovementSystem = new MovementSystem(this, gameState);
        PortalSystem = new PortalSystem(this, gameState);
        HealthSystem = new HealthSystem(this, gameState);
        VehicleEnterExitSystem = new VehicleEnterExitSystem(this, gameState);
        FallOrLavaDamageSystem = new FallOrLavaDamageSystem(this, gameState);
        MiningSystem = new MiningSystem(this, gameState);
        GrappleSystem = new GrappleSystem(this, gameState);
        LaunchSystem = new LaunchSystem(this, gameState);
    }

    public int? PlayerEntityId { get; private set; }

    public int? MinerEntityId { get; private set; }

    public int? ActiveControllableEntityId => _activeControllableEntityId;

    public EntityFactories Factories { get; }
    public IEnumerable<int> EntityIdsInActiveWorldSortedByDistance { get; private set; }

    // Systems
    public DynamiteSystem DynamiteSystem { get; }
    public ExplosionSystem ExplosionSystem { get; }
    public MovementSystem MovementSystem { get; }
    public PortalSystem PortalSystem { get; }
    public HealthSystem HealthSystem { get; }
    public VehicleEnterExitSystem VehicleEnterExitSystem { get; }
    public FallOrLavaDamageSystem FallOrLavaDamageSystem { get; }
    public MiningSystem MiningSystem { get; }
    public GrappleSystem GrappleSystem { get; }
    public LaunchSystem LaunchSystem { get; }

    public Vector2 ActiveControllableEntityCenterPosition => _activeControllableEntityId == null
        ? Vector2.Zero
        : GetComponent<PositionComponent>(_activeControllableEntityId.Value).CenterPosition;

    public bool ActiveControllableEntityIsDead
    {
        get
        {
            if (_activeControllableEntityId == null) return false;
            var healthComponent = GetComponent<HealthComponent>(_activeControllableEntityId.Value);
            return healthComponent?.IsDead ?? false;
        }
    }

    public bool ActiveControllableEntityIsOffAsteroid
    {
        get
        {
            if (_activeControllableEntityId == null) return false;
            var positionComponent = GetComponent<PositionComponent>(_activeControllableEntityId.Value);
            return positionComponent?.IsOffAsteroid ?? false;
        }
    }

    public void Update(GameTime gameTime, HashSet<MiningControls> activeMiningControls)
    {
        DynamiteSystem.Update(gameTime, activeMiningControls);
        ExplosionSystem.Update(gameTime, activeMiningControls);
        MovementSystem.Update(gameTime, activeMiningControls);
        PortalSystem.Update(gameTime, activeMiningControls);
        HealthSystem.Update(gameTime, activeMiningControls);
        VehicleEnterExitSystem.Update(gameTime, activeMiningControls);
        FallOrLavaDamageSystem.Update(gameTime, activeMiningControls);
        MiningSystem.Update(gameTime, activeMiningControls);
        GrappleSystem.Update(gameTime, activeMiningControls);
        LaunchSystem.Update(gameTime, activeMiningControls);
        CalculateEntityIdsInActiveWorldSortedByDistance();
    }

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

    public void SetPlayerEntityId(int entityId)
    {
        PlayerEntityId = entityId;
    }

    public void SetMinerEntityId(int entityId)
    {
        MinerEntityId = entityId;
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

        foreach (var component in components) _componentsByType[component.GetType()].Remove(component);

        _entityComponents.Remove(entityId);

        // Clear entity references if they're being destroyed
        if (_activeControllableEntityId == entityId)
            _activeControllableEntityId = null;
        if (PlayerEntityId == entityId)
            PlayerEntityId = null;
        if (MinerEntityId == entityId)
            MinerEntityId = null;
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

    public void RemoveComponent<T>(int entityId) where T : Component
    {
        if (!_entityComponents.TryGetValue(entityId, out var components))
            return;

        var component = GetComponent<T>(entityId);
        if (component == null)
            return;

        components.Remove(component);

        var type = typeof(T);
        if (_componentsByType.TryGetValue(type, out var typeComponents)) typeComponents.Remove(component);
    }

    public T GetComponent<T>(int entityId) where T : Component
    {
        if (!_entityComponents.TryGetValue(entityId, out var components))
            return null;

        foreach (var component in components)
            if (component is T typedComponent)
                return typedComponent;

        return null;
    }

    public IEnumerable<T> GetAllComponents<T>() where T : Component
    {
        var type = typeof(T);
        if (!_componentsByType.TryGetValue(type, out var components))
            yield break;

        foreach (var component in components) yield return (T)component;
    }

    public IEnumerable<T> GetAllComponentsInActiveWorld<T>() where T : Component
    {
        return GetAllComponents<T>()
            .Where(component =>
            {
                var positionComponent = GetComponent<PositionComponent>(component.EntityId);
                return positionComponent != null && positionComponent.World == _gameState.ActiveWorld;
            });
    }

    private void CalculateEntityIdsInActiveWorldSortedByDistance()
    {
        EntityIdsInActiveWorldSortedByDistance = _entityComponents.Keys
            .Where(entityId =>
            {
                var positionComponent = GetComponent<PositionComponent>(entityId);
                return positionComponent != null && positionComponent.World == _gameState.ActiveWorld;
            })
            .OrderBy(entityId => GetComponent<PositionComponent>(entityId).FrontY)
            .ToList(); // Cache the results since they'll be used multiple times per frame
    }
}