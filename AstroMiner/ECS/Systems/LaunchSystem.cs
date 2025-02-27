using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class LaunchSystem(Ecs ecs, GameState gameState) : System(ecs, gameState)
{
    private const int LauncherHeightPx = 120;
    private const float LaunchAcceleration = 400f;
    private const float LauncherGridHeight = LauncherHeightPx / (float)GameConfig.CellTextureSizePx;
    private readonly List<int> _launchLightEntities = new();
    private int LaunchPadFrontEntityId = -1;
    private int LaunchPadRearEntityId = -1;

    private Vector2 LaunchPadFrontStartPos = ViewHelpers.AbsoluteXyPxToGridPos(26, 126);
    private Vector2 LaunchPadRearStartPos = ViewHelpers.AbsoluteXyPxToGridPos(26, 95);
    private bool _isLaunching;
    private float _minerLaunchSpeed;

    private Vector2 _minerStartPosition;
    private double _startedAt = -1;

    private bool HasJustPassedSeconds(GameTime gameTime, double seconds)
    {
        var secondsPastThreshold = gameTime.TotalGameTime.TotalSeconds - _startedAt;
        return secondsPastThreshold >= seconds &&
               secondsPastThreshold - gameTime.ElapsedGameTime.TotalSeconds < seconds;
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        // TODO ideally these'd be in an init()
        if (LaunchPadFrontEntityId == -1)
        {
            LaunchPadFrontEntityId = GameState.Ecs.Factories.CreateLaunchPadFrontEntity(LaunchPadFrontStartPos);
        }
        if (LaunchPadRearEntityId == -1)
        {
            LaunchPadRearEntityId = GameState.Ecs.Factories.CreateLaunchPadRearEntity(LaunchPadRearStartPos);
        }


        if (GameState.AsteroidWorld.IsInMiner)
        {
            if (_startedAt == -1)
            {
                _startedAt = gameTime.TotalGameTime.TotalSeconds;
                _minerStartPosition = gameState.Ecs.GetComponent<PositionComponent>(gameState.Ecs.MinerEntityId.Value)
                    .Position;
            }

            if (HasJustPassedSeconds(gameTime, 1))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(119, 97);
                var id = GameState.Ecs.Factories.CreateLaunchLightEntity(pos);
                _launchLightEntities.Add(id);
            }

            if (HasJustPassedSeconds(gameTime, 2))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(125, 97);
                var id = GameState.Ecs.Factories.CreateLaunchLightEntity(pos);
                _launchLightEntities.Add(id);
            }

            if (HasJustPassedSeconds(gameTime, 3))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(131, 97);
                var id = GameState.Ecs.Factories.CreateLaunchLightEntity(pos);
                _launchLightEntities.Add(id);
            }

            if (HasJustPassedSeconds(gameTime, 4))
            {
                _isLaunching = true;
                ClearLightEntities();
            }

            if (_isLaunching)
            {
                UpdateMinerLaunchSpeed(gameTime);
                UpdateMinerAndLaunchPadPosition(gameTime);
            }
        }
        else
        {
            _isLaunching = false;
            _startedAt = -1;
            _minerLaunchSpeed = 0f;
            ClearLightEntities();
        }
    }

    private bool HasLeftLauncher()
    {
        if (GameState.ActiveWorld == World.Asteroid)
        {
            return true;
        }
        var minerEntityId = gameState.Ecs.MinerEntityId;
        if (minerEntityId == null) return false;

        var minerPosition = gameState.Ecs.GetComponent<PositionComponent>(minerEntityId.Value);
        return _minerStartPosition.Y - minerPosition.Position.Y > LauncherGridHeight;
    }

    private void UpdateMinerLaunchSpeed(GameTime gameTime)
    {
        if (HasLeftLauncher()) return;

        _minerLaunchSpeed += LaunchAcceleration * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
    }

    private void UpdateMinerAndLaunchPadPosition(GameTime gameTime)
    {
        var minerEntityId = gameState.Ecs.MinerEntityId;
        if (minerEntityId == null) return;

        var distance = _minerLaunchSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
        Console.WriteLine(_minerLaunchSpeed);
        var movement = DirectionHelpers.GetDirectionalVector(distance, Direction.Top);
        var minerPosition = gameState.Ecs.GetComponent<PositionComponent>(minerEntityId.Value);
        minerPosition.Position += movement;

        if (minerPosition.Position.Y < GameConfig.HomeToAsteroidPointY)
        {
            _minerLaunchSpeed = 3f;
            GameState.InitializeAsteroid();
        }

        var launchPadFrontPosition = gameState.Ecs.GetComponent<PositionComponent>(LaunchPadFrontEntityId);
        var launchPadRearPosition = gameState.Ecs.GetComponent<PositionComponent>(LaunchPadRearEntityId);

        launchPadFrontPosition.Position = Vector2.Max(launchPadFrontPosition.Position + movement, LaunchPadFrontStartPos - new Vector2(0, LauncherGridHeight));
        launchPadRearPosition.Position = Vector2.Max(launchPadRearPosition.Position + movement, LaunchPadRearStartPos - new Vector2(0, LauncherGridHeight));

    }

    private void ClearLightEntities()
    {
        foreach (var entityId in _launchLightEntities) GameState.Ecs.DestroyEntity(entityId);
        _launchLightEntities.Clear();
    }
}