using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class FogAnimationManager
{
    private const float BaseFadeOutDurationMs = 70f;
    private readonly HashSet<(int x, int y)> _activeFadingCells = new();
    private readonly GameState _gameState;

    public FogAnimationManager(GameState gameState)
    {
        _gameState = gameState;
    }

    public void AddFadingCell(int x, int y)
    {
        _activeFadingCells.Add((x, y));
    }

    public void Update(GameTime gameTime)
    {
        var completedCells = new List<(int x, int y)>();

        foreach (var (x, y) in _activeFadingCells)
        {
            var cell = _gameState.Asteroid.Grid.GetCellState(x, y);


            var distanceFromPlayer = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f),
                _gameState.Asteroid.ActiveControllableEntity.CenterPosition);

            cell.FogOpacity = Math.Max(0,
                cell.FogOpacity - gameTime.ElapsedGameTime.Milliseconds / (BaseFadeOutDurationMs * distanceFromPlayer));
            if (cell.FogOpacity <= 0) completedCells.Add((x, y));
        }

        foreach (var pos in completedCells) _activeFadingCells.Remove(pos);
    }
}