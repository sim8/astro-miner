using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner.AsteroidWorld;

public class FogAnimationManager
{
    private const float BaseFadeOutDurationMs = 70f;
    private readonly HashSet<(int x, int y)> _activeFadingCells = new();
    private readonly BaseGame _game;

    public FogAnimationManager(BaseGame game)
    {
        _game = game;
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
            var cell = _game.StateManager.AsteroidWorld.Grid.GetCellState(x, y);


            var distanceFromPlayer = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f),
                _game.StateManager.Ecs.ActiveControllableEntityCenterPosition);

            cell.FogOpacity = Math.Max(0,
                cell.FogOpacity - gameTime.ElapsedGameTime.Milliseconds / (BaseFadeOutDurationMs * distanceFromPlayer));
            if (cell.FogOpacity <= 0) completedCells.Add((x, y));
        }

        foreach (var pos in completedCells) _activeFadingCells.Remove(pos);
    }
}