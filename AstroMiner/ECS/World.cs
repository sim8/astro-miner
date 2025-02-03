using System;
using System.Collections.Generic;

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

    public World(GameState gameState)
    {
        _gameState = gameState;
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