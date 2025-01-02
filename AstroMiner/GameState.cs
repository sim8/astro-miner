using System;
using System.Collections.Generic;
using System.Linq;

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
    private HashSet<MiningControls> _emptyMiningControls;
    private bool _prevPressedEnterOrExit;
    public List<Entity> ActiveEntitiesSortedByDistance;
    public CameraState Camera;
    public List<(int x, int y)> EdgeCells;
    public GridState Grid;
    public Inventory Inventory;
    public MinerEntity Miner;
    public PlayerEntity Player;

    public GameState()
    {
        Initialize();
    }

    public int Seed { get; private set; }

    public int TimeUntilAsteroidExplodesMs { get; private set; }

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
        Grid.MarkAllDistancesFromOutsideConnectedFloors();
        Miner = new MinerEntity(this);
        Miner.Initialize(minerPos);
        Player = new PlayerEntity(this);
        Player.Initialize(minerPos);
        Inventory = new Inventory();
        EdgeCells = UserInterfaceHelpers.GetAsteroidEdgeCells(Grid);
        Camera = new CameraState(this);
        ActiveEntitiesSortedByDistance = [Miner];
        _prevPressedEnterOrExit = false;
        _emptyMiningControls = new HashSet<MiningControls>();
        TimeUntilAsteroidExplodesMs = 5 * 60 * 1000;
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

        TimeUntilAsteroidExplodesMs = Math.Max(TimeUntilAsteroidExplodesMs - elapsedMs, 0);
        if (TimeUntilAsteroidExplodesMs == 0)
        {
            ActiveControllableEntity.IsDead = true;
            return;
        }

        if (activeMiningControls.Contains(MiningControls.EnterOrExit))
        {
            // Not continuous
            if (!_prevPressedEnterOrExit)
            {
                ActiveControllableEntity.Disembark();
                if (ActiveControllableEntity == Player && Player.GetDistanceTo(Miner) < GameConfig.MinEmbarkingDistance)
                {
                    DeactivateEntity(Player);
                }
                else if (ActiveControllableEntity == Miner)
                {
                    Player.Position = Miner.Position;
                    ActivateEntity(Player);
                }

                _prevPressedEnterOrExit = true;
            }
        }
        else
        {
            _prevPressedEnterOrExit = false;
        }

        foreach (var entity in ActiveEntitiesSortedByDistance.ToList())
            if (entity is MiningControllableEntity && entity == ActiveControllableEntity)
                entity.Update(elapsedMs, activeMiningControls);
            else
                entity.Update(elapsedMs, _emptyMiningControls);

        foreach (var cell in Grid._activeExplosiveRockCells) cell.Value.Update(elapsedMs);


        // Do last to reflect changes
        SortActiveEntities(); // TODO only call when needed? Seems error prone

        Camera.Update(elapsedMs);
    }
}