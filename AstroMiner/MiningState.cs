using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public enum MiningControls
{
    // Movement
    MoveUp,
    MoveRight,
    MoveDown,
    MoveLeft,

    Drill
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
    public const int GridSize = 40;
    private const float DrillDistance = 0.2f;
    private const float MinerMovementSpeed = 1.5f;
    private readonly Dictionary<MiningControls, Direction> _directionsControlsMapping;
    private readonly Dictionary<CellState, int> _drillTimesMs;
    private readonly CellState[,] _grid;
    private int _acceleratingForMs;
    private int _drillingMs;
    private (int x, int y)? _drillingPos;

    public MiningState()
    {
        _directionsControlsMapping = new Dictionary<MiningControls, Direction>
        {
            { MiningControls.MoveUp, Direction.Top },
            { MiningControls.MoveRight, Direction.Right },
            { MiningControls.MoveDown, Direction.Bottom },
            { MiningControls.MoveLeft, Direction.Left }
        };
        _drillTimesMs = new Dictionary<CellState, int>
        {
            { CellState.Rock, 600 },
            { CellState.Ruby, 1800 },
            { CellState.Diamond, 4000 }
        };
        (_grid, MinerPos) = AsteroidGen.InitializeGridAndStartingPos(GridSize);
    }

    public (int, int)? DrillingPos => _drillingPos;

    public Vector2 MinerPos { get; private set; }

    public Direction MinerDirection { get; private set; } = Direction.Top;

    private bool ApplyVectorToMinerPosIfNoCollisions(Vector2 vector)
    {
        var newPositions = new[]
        {
            MinerPos + vector,
            MinerPos + vector + new Vector2(GameConfig.MinerSize, 0),
            MinerPos + vector + new Vector2(0, GameConfig.MinerSize),
            MinerPos + vector + new Vector2(GameConfig.MinerSize, GameConfig.MinerSize)
        };

        foreach (var newPos in newPositions)
        {
            var (x, y) = ToGridPosition(newPos);
            if (!IsValidGridPosition(x, y) || GetCellState(x, y) != CellState.Floor)
                return false;
        }

        MinerPos += vector;
        return true;
    }

    public CellState GetCellState(int x, int y)
    {
        if (!IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        return _grid[y, x];
    }

    private void SetCellState(int x, int y, CellState newState)
    {
        if (!IsValidGridPosition(x, y))
            throw new IndexOutOfRangeException();
        _grid[y, x] = newState;
    }

    public bool IsValidGridPosition(int x, int y)
    {
        return x >= 0 && x < GridSize && y >= 0 && y < GridSize;
    }

    private static (int x, int y) ToGridPosition(Vector2 vector)
    {
        return ((int)Math.Floor(vector.X), (int)Math.Floor(vector.Y));
    }

    private Direction? GetDirectionFromActiveControls(HashSet<MiningControls> activeMiningControls)
    {
        foreach (var control in activeMiningControls)
            if (_directionsControlsMapping.TryGetValue(control, out var direction))
                return direction;
        return null;
    }

    public void Update(HashSet<MiningControls> activeMiningControls, int elapsedMs)
    {
        var direction = GetDirectionFromActiveControls(activeMiningControls);
        if (direction.HasValue)
            MoveMiner(direction.GetValueOrDefault(), elapsedMs);
        else if (_acceleratingForMs > 0)
            _acceleratingForMs = 0;

        if (activeMiningControls.Contains(MiningControls.Drill))
            UseDrill(elapsedMs);
        else
            ResetDrill();
    }

    private float GetMinerCurrentSpeed()
    {
        return _acceleratingForMs < 500 ? MinerMovementSpeed * _acceleratingForMs / 500 : MinerMovementSpeed;
    }

    public void MoveMiner(Direction direction, int elapsedGameTimeMs)
    {
        var prevDirection = MinerDirection;
        MinerDirection = direction;
        var distance = GetMinerCurrentSpeed() * (elapsedGameTimeMs / 1000f);

        var movement = direction switch
        {
            Direction.Top => new Vector2(0, -distance),
            Direction.Right => new Vector2(distance, 0),
            Direction.Bottom => new Vector2(0, distance),
            Direction.Left => new Vector2(-distance, 0),
            _ => Vector2.Zero
        };

        var noCollisions = ApplyVectorToMinerPosIfNoCollisions(movement);
        if (noCollisions) // TODO and direction is adjacent to current
            _acceleratingForMs += elapsedGameTimeMs;
        else
            _acceleratingForMs = 0;
    }

    private void UseDrill(int elapsedGameTimeMs)
    {
        var drillPos = GetDrillPosition();
        var gridPos = ToGridPosition(drillPos);

        if (gridPos == _drillingPos)
        {
            _drillingMs += elapsedGameTimeMs;
        }
        else
        {
            _drillingPos = gridPos;
            _drillingMs = elapsedGameTimeMs;
        }

        var (x, y) = gridPos;

        if (!IsValidGridPosition(x, y))
            return;

        var cellState = GetCellState(x, y);

        if (_drillTimesMs.TryGetValue(cellState, out var requiredTime) && _drillingMs > requiredTime)
            SetCellState(x, y, CellState.Floor);
    }

    private Vector2 GetDrillPosition()
    {
        return MinerDirection switch
        {
            Direction.Top => MinerPos + new Vector2(GameConfig.MinerSize / 2, -DrillDistance),
            Direction.Right => MinerPos + new Vector2(GameConfig.MinerSize + DrillDistance, GameConfig.MinerSize / 2),
            Direction.Bottom => MinerPos + new Vector2(GameConfig.MinerSize / 2, GameConfig.MinerSize + DrillDistance),
            Direction.Left => MinerPos + new Vector2(-DrillDistance, GameConfig.MinerSize / 2),
            _ => MinerPos
        };
    }

    private void ResetDrill()
    {
        _drillingPos = null;
        _drillingMs = 0;
    }
}