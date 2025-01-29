using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.Entities;
using Microsoft.Xna.Framework;

namespace AstroMiner.HomeWorld;

public class HomeWorldState(GameState gameState) : BaseWorldState(gameState)
{
    public WorldCellType[,] Grid;

    public override void Initialize()
    {
        base.Initialize();
        Grid = WorldGrid.GetOizusGrid();
        Player = new PlayerEntity(gameState);
        Player.Initialize(new Vector2(1.5f, 1.5f));
        ActiveEntitiesSortedByDistance = [Player];
    }

    public override void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (activeMiningControls.Contains(MiningControls.NewGameOrReturnToBase)) gameState.InitializeAsteroid();

        base.Update(activeMiningControls, gameTime);
    }
}