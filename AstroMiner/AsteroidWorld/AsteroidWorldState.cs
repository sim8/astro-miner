using System;
using System.Collections.Generic;
using System.Linq;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.ProceduralGen;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.AsteroidWorld;

public class AsteroidWorldState(AstroMinerGame game) : BaseWorldState(game)
{
    public CollapsingFloorTriggerer CollapsingFloorTriggerer;
    public List<(int x, int y)> EdgeCells;
    public FogAnimationManager FogAnimationManager;
    public GridState Grid;

    public int Seed { get; private set; }

    public long MsSinceStart { get; private set; }

    public bool IsInMiner => game.State.Ecs.ActiveControllableEntityId != null &&
                             game.State.Ecs.HasComponent<MinerTag>(game.State.Ecs.ActiveControllableEntityId
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
        Grid = new GridState(game, grid);

        var (minerPosX, minerPosY) = ViewHelpers.ToGridPosition(minerPos);
        Grid.MarkAllDistancesFromExploredFloor(minerPosX, minerPosY, true);

        InitializeMiner(minerPos);
        InitializePlayer(minerPos);

        EdgeCells = UserInterfaceHelpers.GetAsteroidEdgeCells(Grid);
        CollapsingFloorTriggerer = new CollapsingFloorTriggerer(game);
        MsSinceStart = 0;
        FogAnimationManager = new FogAnimationManager(game);
    }

    private void InitializeMiner(Vector2 minerPos)
    {
        var entityId = game.State.Ecs.MinerEntityId.Value;

        var minerPosition = game.State.Ecs.GetComponent<PositionComponent>(entityId);
        minerPosition.Position = minerPos;
        minerPosition.World = World.Asteroid;

        // Add movement component with miner-specific values
        var movementComponent = game.State.Ecs.AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = GameConfig.Speeds.Running;
        movementComponent.TimeToReachMaxSpeedMs = 600; // From MinerEntity
        movementComponent.TimeToStopMs = 400; // From MinerEntity

        // Add health component
        var healthComponent = game.State.Ecs.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.MinerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.MinerMaxHealth;

        // Add mining component
        var miningComponent = game.State.Ecs.AddComponent<MiningComponent>(entityId);
        miningComponent.DrillingWidth = 0.9f;

        // Add grapple component
        game.State.Ecs.AddComponent<GrappleComponent>(entityId);

        // Add directional light source component
        var directionalLightSourceComponent = game.State.Ecs.AddComponent<DirectionalLightSourceComponent>(entityId);
        directionalLightSourceComponent.TopOffset = new Vector2(1.06f, 0.34f);
        directionalLightSourceComponent.RightOffset = new Vector2(0.70f, 0.66f);
        directionalLightSourceComponent.BottomOffset = new Vector2(0.12f, 0.58f);
        directionalLightSourceComponent.LeftOffset = new Vector2(0.48f, -0.28f);

        // TODO needed?
        game.State.Ecs.SetActiveControllableEntity(game.State.Ecs.MinerEntityId.Value);
    }

    private void InitializePlayer(Vector2 playerPos)
    {
        var entityId = game.State.Ecs.PlayerEntityId.Value;

        var playerPosition = game.State.Ecs.GetComponent<PositionComponent>(entityId);
        playerPosition.Position = playerPos;
        playerPosition.World = World.Asteroid;

        // Add health component
        var healthComponent = game.State.Ecs.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.PlayerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.PlayerMaxHealth;

        // Add mining component
        game.State.Ecs.AddComponent<MiningComponent>(entityId);

        // Add directional light source component
        var directionalLightSourceComponent = game.State.Ecs.AddComponent<DirectionalLightSourceComponent>(entityId);
        directionalLightSourceComponent.TopOffset = new Vector2(0.28f, -0.30f);
        directionalLightSourceComponent.RightOffset = new Vector2(0.32f, -0.30f);
        directionalLightSourceComponent.BottomOffset = new Vector2(0.24f, -0.30f);
        directionalLightSourceComponent.LeftOffset = new Vector2(0.26f, -0.28f);
    }

    public override void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (game.State.Ecs.ActiveControllableEntityIsDead ||
            game.State.Ecs.ActiveControllableEntityIsOffAsteroid)
            if (activeMiningControls.Contains(MiningControls.NewGameOrReturnToBase))
            {
                game.State.SetActiveWorldAndInitialize(World.Home);
                game.State.HomeWorld.InitializeOrResetEntities();
            }

        MsSinceStart += gameTime.ElapsedGameTime.Milliseconds;

        if (MsSinceStart > GameConfig.AsteroidExplodeTimeMs)
        {
            game.State.Ecs.HealthSystem.KillAllEntitiesInWorld();
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