using System.Collections.Generic;
using AstroMiner.Renderers.Asteroid;
using AstroMiner.Renderers.World;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class Renderer
{
    private readonly FrameCounter _frameCounter;
    private readonly GameState _gameState;
    private readonly RendererShared _shared;
    public readonly AsteroidRenderer AsteroidRenderer;
    public readonly WorldRenderer WorldRenderer;

    public Renderer(
        GraphicsDeviceManager graphics,
        Dictionary<string, Texture2D> textures,
        GameState gameState,
        FrameCounter frameCounter)
    {
        _shared = new RendererShared(gameState, graphics, textures);
        _gameState = gameState;
        _frameCounter = frameCounter;
        AsteroidRenderer = new AsteroidRenderer(_shared);
        WorldRenderer = new WorldRenderer(_shared);
    }


    public void Render(SpriteBatch spriteBatch)
    {
        if (_gameState.IsOnAsteroid) AsteroidRenderer.Render(spriteBatch);
        else
            WorldRenderer.Render(spriteBatch);

        RenderDebug(spriteBatch);
    }

    // TODO move?
    private void RenderDebug(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

        _shared.RenderString(spriteBatch, 1000, 0, "FPS " + _frameCounter.AverageFramesPerSecond.ToString("F0"));

        _shared.RenderString(spriteBatch, 1000, 40, "SEED " + _shared.GameState.Asteroid.Seed);

        spriteBatch.End();
    }
}