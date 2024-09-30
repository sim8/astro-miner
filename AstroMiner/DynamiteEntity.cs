using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class DynamiteEntity : Entity
{
    private const int FuseTimeMs = 4000;
    private readonly GameState _gameState;
    private bool _hasExploded;

    public DynamiteEntity(GameState gameState, Vector2 pos)
    {
        TimeToExplodeMs = FuseTimeMs;
        Position = pos;
        _gameState = gameState;
    }

    protected override int BoxSizePx { get; } = 4;
    public override bool CanCollide { get; } = false;

    public float FusePercentLeft => TimeToExplodeMs / (float)FuseTimeMs;

    public int TimeToExplodeMs { get; private set; }

    public override void Update(int elapsedMs, HashSet<MiningControls> activeMiningControls)
    {
        TimeToExplodeMs -= elapsedMs;
        if (TimeToExplodeMs <= 0 && !_hasExploded)
        {
            var explodedCells = GetCellsInRadius(Position.X, Position.Y, 1.3f);
            foreach (var (x, y) in explodedCells) _gameState.Grid.DemolishCell(x, y);
            _hasExploded = true;
        }
    }

    private List<(int x, int y)> GetCellsInRadius(float centerX, float centerY, float radius)
    {
        var cells = new List<(int x, int y)>();

        // Calculate the bounds to iterate over, based on the radius
        var startX = Math.Max(0, (int)Math.Floor(centerX - radius));
        var endX = Math.Min(_gameState.Grid.Columns - 1, (int)Math.Ceiling(centerX + radius));
        var startY = Math.Max(0, (int)Math.Floor(centerY - radius));
        var endY = Math.Min(_gameState.Grid.Rows - 1, (int)Math.Ceiling(centerY + radius));

        // Iterate through the grid cells within these bounds
        for (var i = startX; i <= endX; i++)
        for (var j = startY; j <= endY; j++)
        {
            // Calculate the distance from the center of the cell to the point
            var cellCenterX = i + 0.5f;
            var cellCenterY = j + 0.5f;
            var distance = (float)Math.Sqrt(Math.Pow(cellCenterX - centerX, 2) + Math.Pow(cellCenterY - centerY, 2));

            // If the distance is less than or equal to the radius, include the cell
            if (distance <= radius) cells.Add((i, j));
        }

        return cells;
    }
}