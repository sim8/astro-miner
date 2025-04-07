using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.InteriorsWorld;

public class InteriorsWorldState(GameState gameState) : BaseWorldState(gameState)
{
    public WorldCellType[,] Grid;

    public override void Initialize()
    {
        base.Initialize();
        Grid = WorldGrid.GetOizusGrid();

        InitializePlayer();
    }

    private void InitializePlayer()
    {
        var playerCellOffset = GameConfig.PlayerSize / 2;
        var playerPos = new Vector2(7f + playerCellOffset, 7f + playerCellOffset);

        var playerEntityId = gameState.Ecs.PlayerEntityId ?? gameState.Ecs.Factories.CreatePlayerEntity(playerPos);

        var playerPosition = gameState.Ecs.GetComponent<PositionComponent>(playerEntityId);
        playerPosition.Position = playerPos;
        playerPosition.IsOffAsteroid = false;
        playerPosition.World = World.Home;
        gameState.Ecs.SetActiveControllableEntity(playerEntityId);

        // Remove components that were added in AsteroidWorld
        gameState.Ecs.RemoveComponent<HealthComponent>(playerEntityId);
        gameState.Ecs.RemoveComponent<MiningComponent>(playerEntityId);
        gameState.Ecs.RemoveComponent<DirectionalLightSourceComponent>(playerEntityId);
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