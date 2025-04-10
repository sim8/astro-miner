using System;
using System.Collections.Generic;
using System.Linq;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.ProceduralGen;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.AsteroidWorld;

public class AsteroidWorldState(GameState gameState) : BaseWorldState(gameState)
{
    public CollapsingFloorTriggerer CollapsingFloorTriggerer;
    public List<(int x, int y)> EdgeCells;
    public FogAnimationManager FogAnimationManager;
    public GridState Grid;

    public int Seed { get; private set; }

    public long MsSinceStart { get; private set; }

    public bool IsInMiner => gameState.Ecs.ActiveControllableEntityId != null &&
                             gameState.Ecs.HasComponent<MinerTag>(gameState.Ecs.ActiveControllableEntityId
                                 .Value);

    private void InitSeed()
    {
        var rnd = new Random();
        Seed = rnd.Next(1, 999);
    }

    public override (int, int) GetGridSize()
    {
        return (Grid.Columns, Grid.Rows);
    }

    public override void Initialize()
    {
        base.Initialize();
        InitSeed();
        var (grid, minerPos) = AsteroidGen.InitializeGridAndStartingPos(GameConfig.GridSize, Seed);
        Grid = new GridState(gameState, grid);

        var (minerPosX, minerPosY) = ViewHelpers.ToGridPosition(minerPos);
        Grid.MarkAllDistancesFromExploredFloor(minerPosX, minerPosY, true);

        InitializeMiner(minerPos);
        InitializePlayer(minerPos);

        EdgeCells = UserInterfaceHelpers.GetAsteroidEdgeCells(Grid);
        CollapsingFloorTriggerer = new CollapsingFloorTriggerer(gameState);
        MsSinceStart = 0;
        FogAnimationManager = new FogAnimationManager(gameState);
    }

    private void InitializeMiner(Vector2 minerPos)
    {
        var entityId = gameState.Ecs.MinerEntityId.Value;

        var minerPosition = gameState.Ecs.GetComponent<PositionComponent>(entityId);
        minerPosition.Position = minerPos;
        minerPosition.World = World.Asteroid;

        // Add movement component with miner-specific values
        var movementComponent = gameState.Ecs.AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = 4f; // From MinerEntity
        movementComponent.TimeToReachMaxSpeedMs = 600; // From MinerEntity
        movementComponent.TimeToStopMs = 400; // From MinerEntity

        // Add health component
        var healthComponent = gameState.Ecs.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.MinerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.MinerMaxHealth;

        // Add mining component
        var miningComponent = gameState.Ecs.AddComponent<MiningComponent>(entityId);
        miningComponent.DrillingWidth = 0.9f;

        // Add grapple component
        gameState.Ecs.AddComponent<GrappleComponent>(entityId);

        // Add directional light source component
        var directionalLightSourceComponent = gameState.Ecs.AddComponent<DirectionalLightSourceComponent>(entityId);
        directionalLightSourceComponent.TopOffset = new Vector2(1.06f, 0.34f);
        directionalLightSourceComponent.RightOffset = new Vector2(0.70f, 0.66f);
        directionalLightSourceComponent.BottomOffset = new Vector2(0.12f, 0.58f);
        directionalLightSourceComponent.LeftOffset = new Vector2(0.48f, -0.28f);

        // TODO needed?
        gameState.Ecs.SetActiveControllableEntity(gameState.Ecs.MinerEntityId.Value);
    }

    private void InitializePlayer(Vector2 playerPos)
    {
        var entityId = gameState.Ecs.PlayerEntityId.Value;

        var playerPosition = gameState.Ecs.GetComponent<PositionComponent>(entityId);
        playerPosition.Position = playerPos;
        playerPosition.World = World.Asteroid;

        // Add health component
        var healthComponent = gameState.Ecs.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.PlayerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.PlayerMaxHealth;

        // Add mining component
        gameState.Ecs.AddComponent<MiningComponent>(entityId);

        // Add directional light source component
        var directionalLightSourceComponent = gameState.Ecs.AddComponent<DirectionalLightSourceComponent>(entityId);
        directionalLightSourceComponent.TopOffset = new Vector2(0.28f, -0.30f);
        directionalLightSourceComponent.RightOffset = new Vector2(0.32f, -0.30f);
        directionalLightSourceComponent.BottomOffset = new Vector2(0.24f, -0.30f);
        directionalLightSourceComponent.LeftOffset = new Vector2(0.26f, -0.28f);
    }

    public override void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (gameState.Ecs.ActiveControllableEntityIsDead ||
            gameState.Ecs.ActiveControllableEntityIsOffAsteroid)
            if (activeMiningControls.Contains(MiningControls.NewGameOrReturnToBase))
            {
                gameState.SetActiveWorldAndInitialize(World.Home);
                gameState.HomeWorld.InitializeOrResetEntities();
            }

        MsSinceStart += gameTime.ElapsedGameTime.Milliseconds;

        if (MsSinceStart > GameConfig.AsteroidExplodeTimeMs)
        {
            gameState.Ecs.HealthSystem.KillAllEntitiesInWorld();
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