using System.Collections.Generic;

namespace AstroMiner;

public enum MiningControls
{
    // Movement
    MoveUp,
    MoveRight,
    MoveDown,
    MoveLeft,

    Drill,
    EnterOrExit
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
    private readonly HashSet<MiningControls> _emptyMiningControls;
    public readonly GridState Grid;
    public readonly MinerEntity Miner;
    public readonly PlayerEntity Player;
    private bool _prevPressedEnterOrExit;

    public GameState()
    {
        var (grid, minerPos) = AsteroidGen.InitializeGridAndStartingPos(GameConfig.GridSize);
        Grid = new GridState(grid);
        Miner = new MinerEntity(this, minerPos);
        Player = new PlayerEntity(this, minerPos);
        IsInMiner = true;
        _prevPressedEnterOrExit = false;
        _emptyMiningControls = new HashSet<MiningControls>();
        TimeUntilAsteroidExplodesMs = 5 * 60 * 1000;
    }

    public int TimeUntilAsteroidExplodesMs { get; private set; }

    public bool IsInMiner { get; private set; }

    public MiningControllableEntity GetActiveControllableEntity()
    {
        return IsInMiner ? Miner : Player;
    }

    public void Update(HashSet<MiningControls> activeMiningControls, int elapsedMs)
    {
        TimeUntilAsteroidExplodesMs -= elapsedMs;

        if (activeMiningControls.Contains(MiningControls.EnterOrExit))
        {
            // Not continuous
            if (!_prevPressedEnterOrExit)
            {
                var activeControllableEntity = GetActiveControllableEntity();
                activeControllableEntity.Disembark();
                if (activeControllableEntity == Player && Player.GetDistanceTo(Miner) < GameConfig.MinEmbarkingDistance)
                {
                    IsInMiner = true;
                }
                else if (activeControllableEntity == Miner)
                {
                    IsInMiner = false;
                    Player.Position = Miner.Position;
                }
            }

            _prevPressedEnterOrExit = true;
        }
        else
        {
            _prevPressedEnterOrExit = false;
        }


        Miner.Update(IsInMiner ? activeMiningControls : _emptyMiningControls, elapsedMs);
        Player.Update(IsInMiner ? _emptyMiningControls : activeMiningControls, elapsedMs);
    }
}