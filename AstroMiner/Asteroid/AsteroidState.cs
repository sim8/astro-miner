using System;
using System.Collections.Generic;
using System.Linq;
using AstroMiner.Definitions;
using AstroMiner.Entities;
using AstroMiner.ProceduralGen;
using AstroMiner.Utilities;

namespace AstroMiner.Asteroid;

public class AsteroidState
{
    private readonly GameState _gameState;
    private HashSet<MiningControls> _emptyMiningControls;
    public List<Entity> ActiveEntitiesSortedByDistance;
    public CollapsingFloorTriggerer CollapsingFloorTriggerer;
    public List<(int x, int y)> EdgeCells;
    public FogAnimationManager FogAnimationManager;
    public GridState Grid;
    public MinerEntity Miner;
    public PlayerEntity Player;

    public AsteroidState(
        GameState gameState)
    {
        _gameState = gameState;
        Initialize();
    }

    public int Seed { get; private set; }

    public long MsSinceStart { get; private set; }

    public bool IsInMiner => !ActiveEntitiesSortedByDistance.Contains(Player);


    public MiningControllableEntity ActiveControllableEntity => IsInMiner ? Miner : Player;

    private void InitSeed()
    {
        var rnd = new Random();
        Seed = rnd.Next(1, 999);
    }

    public void Initialize()
    {
        InitSeed();
        var (grid, minerPos) = AsteroidGen.InitializeGridAndStartingPos(GameConfig.GridSize, Seed);
        Grid = new GridState(_gameState, grid);

        var (minerPosX, minerPosY) = ViewHelpers.ToGridPosition(minerPos);
        Grid.MarkAllDistancesFromExploredFloor(minerPosX, minerPosY, true);
        Miner = new MinerEntity(_gameState);
        _emptyMiningControls = new HashSet<MiningControls>();
        Miner.Initialize(minerPos);
        Player = new PlayerEntity(_gameState);
        Player.Initialize(minerPos);
        EdgeCells = UserInterfaceHelpers.GetAsteroidEdgeCells(Grid);
        CollapsingFloorTriggerer = new CollapsingFloorTriggerer(_gameState);
        ActiveEntitiesSortedByDistance = [Miner];
        MsSinceStart = 0;
        FogAnimationManager = new FogAnimationManager(_gameState);
    }

    private void SortActiveEntities()
    {
        ActiveEntitiesSortedByDistance.Sort((a, b) => a.FrontY.CompareTo(b.FrontY));
    }

    public void ActivateEntity(Entity entity)
    {
        ActiveEntitiesSortedByDistance.Add(entity);
    }

    public void DeactivateEntity(Entity entity)
    {
        ActiveEntitiesSortedByDistance.Remove(entity);
    }

    public void Update(HashSet<MiningControls> activeMiningControls, int elapsedMs)
    {
        if (ActiveControllableEntity.IsDead || ActiveControllableEntity.IsOffAsteroid)
        {
            if (activeMiningControls.Contains(MiningControls.NewGame)) Initialize();
            return;
        }

        MsSinceStart += elapsedMs;

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

        foreach (var entity in ActiveEntitiesSortedByDistance.ToList())
            if (entity is MiningControllableEntity && entity == ActiveControllableEntity)
                entity.Update(elapsedMs, activeMiningControls);
            else
                entity.Update(elapsedMs, _emptyMiningControls);

        foreach (var cell in Grid._activeExplosiveRockCells) cell.Value.Update(elapsedMs);
        foreach (var cell in Grid._activeCollapsingFloorCells.Values.ToList()) cell.Update(elapsedMs);


        // Do last to reflect changes
        SortActiveEntities(); // TODO only call when needed? Seems error prone

        CollapsingFloorTriggerer.Update(elapsedMs);

        FogAnimationManager.Update(elapsedMs);
    }
}