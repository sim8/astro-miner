using System.Collections.Generic;

namespace AstroMiner.EntityComponentSystem;

public interface IComponentStorage
{
    void Remove(int entityId);
}

public class ComponentStorage<T> : IComponentStorage where T : struct
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