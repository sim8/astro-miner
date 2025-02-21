using System;
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

        InitializeMiner();
        InitializePlayer();
    }

    private void InitializeMiner()
    {

        var minerCellOffset = 1f - GameConfig.MinerSize / 2;
        var minerPos = new Vector2(1f + minerCellOffset, 3f + minerCellOffset);

        var minerEntityId = gameState.Ecs.MinerEntityId ?? gameState.Ecs.Factories.CreateMinerEntity(minerPos);

        var minerPosition = gameState.Ecs.GetComponent<PositionComponent>(minerEntityId);
        minerPosition.Position = minerPos;
        minerPosition.IsOffAsteroid = false;
        minerPosition.World = World.Home;
        var minerDirection = gameState.Ecs.GetComponent<DirectionComponent>(minerEntityId);
        minerDirection.Direction = Direction.Top;
    }

    private void InitializePlayer()
    {
        var playerCellOffset = GameConfig.PlayerSize / 2;
        var playerPos = new Vector2(4f + playerCellOffset, 7f + playerCellOffset);



        var playerEntityId = gameState.Ecs.PlayerEntityId ?? gameState.Ecs.Factories.CreatePlayerEntity(playerPos);

        var playerPosition = gameState.Ecs.GetComponent<PositionComponent>(playerEntityId);
        playerPosition.Position = playerPos;
        playerPosition.IsOffAsteroid = false;
        playerPosition.World = World.Home;
        gameState.Ecs.SetActiveControllableEntity(playerEntityId);
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