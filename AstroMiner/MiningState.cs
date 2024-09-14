using System;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public enum Direction
{
    Top,
    Right,
    Bottom,
    Left
}

public class MiningState(int minerTextureSizePx, int cellTextureSizePx)
{
    public const int GridSize = 40;
    private const float DrillDistance = 0.2f;
    private const float MinerMovementSpeed = 9f;
    private readonly CellState[,] _grid = AsteroidGen.InitializeGridAndStartingPos(GridSize);
    private readonly float _minerGridSize = (float)minerTextureSizePx / cellTextureSizePx;

    public Vector2 MinerPos { get; private set; } = new(0, 0);

    public Direction MinerDirection { get; private set; } = Direction.Right;

    private bool ApplyVectorToMinerPosIfNoCollisions(Vector2 vector)
    {
        var newTopLeft = MinerPos + vector;
        var newTopRight = MinerPos + vector + new Vector2(_minerGridSize, 0);
        var newBottomLeft = MinerPos + vector + new Vector2(0, _minerGridSize);
        var newBottomRight = MinerPos + vector + new Vector2(_minerGridSize, _minerGridSize);

        foreach (var newPosCorner in new[] { newTopLeft, newTopRight, newBottomRight, newBottomLeft })
            try
            {
                if (GetCellState(newPosCorner) != CellState.Empty) return false;
            }
            catch (Exception)
            {
                return false; // out of bounds
            }

        MinerPos = newTopLeft;
        return true;
    }

    public CellState GetCellState(int row, int column)
    {
        if (row < 0 || row >= GridSize || column < 0 || column >= GridSize) throw new IndexOutOfRangeException();
        return _grid[row, column];
    }

    public CellState GetCellState(Vector2 vector)
    {
        var gridX = (int)Math.Floor(vector.X);
        var gridY = (int)Math.Floor(vector.Y);
        return GetCellState(gridX, gridY);
    }

    public void SetCellState(Vector2 vector, CellState newState)
    {
        var gridX = (int)Math.Floor(vector.X);
        var gridY = (int)Math.Floor(vector.Y);
        _grid[gridX, gridY] = newState;
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

    public void UseDrill(int ellapsedGameTimeMs)
    {
        Vector2 drillPos;
        if (MinerDirection == Direction.Top) drillPos = MinerPos + new Vector2(_minerGridSize / 2, -DrillDistance);
        else if (MinerDirection == Direction.Right)
            drillPos = MinerPos + new Vector2(_minerGridSize + DrillDistance, _minerGridSize / 2);
        else if (MinerDirection == Direction.Bottom)
            drillPos = MinerPos + new Vector2(_minerGridSize / 2, _minerGridSize + DrillDistance);
        else drillPos = MinerPos + new Vector2(-DrillDistance, _minerGridSize / 2);

        if (GetCellState(drillPos) == CellState.Rock) SetCellState(drillPos, CellState.Empty);
    }
}