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
        var posX = GameConfig.Launch.MinerHomeStartPosCenter.x - GameConfig.MinerSize / 2;
        var posY = GameConfig.Launch.MinerHomeStartPosCenter.y - GameConfig.MinerSize / 2;
        var minerPos = new Vector2(posX, posY);

        var minerEntityId = gameState.Ecs.MinerEntityId ?? gameState.Ecs.Factories.CreateMinerEntity(minerPos);

        var minerPosition = gameState.Ecs.GetComponent<PositionComponent>(minerEntityId);
        minerPosition.Position = minerPos;
        minerPosition.IsOffAsteroid = false;
        minerPosition.World = World.Home;
        var minerDirection = gameState.Ecs.GetComponent<DirectionComponent>(minerEntityId);
        minerDirection.Direction = Direction.Top;

        // Remove components that were added in AsteroidWorld
        gameState.Ecs.RemoveComponent<MovementComponent>(minerEntityId);
        gameState.Ecs.RemoveComponent<HealthComponent>(minerEntityId);
        gameState.Ecs.RemoveComponent<MiningComponent>(minerEntityId);
        gameState.Ecs.RemoveComponent<GrappleComponent>(minerEntityId);
        gameState.Ecs.RemoveComponent<DirectionalLightSourceComponent>(minerEntityId);
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
        if (activeMiningControls.Contains(MiningControls.NewGameOrReturnToBase))
            gameState.SetActiveWorldAndInitialize(World.Asteroid);

        base.Update(activeMiningControls, gameTime);
    }

    public override bool CellIsCollideable(int x, int y)
    {
        // TODO centralize out of bounds checks
        if (x < 0 || x >= Grid.GetLength(1) || y < 0 ||
            y >= Grid.GetLength(0)) return false;
        return Grid[y, x] == WorldCellType.Filled;
    }

    public override bool CellIsPortal(int x, int y)
    {
        // TODO centralize out of bounds checks
        if (x < 0 || x >= Grid.GetLength(1) || y < 0 ||
            y >= Grid.GetLength(0)) return false;
        return Grid[y, x] == WorldCellType.Portal;
    }
}