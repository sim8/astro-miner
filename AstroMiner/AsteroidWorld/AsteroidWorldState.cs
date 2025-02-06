using System;
using System.Collections.Generic;
using System.Linq;
using AstroMiner.Definitions;
using AstroMiner.Entities;
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
    public MinerEntity Miner;

    public int Seed { get; private set; }

    public long MsSinceStart { get; private set; }

    public bool IsInMiner => !ActiveEntitiesSortedByDistance.Contains(Player);


    public override MiningControllableEntity ActiveControllableEntity => IsInMiner ? Miner : Player;

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
        
        // Create legacy miner
        Miner = new MinerEntity(gameState);
        Miner.Initialize(minerPos);

        // Create legacy player
        Player = new PlayerEntity(gameState);
        Player.Initialize(minerPos);
        
        // Create ECS entities
        var minerEntityId = gameState.EcsWorld.CreateMinerEntity(minerPos);
        gameState.EcsWorld.CreatePlayerEntity(minerPos);
        gameState.EcsWorld.SetActiveControllableEntity(minerEntityId);
        
        EdgeCells = UserInterfaceHelpers.GetAsteroidEdgeCells(Grid);
        CollapsingFloorTriggerer = new CollapsingFloorTriggerer(gameState);
        ActiveEntitiesSortedByDistance = [Miner];
        MsSinceStart = 0;
        FogAnimationManager = new FogAnimationManager(gameState);
    }

    public override void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (ActiveControllableEntity.IsDead || ActiveControllableEntity.IsOffAsteroid)
            if (activeMiningControls.Contains(MiningControls.NewGameOrReturnToBase))
                gameState.InitializeHome();

        MsSinceStart += gameTime.ElapsedGameTime.Milliseconds;

        if (MsSinceStart > GameConfig.AsteroidExplodeTimeMs)
        {
            ActiveControllableEntity.IsDead = true;
            return;
        }

        if (activeMiningControls.Contains(MiningControls.EnterOrExit))
        {
            ActiveControllableEntity.Disembark();
            if (ActiveControllableEntity == Player && !Miner.IsDead &&
                Player.GetDistanceTo(Miner) < GameConfig.MinEmbarkingDistance)
            {
                DeactivateEntity(Player);
            }
            else if (ActiveControllableEntity == Miner)
            {
                Player.Position = Miner.Position;
                ActivateEntity(Player);
            }
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