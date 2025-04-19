using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.HomeWorld;

public class HomeWorldState(BaseGame game) : BaseWorldState(game)
{
    public WorldCellType[,] Grid;

    public override void Initialize()
    {
        base.Initialize();
        Grid = WorldGrid.GetOizusGrid();
    }

    public void InitializeOrResetEntities()
    {
        InitializeMiner();
        InitializePlayer();
        game.State.Ecs.LaunchSystem.Reset();
    }

    private void InitializeMiner()
    {
        var posX = Coordinates.Grid.MinerHomeStartPosCenter.x - GameConfig.MinerSize / 2;
        var posY = Coordinates.Grid.MinerHomeStartPosCenter.y - GameConfig.MinerSize / 2;
        var minerPos = new Vector2(posX, posY);

        var minerEntityId = game.State.Ecs.MinerEntityId ?? game.State.Ecs.Factories.CreateMinerEntity(minerPos);

        var minerPosition = game.State.Ecs.GetComponent<PositionComponent>(minerEntityId);
        minerPosition.Position = minerPos;
        minerPosition.IsOffAsteroid = false;
        minerPosition.World = World.Home;
        var minerDirection = game.State.Ecs.GetComponent<DirectionComponent>(minerEntityId);
        minerDirection.Direction = Direction.Top;

        // Remove components that were added in AsteroidWorld
        game.State.Ecs.RemoveComponent<MovementComponent>(minerEntityId);
        game.State.Ecs.RemoveComponent<HealthComponent>(minerEntityId);
        game.State.Ecs.RemoveComponent<MiningComponent>(minerEntityId);
        game.State.Ecs.RemoveComponent<GrappleComponent>(minerEntityId);
        game.State.Ecs.RemoveComponent<DirectionalLightSourceComponent>(minerEntityId);
    }

    private void InitializePlayer()
    {
        var playerCellOffset = GameConfig.PlayerSize / 2;
        var playerPos = new Vector2(Coordinates.Grid.PlayerHomeStartPos.x + playerCellOffset,
            Coordinates.Grid.PlayerHomeStartPos.y + playerCellOffset);

        var playerEntityId = game.State.Ecs.PlayerEntityId ?? game.State.Ecs.Factories.CreatePlayerEntity(playerPos);

        var playerPosition = game.State.Ecs.GetComponent<PositionComponent>(playerEntityId);
        playerPosition.Position = playerPos;
        playerPosition.IsOffAsteroid = false;
        playerPosition.World = World.Home;
        game.State.Ecs.SetActiveControllableEntity(playerEntityId);

        // Remove components that were added in AsteroidWorld
        game.State.Ecs.RemoveComponent<HealthComponent>(playerEntityId);
        game.State.Ecs.RemoveComponent<MiningComponent>(playerEntityId);
        game.State.Ecs.RemoveComponent<DirectionalLightSourceComponent>(playerEntityId);
    }

    public override void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (activeMiningControls.Contains(MiningControls.NewGameOrReturnToBase))
            game.State.SetActiveWorldAndInitialize(World.Asteroid);

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

    public override (int, int) GetGridSize()
    {
        return (Grid.GetLength(1), Grid.GetLength(0));
    }
}