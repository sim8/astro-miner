using System;
using System.Collections.Generic;

namespace AstroMiner;

public class FogAnimationManager
{
    private const float FadeOutDurationMs = 600f;
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

    public void Update(float elapsedMs)
    {
        var completedCells = new List<(int x, int y)>();

        foreach (var (x, y) in _activeFadingCells)
        {
            var cell = _gameState.Grid.GetCellState(x, y);

            cell.FogOpacity = Math.Max(0, cell.FogOpacity - elapsedMs / FadeOutDurationMs);
            if (cell.FogOpacity <= 0) completedCells.Add((x, y));
        }

        foreach (var pos in completedCells) _activeFadingCells.Remove(pos);
    }
}