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
    private const float MinerMovementSpeed = 9f;
    private readonly Dictionary<MiningControls, Direction> _directionsControlsMapping;
    private readonly CellState[,] _grid;
    private readonly float _minerSize;
    private int _drillingMs;
    private (int x, int y)? _drillingPosition;

    public MiningState(float minerSize)
    {
        _directionsControlsMapping = new Dictionary<MiningControls, Direction>
        {
            { MiningControls.MoveUp, Direction.Top },
            { MiningControls.MoveRight, Direction.Right },
            { MiningControls.MoveDown, Direction.Bottom },
            { MiningControls.MoveLeft, Direction.Left }
        };
        _minerSize = minerSize;
        (_grid, MinerPos) = AsteroidGen.InitializeGridAndStartingPos(GridSize, _minerSize);
    }

    public Vector2 MinerPos { get; private set; }


    public Direction MinerDirection { get; private set; } = Direction.Top;

    private bool ApplyVectorToMinerPosIfNoCollisions(Vector2 vector)
    {
        var newTopLeft = MinerPos + vector;
        var newTopRight = MinerPos + vector + new Vector2(_minerSize, 0);
        var newBottomLeft = MinerPos + vector + new Vector2(0, _minerSize);
        var newBottomRight = MinerPos + vector + new Vector2(_minerSize, _minerSize);

        foreach (var newPosCorner in new[] { newTopLeft, newTopRight, newBottomRight, newBottomLeft })
            try
            {
                if (GetCellState(newPosCorner) != CellState.Floor) return false;
            }
            catch (Exception)
            {
                return false; // out of bounds
            }

        MinerPos = newTopLeft;
        return true;
    }

    public CellState GetCellState(int column, int row)
    {
        if (row < 0 || row >= GridSize || column < 0 || column >= GridSize) throw new IndexOutOfRangeException();
        return _grid[row, column];
    }

    private static (int x, int y) ToGridPosition(Vector2 vector)
    {
        return ((int)Math.Floor(vector.X), (int)Math.Floor(vector.Y));
    }

    private CellState GetCellState(Vector2 vector)
    {
        var (gridX, gridY) = ToGridPosition(vector);
        return GetCellState(gridX, gridY);
    }

    private void SetCellState(Vector2 vector, CellState newState)
    {
        var (gridX, gridY) = ToGridPosition(vector);
        _grid[gridY, gridX] = newState;
    }

    public void Update(HashSet<MiningControls> activeMiningControls, int elapsedMs)
    {
        if (activeMiningControls.Contains(MiningControls.MoveUp))
            MoveMiner(Direction.Top, elapsedMs);
        else if (activeMiningControls.Contains(MiningControls.MoveRight))
            MoveMiner(Direction.Right, elapsedMs);
        else if (activeMiningControls.Contains(MiningControls.MoveDown))
            MoveMiner(Direction.Bottom, elapsedMs);
        else if (activeMiningControls.Contains(MiningControls.MoveLeft))
            MoveMiner(Direction.Left, elapsedMs);

        if (activeMiningControls.Contains(MiningControls.Drill)) UseDrill(elapsedMs);
    }

    public void MoveMiner(Direction direction, int ellapsedGameTimeMs)
    {
        MinerDirection = direction;
        var distance = MinerMovementSpeed * (ellapsedGameTimeMs / 1000f);

        if (direction == Direction.Top) ApplyVectorToMinerPosIfNoCollisions(new Vector2(0, -distance));
        if (direction == Direction.Right) ApplyVectorToMinerPosIfNoCollisions(new Vector2(distance, 0));
        if (direction == Direction.Bottom) ApplyVectorToMinerPosIfNoCollisions(new Vector2(0, distance));
        if (direction == Direction.Left) ApplyVectorToMinerPosIfNoCollisions(new Vector2(-distance, 0));
    }

    private void UseDrill(int ellapsedGameTimeMs)
    {
        Vector2 drillPos;
        if (MinerDirection == Direction.Top) drillPos = MinerPos + new Vector2(_minerSize / 2, -DrillDistance);
        else if (MinerDirection == Direction.Right)
            drillPos = MinerPos + new Vector2(_minerSize + DrillDistance, _minerSize / 2);
        else if (MinerDirection == Direction.Bottom)
            drillPos = MinerPos + new Vector2(_minerSize / 2, _minerSize + DrillDistance);
        else drillPos = MinerPos + new Vector2(-DrillDistance, _minerSize / 2);

        var gridPos = ToGridPosition(drillPos);

        if (gridPos == _drillingPosition)
        {
            _drillingMs += ellapsedGameTimeMs;
        }
        else
        {
            // Drilling different cell, reset timer
            _drillingPosition = gridPos;
            _drillingMs = ellapsedGameTimeMs;
        }

        if (GetCellState(gridPos.Item1, gridPos.Item2) == CellState.Rock && _drillingMs > 1000)
            SetCellState(drillPos, CellState.Floor);
    }

    private void ResetDrill()
    {
        if (_drillingPosition.HasValue)
        {
            _drillingPosition = null;
            _drillingMs = 0;
        }
    }
}