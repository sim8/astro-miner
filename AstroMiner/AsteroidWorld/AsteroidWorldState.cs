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

    public bool IsInMiner => game.Model.Ecs.ActiveControllableEntityId != null &&
                             game.StateManager.Ecs.HasComponent<MinerTag>(game.Model.Ecs
                                 .ActiveControllableEntityId
                                 .Value);

    public long MsTilExplosion =>
        Math.Max(game.Model.Asteroid.WillExplodeAt - game.StateManager.GetTotalPlayTime(), 0);

    private void InitSeed()
    {
        var rnd = new Random();
        game.Model.Asteroid.Seed = rnd.Next(1, 999);
    }

    public override (int, int) GetGridSize()
    {
        return (Grid.Columns, Grid.Rows);
    }

    public override void Initialize()
    {
        base.Initialize();
        InitSeed();
        var (grid, minerCenterPos) =
            AsteroidGen.InitializeGridAndStartingCenterPos(GameConfig.GridSize, game.Model.Asteroid.Seed);
        game.Model.Asteroid.Grid = grid;
        Grid = new GridState(game);
        game.Model.Asteroid.MinerStartingPos = minerCenterPos;
        var (minerPosX, minerPosY) = ViewHelpers.ToGridPosition(minerCenterPos);
        Grid.MarkAllDistancesFromExploredFloor(minerPosX, minerPosY, true);

        EdgeCells = UserInterfaceHelpers.GetAsteroidEdgeCells(Grid);
        CollapsingFloorTriggerer = new CollapsingFloorTriggerer(game);
        FogAnimationManager = new FogAnimationManager(game);
        game.Model.Asteroid.WillExplodeAt =
            game.StateManager.GetTotalPlayTime() + GameConfig.AsteroidExplodeTimeMs;
    }


    public override void Update(ActiveControls activeControls, GameTime gameTime)
    {
        if (game.StateManager.Ecs.ActiveControllableEntityIsDead ||
            game.StateManager.Ecs.ActiveControllableEntityIsOffAsteroid)
            if (activeControls.Mining.Contains(MiningControls.NewGameOrReturnToBase))
            {
                game.StateManager.Ecs.EntityTransformSystem.MoveMinerAndPlayerToShipDownstairs();
                game.StateManager.SetActiveWorldAndInitialize(World.ShipDownstairs);
            }

        if (MsTilExplosion <= 0)
        {
            game.StateManager.Ecs.HealthSystem.KillAllEntitiesInWorld();
            return;
        }

        base.Update(activeControls, gameTime);

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