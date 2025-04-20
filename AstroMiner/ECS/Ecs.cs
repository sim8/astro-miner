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
    private readonly BaseGame _game;

    private int _nextEntityId = 1;

    // Track special entities

    public Ecs(BaseGame game)
    {
        _game = game;
        Factories = new EntityFactories(this, game);

        // Initialize systems
        DynamiteSystem = new DynamiteSystem(this, game);
        ExplosionSystem = new ExplosionSystem(this, game);
        MovementSystem = new MovementSystem(this, game);
        PortalSystem = new PortalSystem(this, game);
        HealthSystem = new HealthSystem(this, game);
        VehicleEnterExitSystem = new VehicleEnterExitSystem(this, game);
        FallOrLavaDamageSystem = new FallOrLavaDamageSystem(this, game);
        MiningSystem = new MiningSystem(this, game);
        GrappleSystem = new GrappleSystem(this, game);
        LaunchSystem = new LaunchSystem(this, game);
    }

    public int? PlayerEntityId { get; private set; }

    public int? MinerEntityId { get; private set; }

    public int? ActiveControllableEntityId => _game.Model.Ecs.ActiveControllableEntityId;

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

    public Vector2 ActiveControllableEntityCenterPosition => ActiveControllableEntityId == null
        ? Vector2.Zero
        : GetComponent<PositionComponent>(ActiveControllableEntityId.Value).CenterPosition;

    public bool ActiveControllableEntityIsDead
    {
        get
        {
            if (ActiveControllableEntityId == null) return false;
            var healthComponent = GetComponent<HealthComponent>(ActiveControllableEntityId.Value);
            return healthComponent?.IsDead ?? false;
        }
    }

    public bool ActiveControllableEntityIsOffAsteroid
    {
        get
        {
            if (ActiveControllableEntityId == null) return false;
            var positionComponent = GetComponent<PositionComponent>(ActiveControllableEntityId.Value);
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

        _game.Model.Ecs.ActiveControllableEntityId = entityId;
    }

    public void DeactivateControllableEntity()
    {
        _game.Model.Ecs.ActiveControllableEntityId = null;
    }

    public bool GetIsActive(int entityId)
    {
        // If this is the active controllable entity, it's active
        if (entityId == ActiveControllableEntityId)
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
        if (ActiveControllableEntityId == entityId)
            _game.Model.Ecs.ActiveControllableEntityId = null;
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
                return positionComponent != null && positionComponent.World == _game.Model.ActiveWorld;
            });
    }

    private void CalculateEntityIdsInActiveWorldSortedByDistance()
    {
        EntityIdsInActiveWorldSortedByDistance = _entityComponents.Keys
            .Where(entityId =>
            {
                var positionComponent = GetComponent<PositionComponent>(entityId);
                return positionComponent != null && positionComponent.World == _game.Model.ActiveWorld;
            })
            .OrderBy(entityId => GetComponent<PositionComponent>(entityId).FrontY)
            .ToList(); // Cache the results since they'll be used multiple times per frame
    }
}