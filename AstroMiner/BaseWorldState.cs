using System.Collections.Generic;
using System.Linq;
using AstroMiner.Entities;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class BaseWorldState(GameState g)
{
    private readonly HashSet<MiningControls> _emptyMiningControls = [];
    public List<Entity> ActiveEntitiesSortedByDistance = [];
    public PlayerEntity Player;

    public virtual MiningControllableEntity ActiveControllableEntity => Player;

    public virtual void Initialize()
    {
    }

    private void SortActiveEntities()
    {
        ActiveEntitiesSortedByDistance.Sort((a, b) => a.FrontY.CompareTo(b.FrontY));
    }

    public void ActivateEntity(Entity entity)
    {
        ActiveEntitiesSortedByDistance.Add(entity);
    }

    public void DeactivateEntity(Entity entity)
    {
        ActiveEntitiesSortedByDistance.Remove(entity);
    }

    public virtual void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        foreach (var entity in ActiveEntitiesSortedByDistance.ToList())
            if (entity is MiningControllableEntity && entity == g.ActiveControllableEntity)
                entity.Update(gameTime, activeMiningControls);
            else
                entity.Update(gameTime, _emptyMiningControls);


        // Do last to reflect changes
        SortActiveEntities(); // TODO only call when needed? Seems error prone
    }
}