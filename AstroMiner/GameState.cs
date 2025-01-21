using System;
using System.Collections.Generic;
using System.Linq;
using AstroMiner.Definitions;
using AstroMiner.Entities;
using AstroMiner.ProceduralGen;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public enum MiningControls
{
    // Movement
    MoveUp,
    MoveRight,
    MoveDown,
    MoveLeft,

    Drill,
    EnterOrExit,

    // Player-only
    PlaceDynamite,

    CycleZoom,
    NewGame // TODO factor out
}

public enum Direction
{
    Top,
    Right,
    Bottom,
    Left
}

public class GameState
{
    public readonly GraphicsDeviceManager Graphics;
    private HashSet<MiningControls> _emptyMiningControls;
    public List<Entity> ActiveEntitiesSortedByDistance;
    public CameraState Camera;
    public CloudManager CloudManager;
    public CollapsingFloorTriggerer CollapsingFloorTriggerer;
    public List<(int x, int y)> EdgeCells;
    public GridState Grid;
    public Inventory Inventory;
    public MinerEntity Miner;
    public PlayerEntity Player;

    public GameState(
        GraphicsDeviceManager graphics)
    {
        Graphics = graphics;
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
        Grid = new GridState(this, grid);

        var (minerPosX, minerPosY) = ViewHelpers.ToGridPosition(minerPos);
        Grid.MarkAllDistancesFromExploredFloor(minerPosX, minerPosY);
        Miner = new MinerEntity(this);
        Miner.Initialize(minerPos);
        Player = new PlayerEntity(this);
        Player.Initialize(minerPos);
        Inventory = new Inventory();
        EdgeCells = UserInterfaceHelpers.GetAsteroidEdgeCells(Grid);
        Camera = new CameraState(this);
        CollapsingFloorTriggerer = new CollapsingFloorTriggerer(this);
        ActiveEntitiesSortedByDistance = [Miner];
        _emptyMiningControls = new HashSet<MiningControls>();
        MsSinceStart = 0;
        CloudManager = new CloudManager(this);
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

        Camera.Update(elapsedMs, activeMiningControls);

        CloudManager.Update(elapsedMs);

        CollapsingFloorTriggerer.Update(elapsedMs);
    }
}