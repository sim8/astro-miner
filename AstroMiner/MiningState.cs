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
    public readonly GridState GridState;
    public readonly MinerEntity Miner;
    public readonly PlayerEntity Player;

    public MiningState()
    {
        var (grid, minerPos) = AsteroidGen.InitializeGridAndStartingPos(GameConfig.GridSize);
        GridState = new GridState(grid);
        Miner = new MinerEntity(GridState, minerPos);
        Player = new PlayerEntity(GridState, minerPos);
        IsInMiner = true;
    }

    public bool IsInMiner { get; private set; }

    public MiningControllableEntity GetActiveControllableEntity()
    {
        return IsInMiner ? Miner : Player;
    }

    public void Update(HashSet<MiningControls> activeMiningControls, int elapsedMs)
    {
        // TODO only press once
        if (activeMiningControls.Contains(MiningControls.EnterOrExit))
            if (GetActiveControllableEntity() == Player)
                // TODO check distance
                IsInMiner = true;
            else
                IsInMiner = false;
        GetActiveControllableEntity().Update(activeMiningControls, elapsedMs);
    }
}