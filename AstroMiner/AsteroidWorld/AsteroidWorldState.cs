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

    public bool IsInMiner => gameState.EcsWorld.ActiveControllableEntityId != null &&
                             gameState.EcsWorld.HasComponent<MinerTag>(gameState.EcsWorld.ActiveControllableEntityId
                                 .Value);

    private void InitSeed()
    {
        var rnd = new Random();
        Seed = rnd.Next(1, 999);
    }

    public override void Initialize()
    {
        base.Initialize();
        InitSeed();
        var (grid, minerPos) = AsteroidGen.InitializeGridAndStartingPos(GameConfig.GridSize, Seed);
        Grid = new GridState(gameState, grid);

        var (minerPosX, minerPosY) = ViewHelpers.ToGridPosition(minerPos);
        Grid.MarkAllDistancesFromExploredFloor(minerPosX, minerPosY, true);
        // Create ECS entities
        var minerEntityId = gameState.EcsWorld.CreateMinerEntity(minerPos);
        gameState.EcsWorld.CreatePlayerEntity(minerPos);
        gameState.EcsWorld.SetActiveControllableEntity(minerEntityId);

        EdgeCells = UserInterfaceHelpers.GetAsteroidEdgeCells(Grid);
        CollapsingFloorTriggerer = new CollapsingFloorTriggerer(gameState);
        MsSinceStart = 0;
        FogAnimationManager = new FogAnimationManager(gameState);
    }

    public override void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (gameState.EcsWorld.ActiveControllableEntityIsDead ||
            gameState.EcsWorld.ActiveControllableEntityIsOffAsteroid)
            if (activeMiningControls.Contains(MiningControls.NewGameOrReturnToBase))
                gameState.InitializeHome();

        MsSinceStart += gameTime.ElapsedGameTime.Milliseconds;

        if (MsSinceStart > GameConfig.AsteroidExplodeTimeMs)
        {
            gameState.HealthSystem.KillAllEntities();
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
}