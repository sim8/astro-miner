using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.HomeWorld;

public class HomeWorldState(GameState gameState) : BaseWorldState(gameState)
{
    public WorldCellType[,] Grid;

    public override void Initialize()
    {
        base.Initialize();
        Grid = WorldGrid.GetOizusGrid();

        // Create or update ECS entities
        if (gameState.Ecs.MinerEntityId == null)
        {
            gameState.Ecs.Factories.CreateMinerEntity(new Vector2(0.2f, 0.2f));
        }
        else
        {
            var minerPosition = gameState.Ecs.GetComponent<PositionComponent>(gameState.Ecs.MinerEntityId.Value);
            minerPosition.Position = new Vector2(0.2f, 0.2f);
            minerPosition.IsOffAsteroid = false;
            minerPosition.World = World.Home;
        }

        if (gameState.Ecs.PlayerEntityId == null)
        {
            var playerEntityId = gameState.Ecs.Factories.CreatePlayerEntity(new Vector2(1.5f, 1.5f));
            gameState.Ecs.SetActiveControllableEntity(playerEntityId);
        }
        else
        {
            var playerPosition = gameState.Ecs.GetComponent<PositionComponent>(gameState.Ecs.PlayerEntityId.Value);
            playerPosition.Position = new Vector2(1.5f, 1.5f);
            playerPosition.IsOffAsteroid = false;
            playerPosition.World = World.Home;
            gameState.Ecs.SetActiveControllableEntity(gameState.Ecs.PlayerEntityId.Value);
        }
    }

    public override void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (activeMiningControls.Contains(MiningControls.NewGameOrReturnToBase)) gameState.InitializeAsteroid();

        base.Update(activeMiningControls, gameTime);
    }

    public override bool CellIsCollideable(int x, int y)
    {
        // TODO centralize out of bounds checks
        if (x < 0 || x >= Grid.GetLength(1) || y < 0 ||
            y >= Grid.GetLength(0)) return false;
        return Grid[y, x] == WorldCellType.Filled;
    }
}