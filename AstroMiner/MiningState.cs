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

public class MiningState
{
    private readonly HashSet<MiningControls> _EmptyMiningControls;
    public readonly GridState GridState;
    public readonly MinerEntity Miner;
    public readonly PlayerEntity Player;
    private bool _prevPressedEnterOrExit;

    public MiningState()
    {
        var (grid, minerPos) = AsteroidGen.InitializeGridAndStartingPos(GameConfig.GridSize);
        GridState = new GridState(grid);
        Miner = new MinerEntity(GridState, minerPos);
        Player = new PlayerEntity(GridState, minerPos);
        IsInMiner = true;
        _prevPressedEnterOrExit = false;
        _EmptyMiningControls = new HashSet<MiningControls>();
    }

    public bool IsInMiner { get; private set; }

    public MiningControllableEntity GetActiveControllableEntity()
    {
        return IsInMiner ? Miner : Player;
    }

    public void Update(HashSet<MiningControls> activeMiningControls, int elapsedMs)
    {
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
                else
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


        Miner.Update(IsInMiner ? activeMiningControls : _EmptyMiningControls, elapsedMs);
        Player.Update(IsInMiner ? _EmptyMiningControls : activeMiningControls, elapsedMs);
    }
}