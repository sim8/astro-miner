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
        Grid = StaticWorlds.StaticWorldConfigs[World.Home].World;
    }

    public void ResetEntities()
    {
        ResetMiner();
        ResetPlayer();
    }

    private void ResetMiner()
    {
        var posX = Coordinates.Grid.MinerHomeStartPosCenter.x - GameConfig.MinerSize / 2;
        var posY = Coordinates.Grid.MinerHomeStartPosCenter.y - GameConfig.MinerSize / 2;
        var minerPos = new Vector2(posX, posY);

        var minerEntityId = game.StateManager.Ecs.MinerEntityId.Value;

        var minerPosition = game.StateManager.Ecs.GetComponent<PositionComponent>(minerEntityId);
        minerPosition.Position = minerPos;
        minerPosition.IsOffAsteroid = false;
        minerPosition.World = World.Home;
        var minerDirection = game.StateManager.Ecs.GetComponent<DirectionComponent>(minerEntityId);
        minerDirection.Direction = Direction.Top;

        // Remove components that were added in AsteroidWorld
        game.StateManager.Ecs.RemoveComponent<MovementComponent>(minerEntityId);
        game.StateManager.Ecs.RemoveComponent<HealthComponent>(minerEntityId);
        game.StateManager.Ecs.RemoveComponent<MiningComponent>(minerEntityId);
        game.StateManager.Ecs.RemoveComponent<GrappleComponent>(minerEntityId);
        game.StateManager.Ecs.RemoveComponent<DirectionalLightSourceComponent>(minerEntityId);
    }

    private void ResetPlayer()
    {
        var playerCellOffset = GameConfig.PlayerSize / 2;
        var playerPos = new Vector2(Coordinates.Grid.PlayerHomeStartPos.x + playerCellOffset,
            Coordinates.Grid.PlayerHomeStartPos.y + playerCellOffset);

        var playerEntityId = game.StateManager.Ecs.PlayerEntityId.Value;

        var playerPosition = game.StateManager.Ecs.GetComponent<PositionComponent>(playerEntityId);
        playerPosition.Position = playerPos;
        playerPosition.IsOffAsteroid = false;
        playerPosition.World = World.Home;
        game.StateManager.Ecs.SetActiveControllableEntity(playerEntityId);

        // Remove components that were added in AsteroidWorld
        game.StateManager.Ecs.RemoveComponent<HealthComponent>(playerEntityId);
        game.StateManager.Ecs.RemoveComponent<MiningComponent>(playerEntityId);
        game.StateManager.Ecs.RemoveComponent<DirectionalLightSourceComponent>(playerEntityId);
    }

    public override void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (activeMiningControls.Contains(MiningControls.NewGameOrReturnToBase))
            game.StateManager.SetActiveWorldAndInitialize(World.Asteroid);

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