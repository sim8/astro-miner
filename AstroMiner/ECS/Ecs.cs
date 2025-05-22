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
    private readonly BaseGame _game;


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
        FallOrLavaDamageSystem = new FallOrLavaDamageSystem(this, game);
        MiningSystem = new MiningSystem(this, game);
        GrappleSystem = new GrappleSystem(this, game);
        InteractionSystem = new InteractionSystem(this, game);
        EntityTransformSystem = new EntityTransformSystem(this, game);
    }

    public int? ActiveControllableEntityId => _game.Model.Ecs.ActiveControllableEntityId;

    public EntityFactories Factories { get; }
    public IEnumerable<int> EntityIdsInActiveWorldSortedByDistance { get; private set; } = [];

    // Systems
    public DynamiteSystem DynamiteSystem { get; }
    public ExplosionSystem ExplosionSystem { get; }
    public MovementSystem MovementSystem { get; }
    public PortalSystem PortalSystem { get; }
    public HealthSystem HealthSystem { get; }
    public FallOrLavaDamageSystem FallOrLavaDamageSystem { get; }
    public MiningSystem MiningSystem { get; }
    public GrappleSystem GrappleSystem { get; }
    public InteractionSystem InteractionSystem { get; }
    public EntityTransformSystem EntityTransformSystem { get; }

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

    public int? PlayerEntityId => _game.Model.Ecs.PlayerEntityId;
    public int? MinerEntityId => _game.Model.Ecs.MinerEntityId;

    public void Update(GameTime gameTime, ActiveControls activeControls)
    {
        DynamiteSystem.Update(gameTime, activeControls);
        ExplosionSystem.Update(gameTime, activeControls);
        MovementSystem.Update(gameTime, activeControls);
        PortalSystem.Update(gameTime, activeControls);
        HealthSystem.Update(gameTime, activeControls);
        FallOrLavaDamageSystem.Update(gameTime, activeControls);
        MiningSystem.Update(gameTime, activeControls);
        GrappleSystem.Update(gameTime, activeControls);
        InteractionSystem.Update(gameTime, activeControls);
        EntityTransformSystem.Update(gameTime, activeControls);
        CalculateEntityIdsInActiveWorldSortedByDistance();
    }


    public void SetActiveControllableEntity(int entityId)
    {
        _game.Model.Ecs.ActiveControllableEntityId = entityId;
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
        _game.Model.Ecs.PlayerEntityId = entityId;
    }

    public void SetMinerEntityId(int entityId)
    {
        _game.Model.Ecs.MinerEntityId = entityId;
    }

    public bool HasComponent<T>(int entityId) where T : Component
    {
        return GetComponentsOfType<T>().ContainsKey(entityId);
    }

    public int CreateEntity()
    {
        var entityId = _game.Model.Ecs.NextEntityId++;
        // _game.Model.Ecs.EntityComponents[entityId] = new HashSet<Component>();
        return entityId;
    }

    public void DestroyEntity(int entityId)
    {
        _game.Model.Ecs.ComponentsByEntityId.Position.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.Fuse.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.Direction.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.Movement.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.Health.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.Mining.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.Grapple.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.DirectionalLightSource.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.Texture.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.RadialLightSource.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.RenderLayer.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.Explosion.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.DynamiteTag.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.PlayerTag.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.MinerTag.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.ExplosionTag.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.LaunchConsoleTag.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.Npc.Remove(entityId);
        _game.Model.Ecs.ComponentsByEntityId.Interactive.Remove(entityId);

        // Clear entity references if they're being destroyed
        if (ActiveControllableEntityId == entityId)
            _game.Model.Ecs.ActiveControllableEntityId = null;
        if (_game.Model.Ecs.PlayerEntityId == entityId)
            _game.Model.Ecs.PlayerEntityId = null;
        if (_game.Model.Ecs.MinerEntityId == entityId)
            _game.Model.Ecs.MinerEntityId = null;
    }

    public T AddComponent<T>(int entityId) where T : Component, new()
    {
        var component = new T { EntityId = entityId };

        var components = GetComponentsOfType<T>();

        if (components.ContainsKey(entityId))
            throw new ArgumentException($"Entity {entityId} already has a component of type {typeof(T)}");

        components[entityId] = component;

        return component;
    }

    public void RemoveComponent<T>(int entityId) where T : Component
    {
        GetComponentsOfType<T>().Remove(entityId);
    }

    public T GetComponent<T>(int entityId) where T : Component
    {
        return GetComponentsOfType<T>().TryGetValue(entityId, out var component) ? component : null;
    }

    public IEnumerable<T> GetAllComponents<T>() where T : Component
    {
        var components = GetComponentsOfType<T>();

        foreach (var component in components.Values) yield return component;
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
        // TODO simplify only needs calling once
        EntityIdsInActiveWorldSortedByDistance = GetComponentsOfType<PositionComponent>().Keys
            .Where(entityId =>
            {
                var positionComponent = GetComponent<PositionComponent>(entityId);
                return positionComponent != null && positionComponent.World == _game.Model.ActiveWorld;
            })
            .OrderBy(entityId => GetComponent<PositionComponent>(entityId).FrontY)
            .ToList(); // Cache the results since they'll be used multiple times per frame
    }

    private Dictionary<int, T> GetComponentsOfType<T>() where T : class
    {
        var componentsByEntityId = _game.Model.Ecs.ComponentsByEntityId;

        if (typeof(T) == typeof(PositionComponent))
            return componentsByEntityId.Position as Dictionary<int, T>;
        if (typeof(T) == typeof(FuseComponent))
            return componentsByEntityId.Fuse as Dictionary<int, T>;
        if (typeof(T) == typeof(DirectionComponent))
            return componentsByEntityId.Direction as Dictionary<int, T>;
        if (typeof(T) == typeof(MovementComponent))
            return componentsByEntityId.Movement as Dictionary<int, T>;
        if (typeof(T) == typeof(HealthComponent))
            return componentsByEntityId.Health as Dictionary<int, T>;
        if (typeof(T) == typeof(MiningComponent))
            return componentsByEntityId.Mining as Dictionary<int, T>;
        if (typeof(T) == typeof(GrappleComponent))
            return componentsByEntityId.Grapple as Dictionary<int, T>;
        if (typeof(T) == typeof(DirectionalLightSourceComponent))
            return componentsByEntityId.DirectionalLightSource as Dictionary<int, T>;
        if (typeof(T) == typeof(TextureComponent))
            return componentsByEntityId.Texture as Dictionary<int, T>;
        if (typeof(T) == typeof(RadialLightSourceComponent))
            return componentsByEntityId.RadialLightSource as Dictionary<int, T>;
        if (typeof(T) == typeof(RenderLayerComponent))
            return componentsByEntityId.RenderLayer as Dictionary<int, T>;
        if (typeof(T) == typeof(ExplosionComponent))
            return componentsByEntityId.Explosion as Dictionary<int, T>;
        if (typeof(T) == typeof(DynamiteTag))
            return componentsByEntityId.DynamiteTag as Dictionary<int, T>;
        if (typeof(T) == typeof(PlayerTag))
            return componentsByEntityId.PlayerTag as Dictionary<int, T>;
        if (typeof(T) == typeof(MinerTag))
            return componentsByEntityId.MinerTag as Dictionary<int, T>;
        if (typeof(T) == typeof(ExplosionTag))
            return componentsByEntityId.ExplosionTag as Dictionary<int, T>;
        if (typeof(T) == typeof(LaunchConsoleTag))
            return componentsByEntityId.LaunchConsoleTag as Dictionary<int, T>;
        if (typeof(T) == typeof(NpcComponent))
            return componentsByEntityId.Npc as Dictionary<int, T>;
        if (typeof(T) == typeof(InteractiveComponent))
            return componentsByEntityId.Interactive as Dictionary<int, T>;

        throw new ArgumentException($"No handler for type {typeof(T)} in GetComponentsOfType");
    }
}