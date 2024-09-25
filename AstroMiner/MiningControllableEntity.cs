using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class MiningControllableEntity : Entity
{
    private const float DrillDistance = 0.2f;

    private readonly Dictionary<MiningControls, Direction> _directionsControlsMapping = new()
    {
        { MiningControls.MoveUp, Direction.Top },
        { MiningControls.MoveRight, Direction.Right },
        { MiningControls.MoveDown, Direction.Bottom },
        { MiningControls.MoveLeft, Direction.Left }
    };

    private readonly Dictionary<CellState, int> _drillTimesMs;

    private readonly GridState _gridState;
    private float _currentSpeed;

    private int _drillingMs;

    // TODO is this needed?
    private (int x, int y)? _drillingPos;

    public MiningControllableEntity(GridState gridState, Vector2 pos)
    {
        _gridState = gridState;
        Position = pos;
        _drillTimesMs = new Dictionary<CellState, int>
        {
            { CellState.Rock, 600 },
            { CellState.Ruby, 1800 },
            { CellState.Diamond, 4000 }
        };
    }

    protected virtual float MaxSpeed => 1f;
    protected virtual int TimeToReachMaxSpeedMs { get; } = 0;
    protected virtual int TimeToStopMs { get; } = 0;


    public Direction Direction { get; private set; } = Direction.Top;

    private Direction? GetDirectionFromActiveControls(HashSet<MiningControls> activeMiningControls)
    {
        foreach (var control in activeMiningControls)
            if (_directionsControlsMapping.TryGetValue(control, out var direction))
                return direction;
        return null;
    }

    public void Disembark()
    {
        _drillingMs = 0;
    }

    private bool ApplyVectorToPosIfNoCollisions(Vector2 vector)
    {
        var newPositions = new[]
        {
            Position + vector,
            Position + vector + new Vector2(GridBoxSize, 0),
            Position + vector + new Vector2(0, GridBoxSize),
            Position + vector + new Vector2(GridBoxSize, GridBoxSize)
        };

        foreach (var newPos in newPositions)
        {
            var (x, y) = ViewHelpers.ToGridPosition(newPos);
            if (!ViewHelpers.IsValidGridPosition(x, y) || _gridState.GetCellState(x, y) != CellState.Floor)
                return false;
        }

        Position += vector;
        return true;
    }

    public void Update(HashSet<MiningControls> activeMiningControls, int elapsedMs)
    {
        var direction = GetDirectionFromActiveControls(activeMiningControls);
        UpdateMinerPosAndSpeed(direction, elapsedMs);

        if (activeMiningControls.Contains(MiningControls.Drill))
            UseDrill(elapsedMs);
        else
            ResetDrill();
    }

    private void UpdateMinerPosAndSpeed(Direction? selectedDirection, int elapsedGameTimeMs)
    {
        Direction = selectedDirection ?? Direction;

        // Decelerate if nothing pressed
        if (!selectedDirection.HasValue && _currentSpeed > 0)
            _currentSpeed = Math.Max(0,
                _currentSpeed - MaxSpeed * (elapsedGameTimeMs / (float)TimeToStopMs));

        if (_currentSpeed > 0 || selectedDirection.HasValue)
        {
            var hasCollisions = UpdateMinerPos(elapsedGameTimeMs);
            if (hasCollisions)
                _currentSpeed = 0;
            // Accelerate if direction pressed
            else if (selectedDirection.HasValue)
                _currentSpeed = Math.Min(MaxSpeed,
                    _currentSpeed + MaxSpeed * (elapsedGameTimeMs / (float)TimeToReachMaxSpeedMs));
        }
    }

    private bool UpdateMinerPos(int elapsedGameTimeMs)
    {
        var distance = _currentSpeed * (elapsedGameTimeMs / 1000f);

        var movement = Direction switch
        {
            Direction.Top => new Vector2(0, -distance),
            Direction.Right => new Vector2(distance, 0),
            Direction.Bottom => new Vector2(0, distance),
            Direction.Left => new Vector2(-distance, 0),
            _ => Vector2.Zero
        };

        return !ApplyVectorToPosIfNoCollisions(movement);
    }

    private void UseDrill(int elapsedGameTimeMs)
    {
        var drillPos = GetDrillPosition();
        var gridPos = ViewHelpers.ToGridPosition(drillPos);

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

        if (!ViewHelpers.IsValidGridPosition(x, y))
            return;

        var cellState = _gridState.GetCellState(x, y);

        if (_drillTimesMs.TryGetValue(cellState, out var requiredTime) && _drillingMs > requiredTime)
            _gridState.SetCellState(x, y, CellState.Floor);
    }

    private Vector2 GetDrillPosition()
    {
        return Direction switch
        {
            Direction.Top => Position + new Vector2(GridBoxSize / 2, -DrillDistance),
            Direction.Right => Position + new Vector2(GridBoxSize + DrillDistance, GridBoxSize / 2),
            Direction.Bottom => Position + new Vector2(GridBoxSize / 2, GridBoxSize + DrillDistance),
            Direction.Left => Position + new Vector2(-DrillDistance, GridBoxSize / 2),
            _ => Position
        };
    }

    public Vector2 GetMinerLightSource()
    {
        return Direction switch
        {
            Direction.Top => Position + new Vector2(0.52f, 0.15f),
            Direction.Right => Position + new Vector2(0.35f, 0.33f),
            Direction.Bottom => Position + new Vector2(0.08f, 0.3f),
            Direction.Left => Position + new Vector2(0.2f, -0.1f),
            _ => Position
        };
    }

    private void ResetDrill()
    {
        _drillingPos = null;
        _drillingMs = 0;
    }
}