using System.Collections.Generic;

namespace AstroMiner.EntityComponentSystem;

public class Entity2Manager
{
    private int _nextId;
    public List<Entity2> Entities { get; } = new();

    public Entity2 CreateEntity()
    {
        var entity = new Entity2(_nextId++);
        Entities.Add(entity);
        return entity;
    }
}