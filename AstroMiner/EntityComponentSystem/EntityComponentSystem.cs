using System.Collections.Generic;
using AstroMiner.EntityComponentSystem.Systems;
using Microsoft.Xna.Framework;

namespace AstroMiner.EntityComponentSystem;

public class EntityComponentSystem
{
    private readonly List<IComponentStorage> _allStorage;
    private readonly ComponentStorage<Explodable> _explodableStorage = new();
    private readonly GameState _gameState;
    private readonly ComponentStorage<PositionAndSize> _positionAndSizeStorage = new();
    private readonly List<ISystem> _systems;

    private int _nextId;

    public EntityComponentSystem(GameState gameState)
    {
        _gameState = gameState;
        _allStorage =
        [
            _positionAndSizeStorage,
            _explodableStorage
        ];
        _systems = [new ExplodableSystem(_gameState, _explodableStorage, _positionAndSizeStorage)];
    }

    public List<Entity2> Entities { get; } = new();

    public Entity2 CreateEntity()
    {
        var entity = new Entity2(_nextId++);
        Entities.Add(entity);
        return entity;
    }

    public void RemoveEntity(int entityId)
    {
        // Remove from entity list
        Entities.RemoveAll(e => e.Id == entityId);

        // Remove all associated components
        foreach (var componentStorage in _allStorage) componentStorage.Remove(entityId);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var system in _systems) system.Update(gameTime);
    }
}