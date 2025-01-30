using AstroMiner.Entities;
using Microsoft.Xna.Framework;

namespace AstroMiner.EntityComponentSystem.Systems;

public class ExplodableSystem(
    GameState gameState,
    ComponentStorage<Explodable> explodableStorage,
    ComponentStorage<PositionAndSize> posAndSizeStorage) : ISystem
{
    public void Update(GameTime gameTime)
    {
        foreach (var (entityId, _) in explodableStorage.All())
            if (posAndSizeStorage.Has(entityId))
            {
                var positionAndSize = posAndSizeStorage.Get(entityId);

                var explodable = explodableStorage.Get(entityId);
                explodable.TimeRemainingMs -= gameTime.ElapsedGameTime.Milliseconds;

                if (explodable.TimeRemainingMs <= 0)
                {
                    var explosionEntity = new ExplosionEntity(gameState, positionAndSize.CenterPosition);
                    gameState.AsteroidWorld.ActivateEntity(explosionEntity);
                    gameState.Ecs.RemoveEntity(entityId);
                }
            }
    }
}