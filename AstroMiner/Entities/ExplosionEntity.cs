using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class ExplosionEntity : Entity
{
    private const int _animationTime = 400;
    private readonly float _explosionRadius = 2.2f;
    private readonly GameState _gameState;
    private bool _hasExploded;
    private int TimeSinceExplosionMs;

    public ExplosionEntity(GameState gameState, Vector2 pos)
    {
        Position = pos;
        _gameState = gameState;
    }

    public float AnimationPercentage => TimeSinceExplosionMs / (float)_animationTime;

    protected override int BoxSizePx { get; } = 1;
    public override bool CanCollide { get; } = false;

    private void ExplodeGrid()
    {
        var gridPos = ViewHelpers.ToGridPosition(Position);
        var explodedCells = GetCellsInRadius(Position.X, Position.Y, _explosionRadius);
        foreach (var (x, y) in explodedCells)
            // If x,y = gridPos, already triggered explosion
            if (_gameState.Grid.GetCellType(x, y) == CellType.ExplosiveRock && (x, y) != gridPos)
                _gameState.Grid.ActivateExplosiveRockCell(x, y, 300);
            else
                _gameState.Grid.ClearCell(x, y);

        _hasExploded = true;
    }

    private void calculateEntityDamage(MiningControllableEntity entity)
    {
        var distance = GetDistanceTo(entity);

        if (distance < _explosionRadius)
        {
            var damagePercentage = 1f - distance / _explosionRadius;
            var damage = (int)(GameConfig.ExplosionMaxDamage * damagePercentage);
            entity.TakeDamage(damage);
        }
    }

    public override void Update(int elapsedMs, HashSet<MiningControls> activeMiningControls)
    {
        if (!_hasExploded)
        {
            ExplodeGrid();
            calculateEntityDamage(_gameState.Miner);
            if (!_gameState.IsInMiner) calculateEntityDamage(_gameState.Player);
            _hasExploded = true;
        }
        else
        {
            TimeSinceExplosionMs += elapsedMs;
        }

        if (TimeSinceExplosionMs >= _animationTime) _gameState.DeactivateEntity(this);
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