using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class CameraState
{
    private const int ZoomTransitionMs = 800;
    private readonly GameState _gameState;
    private float _endScale;
    private float _selectedZoom = 2f; // Default zoom level

    // Fields for handling transitions
    private float _startScale;
    private int _zoomTransitionElapsed;

    public CameraState(GameState gameState)
    {
        _gameState = gameState;
        ScaleMultiplier = GameConfig.ZoomLevelPlayer;
        // Initialize the transition state
        _startScale = ScaleMultiplier;
        _endScale = ScaleMultiplier;
        _zoomTransitionElapsed = 0;
    }

    public float ScaleMultiplier { get; private set; }

    public void Update(GameTime gameTime, HashSet<MiningControls> activeMiningControls)
    {
        if (activeMiningControls.Contains(MiningControls.CycleZoom))
            _selectedZoom = _selectedZoom switch
            {
                2f => 3f,
                3f => 2f
            };

        // Check if target scale changed (e.g., player switched from miner to player or vice versa)
        var currentTarget = _selectedZoom;
        if (Math.Abs(currentTarget - _endScale) > 0.0001f)
        {
            // Start a new transition towards the new target
            _startScale = ScaleMultiplier;
            _endScale = currentTarget;
            _zoomTransitionElapsed = 0;
        }

        var totalDifference = _endScale - _startScale;

        // Evaluate how close we currently are to the target
        var currentDifference = _endScale - ScaleMultiplier;

        // If the current difference is negligible, just set it and return
        if (Math.Abs(currentDifference) < 0.0001f)
        {
            ScaleMultiplier = _endScale;
            return;
        }

        // Accumulate elapsed time
        _zoomTransitionElapsed += gameTime.ElapsedGameTime.Milliseconds;

        // Calculate how far along we are from 0 to 1
        var fraction = (float)_zoomTransitionElapsed / ZoomTransitionMs;
        if (fraction > 1.0f)
            fraction = 1.0f;

        // Apply an ease-in-out function (cosine-based)
        var easedFraction = (float)((1.0 - Math.Cos(Math.PI * fraction)) / 2.0);

        // Interpolate
        ScaleMultiplier = _startScale + totalDifference * easedFraction;

        // Once we're done (fraction = 1.0), clamp to the exact target
        if (fraction >= 1.0f) ScaleMultiplier = _endScale;
    }
}