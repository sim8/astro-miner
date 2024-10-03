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
    PlaceDynamite
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
    public GridState Grid;
    public MinerEntity Miner;
    public PlayerEntity Player;

    public GameState()
    {
        Initialize();
    }

    public int TimeUntilAsteroidExplodesMs { get; private set; }

    public bool IsInMiner => !ActiveEntitiesSortedByDistance.Contains(Player);


    public MiningControllableEntity ActiveControllableEntity => IsInMiner ? Miner : Player;

    public void Initialize()
    {
        var (grid, minerPos) = AsteroidGen.InitializeGridAndStartingPos(GameConfig.GridSize);
        Grid = new GridState(grid);
        Miner = new MinerEntity(this, minerPos);
        Player = new PlayerEntity(this, minerPos);
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
        TimeUntilAsteroidExplodesMs -= elapsedMs;

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


        // Do last to reflect changes
        SortActiveEntities(); // TODO only call when needed? Seems error prone
    }
}