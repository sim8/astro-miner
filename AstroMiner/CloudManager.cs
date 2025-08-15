using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class CloudManager
{
    private const float MinBgSpawnInterval = 200f; // ms
    private const float MaxBgSpawnInterval = 400f; // ms
    private const float MinFgSpawnInterval = 200f; // ms
    private const float MaxFgSpawnInterval = 400f; // ms

    private const float BgCloudSpeed = 1000;
    private const float FgCloudSpeed = 4000;

    private const float BgCloudScale = 0.25f;

    // Render foreground at full resolution
    public const int ForegroundCloudSizePx = 1024;
    public const int BackgroundCloudSizePx = (int)(ForegroundCloudSizePx * BgCloudScale);
    private readonly BaseGame _game;

    private readonly Random _rng = new();

    public readonly List<Cloud> CloudsBg = new();
    public readonly List<Cloud> CloudsFg = new();

    private float _nextBgSpawnTime;
    private float _nextFgSpawnTime;

    private float _timeSinceLastCloudBg;
    private float _timeSinceLastCloudFg;

    public CloudManager(BaseGame game)
    {
        _game = game;
        // Initialize first random spawn times
        _nextBgSpawnTime = GetRandomInterval(MinBgSpawnInterval, MaxBgSpawnInterval);
        _nextFgSpawnTime = GetRandomInterval(MinFgSpawnInterval, MaxFgSpawnInterval);
    }

    public void Update(GameTime gameTime)
    {
        var elapsedSec = gameTime.ElapsedGameTime.Milliseconds / 1000f;

        // Generate new

        _timeSinceLastCloudBg += gameTime.ElapsedGameTime.Milliseconds;
        if (_timeSinceLastCloudBg >= _nextBgSpawnTime)
        {
            SpawnCloud(false);
            _timeSinceLastCloudBg = 0f;
            _nextBgSpawnTime = GetRandomInterval(MinBgSpawnInterval, MaxBgSpawnInterval);
        }

        _timeSinceLastCloudFg += gameTime.ElapsedGameTime.Milliseconds;
        if (_timeSinceLastCloudFg >= _nextFgSpawnTime)
        {
            SpawnCloud(true);
            _timeSinceLastCloudFg = 0f;
            _nextFgSpawnTime = GetRandomInterval(MinFgSpawnInterval, MaxFgSpawnInterval);
        }

        // Update existing

        for (var i = CloudsBg.Count - 1; i >= 0; i--)
        {
            var c = CloudsBg[i];
            c.X -= BgCloudSpeed * elapsedSec;

            if (c.X < -BackgroundCloudSizePx) CloudsBg.RemoveAt(i);
        }

        for (var i = CloudsFg.Count - 1; i >= 0; i--)
        {
            var c = CloudsFg[i];
            c.X -= FgCloudSpeed * elapsedSec;

            if (c.X < -ForegroundCloudSizePx) CloudsFg.RemoveAt(i);
        }
    }

    private void SpawnCloud(bool isForeground)
    {
        var size = isForeground ? ForegroundCloudSizePx : BackgroundCloudSizePx;
        var viewportWidth = _game.Graphics.GraphicsDevice.Viewport.Width;

        var yPos = (float)(_rng.NextDouble() * (_game.Graphics.GraphicsDevice.Viewport.Height + size)) - size;
        var xPos = viewportWidth;

        var list = isForeground ? CloudsFg : CloudsBg;
        list.Add(new Cloud(xPos, yPos));
    }

    private float GetRandomInterval(float min, float max)
    {
        return (float)(_rng.NextDouble() * (max - min) + min);
    }
}

public class Cloud(float x, float y)
{
    public float X = x;
    public float Y = y;
}