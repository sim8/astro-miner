using System.Collections.Generic;

namespace AstroMiner.Entities;

public class ComponentStorage<T> where T : struct
{
    private readonly Dictionary<int, T> _components = new();

    public void Add(int entityId, T component)
    {
        _components[entityId] = component;
    }

    public bool Has(int entityId)
    {
        return _components.ContainsKey(entityId);
    }

    public T Get(int entityId)
    {
        return _components[entityId];
    }

    public void Remove(int entityId)
    {
        _components.Remove(entityId);
    }

    public IEnumerable<KeyValuePair<int, T>> All()
    {
        return _components;
    }
}