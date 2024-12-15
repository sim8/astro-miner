using System;

namespace AstroMiner;

public class UserInterfaceState
{
    private const int ZoomTransitionMs = 800;
    private readonly GameState gameState;
    private float endScale;

    // Fields for handling transitions
    private float startScale;
    private int zoomTransitionElapsed;

    public UserInterfaceState(GameState gameState)
    {
        this.gameState = gameState;
        ScaleMultiplier = GameConfig.ZoomLevelPlayer;
        // Initialize the transition state
        startScale = ScaleMultiplier;
        endScale = ScaleMultiplier;
        zoomTransitionElapsed = 0;
    }

    private float BaseScaleMultiplier => gameState.IsInMiner ? GameConfig.ZoomLevelMiner : GameConfig.ZoomLevelPlayer;

    public float ScaleMultiplier { get; private set; }

    public void Update(int elapsedMs)
    {
        // Check if target scale changed (e.g., player switched from miner to player or vice versa)
        var currentTarget = BaseScaleMultiplier;
        if (Math.Abs(currentTarget - endScale) > 0.0001f)
        {
            // Start a new transition towards the new target
            startScale = ScaleMultiplier;
            endScale = currentTarget;
            zoomTransitionElapsed = 0;
        }

        var difference = endScale - startScale;

        // If the difference is negligible, just set it
        if (Math.Abs(difference) < 0.0001f)
        {
            ScaleMultiplier = endScale;
            return;
        }

        // Accumulate elapsed time
        zoomTransitionElapsed += elapsedMs;

        // Calculate how far along we are from 0 to 1
        var fraction = (float)zoomTransitionElapsed / ZoomTransitionMs;
        if (fraction > 1.0f)
            fraction = 1.0f;

        // Apply an ease-in-out function
        var easedFraction = (float)((1.0 - Math.Cos(Math.PI * fraction)) / 2.0);

        // Interpolate
        ScaleMultiplier = startScale + difference * easedFraction;

        // Once we're done (fraction = 1.0), clamp to the exact target
        if (fraction >= 1.0f) ScaleMultiplier = endScale;
    }
}