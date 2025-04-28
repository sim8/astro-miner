using System;
using System.Collections.Generic;
using System.Linq;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.ProceduralGen;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.AsteroidWorld;

public class AsteroidWorldState(BaseGame game) : BaseWorldState(game)
{
    public CollapsingFloorTriggerer CollapsingFloorTriggerer;
    public List<(int x, int y)> EdgeCells;
    public FogAnimationManager FogAnimationManager;
    public GridState Grid;

    public long MsSinceStart { get; private set; }

    public bool IsInMiner => game.Model.Ecs.ActiveControllableEntityId != null &&
                             game.StateManager.Ecs.HasComponent<MinerTag>(game.Model.Ecs
                                 .ActiveControllableEntityId
                                 .Value);

    private void InitSeed()
    {
        var rnd = new Random();
        game.Model.AsteroidModel.Seed = rnd.Next(1, 999);
    }

    public override (int, int) GetGridSize()
    {
        return (Grid.Columns, Grid.Rows);
    }

    public override void Initialize()
    {
        base.Initialize();
        InitSeed();
        var (grid, minerPos) =
            AsteroidGen.InitializeGridAndStartingPos(GameConfig.GridSize, game.Model.AsteroidModel.Seed);
        game.Model.AsteroidModel.Grid = grid;
        Grid = new GridState(game);

        var (minerPosX, minerPosY) = ViewHelpers.ToGridPosition(minerPos);
        Grid.MarkAllDistancesFromExploredFloor(minerPosX, minerPosY, true);

        InitializeMiner(minerPos);
        InitializePlayer(minerPos);

        EdgeCells = UserInterfaceHelpers.GetAsteroidEdgeCells(Grid);
        CollapsingFloorTriggerer = new CollapsingFloorTriggerer(game);
        FogAnimationManager = new FogAnimationManager(game);
        game.Model.AsteroidModel.WillExplodeAt =
            game.StateManager.GetTotalPlayTime() + GameConfig.AsteroidExplodeTimeMs;
    }

    private void InitializeMiner(Vector2 minerPos)
    {
        var entityId = game.Model.Ecs.MinerEntityId.Value;

        var minerPosition = game.StateManager.Ecs.GetComponent<PositionComponent>(entityId);
        minerPosition.Position = minerPos;
        minerPosition.World = World.Asteroid;

        // Add movement component with miner-specific values
        var movementComponent = game.StateManager.Ecs.AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = GameConfig.Speeds.Running;
        movementComponent.TimeToReachMaxSpeedMs = 600; // From MinerEntity
        movementComponent.TimeToStopMs = 400; // From MinerEntity

        // Add health component
        var healthComponent = game.StateManager.Ecs.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.MinerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.MinerMaxHealth;

        // Add mining component
        var miningComponent = game.StateManager.Ecs.AddComponent<MiningComponent>(entityId);
        miningComponent.DrillingWidth = 0.9f;

        // Add grapple component
        game.StateManager.Ecs.AddComponent<GrappleComponent>(entityId);

        // Add directional light source component
        var directionalLightSourceComponent =
            game.StateManager.Ecs.AddComponent<DirectionalLightSourceComponent>(entityId);
        directionalLightSourceComponent.TopOffset = new Vector2(1.06f, 0.34f);
        directionalLightSourceComponent.RightOffset = new Vector2(0.70f, 0.66f);
        directionalLightSourceComponent.BottomOffset = new Vector2(0.12f, 0.58f);
        directionalLightSourceComponent.LeftOffset = new Vector2(0.48f, -0.28f);

        // TODO needed?
        game.StateManager.Ecs.SetActiveControllableEntity(game.Model.Ecs.MinerEntityId.Value);
    }

    private void InitializePlayer(Vector2 playerPos)
    {
        var entityId = game.Model.Ecs.PlayerEntityId.Value;

        var playerPosition = game.StateManager.Ecs.GetComponent<PositionComponent>(entityId);
        playerPosition.Position = playerPos;
        playerPosition.World = World.Asteroid;

        // Add health component
        var healthComponent = game.StateManager.Ecs.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.PlayerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.PlayerMaxHealth;

        // Add mining component
        game.StateManager.Ecs.AddComponent<MiningComponent>(entityId);

        // Add directional light source component
        var directionalLightSourceComponent =
            game.StateManager.Ecs.AddComponent<DirectionalLightSourceComponent>(entityId);
        directionalLightSourceComponent.TopOffset = new Vector2(0.28f, -0.30f);
        directionalLightSourceComponent.RightOffset = new Vector2(0.32f, -0.30f);
        directionalLightSourceComponent.BottomOffset = new Vector2(0.24f, -0.30f);
        directionalLightSourceComponent.LeftOffset = new Vector2(0.26f, -0.28f);
    }

    public override void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (game.StateManager.Ecs.ActiveControllableEntityIsDead ||
            game.StateManager.Ecs.ActiveControllableEntityIsOffAsteroid)
            if (activeMiningControls.Contains(MiningControls.NewGameOrReturnToBase))
            {
                game.StateManager.SetActiveWorldAndInitialize(World.Home);
                game.StateManager.HomeWorld.InitializeOrResetEntities();
            }

        MsSinceStart += gameTime.ElapsedGameTime.Milliseconds;

        if (MsSinceStart > GameConfig.AsteroidExplodeTimeMs)
        {
            game.StateManager.Ecs.HealthSystem.KillAllEntitiesInWorld();
            return;
        }

        base.Update(activeMiningControls, gameTime);

        foreach (var cell in Grid._activeExplosiveRockCells) cell.Value.Update(gameTime);
        foreach (var cell in Grid._activeCollapsingFloorCells.Values.ToList()) cell.Update(gameTime);

        CollapsingFloorTriggerer.Update(gameTime);

        FogAnimationManager.Update(gameTime);
    }

    public override bool CellIsCollideable(int x, int y)
    {
        return Grid.GetWallType(x, y) != WallType.Empty;
    }

    public override bool CellIsPortal(int x, int y)
    {
        return false;
    }
}