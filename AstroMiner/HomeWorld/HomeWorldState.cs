using System.Collections.Generic;
using System.Linq;
using AstroMiner.Definitions;
using AstroMiner.Entities;
using Microsoft.Xna.Framework;

namespace AstroMiner.HomeWorld;

public class HomeWorldState(GameState gameState)
{
    private HashSet<MiningControls> _emptyMiningControls;
    public List<Entity> ActiveEntitiesSortedByDistance;
    public WorldCellType[,] Grid;
    public PlayerEntity Player;

    public void Initialize()
    {
        Grid = WorldGrid.GetOizusGrid();
        Player = new PlayerEntity(gameState);
        Player.Position = new Vector2(1.5f, 1.5f);
        _emptyMiningControls = new HashSet<MiningControls>();
        ActiveEntitiesSortedByDistance = [Player];
    }

    private void SortActiveEntities()
    {
        ActiveEntitiesSortedByDistance.Sort((a, b) => a.FrontY.CompareTo(b.FrontY));
    }

    public void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        foreach (var entity in ActiveEntitiesSortedByDistance.ToList())
            if (entity is MiningControllableEntity && entity == gameState.ActiveControllableEntity)
                entity.Update(gameTime, activeMiningControls);
            else
                entity.Update(gameTime, _emptyMiningControls);


        // Do last to reflect changes
        SortActiveEntities(); // TODO only call when needed? Seems error prone
    }
}