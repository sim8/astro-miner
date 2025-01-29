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
    private readonly GameState _gameState;

    private readonly Random _rng = new();

    public readonly List<Cloud> CloudsBg = new();
    public readonly List<Cloud> CloudsFg = new();

    private float _nextBgSpawnTime;
    private float _nextFgSpawnTime;

    private float _timeSinceLastCloudBg;
    private float _timeSinceLastCloudFg;

    public CloudManager(GameState gameState)
    {
        _gameState = gameState;
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
            c.Y += BgCloudSpeed * elapsedSec;

            if (c.Y > _gameState.Graphics.GraphicsDevice.Viewport.Height) CloudsBg.RemoveAt(i);
        }

        for (var i = CloudsFg.Count - 1; i >= 0; i--)
        {
            var c = CloudsFg[i];
            c.Y += FgCloudSpeed * elapsedSec;

            if (c.Y > _gameState.Graphics.GraphicsDevice.Viewport.Height) CloudsFg.RemoveAt(i);
        }
    }

    private void SpawnCloud(bool isForeground)
    {
        var size = isForeground ? ForegroundCloudSizePx : BackgroundCloudSizePx;
        var xPos = (float)(_rng.NextDouble() * (_gameState.Graphics.GraphicsDevice.Viewport.Width + size)) - size;

        var yPos = -size;

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