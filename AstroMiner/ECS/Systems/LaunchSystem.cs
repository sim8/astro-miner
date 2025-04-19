using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class LaunchSystem(Ecs ecs, BaseGame game) : System(ecs, game)
{
    private const int LauncherHeightPx = 120;

    private const float LauncherGridHeight = LauncherHeightPx / (float)GameConfig.CellTextureSizePx;
    private readonly List<int> _launchLightEntities = new();

    private readonly Vector2 _launchPadFrontStartPos =
        ViewHelpers.AbsoluteXyPxToGridPos(Coordinates.Px.LaunchPadsX, Coordinates.Px.LaunchPadFrontStartY);

    private readonly Vector2 _launchPadRearStartPos =
        ViewHelpers.AbsoluteXyPxToGridPos(Coordinates.Px.LaunchPadsX, Coordinates.Px.LaunchPadRearStartY);

    private bool _isLaunching;
    private int _launchPadFrontEntityId = -1;
    private int _launchPadRearEntityId = -1;
    private float _minerLaunchSpeed;


    private Vector2 _minerStartPosition;
    private double _startedAt = -1;

    private bool HasJustPassedSeconds(GameTime gameTime, double seconds)
    {
        var secondsPastThreshold = gameTime.TotalGameTime.TotalSeconds - _startedAt;
        return secondsPastThreshold >= seconds &&
               secondsPastThreshold - gameTime.ElapsedGameTime.TotalSeconds < seconds;
    }

    public void Reset()
    {
        _isLaunching = false;
        _startedAt = -1;
        _minerLaunchSpeed = 0f;
        ClearLightEntities();

        if (_launchPadFrontEntityId == -1)
            _launchPadFrontEntityId = game.State.Ecs.Factories.CreateLaunchPadFrontEntity(_launchPadFrontStartPos);
        else
            game.State.Ecs.GetComponent<PositionComponent>(_launchPadFrontEntityId).Position = _launchPadFrontStartPos;
        if (_launchPadRearEntityId == -1)
            _launchPadRearEntityId = game.State.Ecs.Factories.CreateLaunchPadRearEntity(_launchPadRearStartPos);
        else
            game.State.Ecs.GetComponent<PositionComponent>(_launchPadRearEntityId).Position = _launchPadRearStartPos;
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        // TODO ideally these'd be in an init()
        if (_launchPadFrontEntityId == -1)
            _launchPadFrontEntityId = game.State.Ecs.Factories.CreateLaunchPadFrontEntity(_launchPadFrontStartPos);
        if (_launchPadRearEntityId == -1)
            _launchPadRearEntityId = game.State.Ecs.Factories.CreateLaunchPadRearEntity(_launchPadRearStartPos);


        if (game.State.AsteroidWorld.IsInMiner)
        {
            if (_startedAt == -1)
            {
                _startedAt = gameTime.TotalGameTime.TotalSeconds;
                _minerStartPosition = game.State.Ecs.GetComponent<PositionComponent>(game.State.Ecs.MinerEntityId.Value)
                    .Position;
            }

            if (HasJustPassedSeconds(gameTime, 1))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(Coordinates.Px.LaunchLight1X,
                    Coordinates.Px.LaunchLightsY);
                var id = game.State.Ecs.Factories.CreateLaunchLightEntity(pos);
                _launchLightEntities.Add(id);
            }

            if (HasJustPassedSeconds(gameTime, 2))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(Coordinates.Px.LaunchLight2X,
                    Coordinates.Px.LaunchLightsY);
                var id = game.State.Ecs.Factories.CreateLaunchLightEntity(pos);
                _launchLightEntities.Add(id);
            }

            if (HasJustPassedSeconds(gameTime, 3))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(Coordinates.Px.LaunchLight3X,
                    Coordinates.Px.LaunchLightsY);
                var id = game.State.Ecs.Factories.CreateLaunchLightEntity(pos);
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
            Reset();
        }
    }

    private bool HasLeftLauncher()
    {
        if (game.State.ActiveWorld == World.Asteroid) return true;
        var minerEntityId = game.State.Ecs.MinerEntityId;
        if (minerEntityId == null) return false;

        var minerPosition = game.State.Ecs.GetComponent<PositionComponent>(minerEntityId.Value);
        return _minerStartPosition.Y - minerPosition.Position.Y > LauncherGridHeight;
    }

    private void UpdateMinerLaunchSpeed(GameTime gameTime)
    {
        if (HasLeftLauncher()) return;

        // Calculate how far the miner has traveled within the launcher
        var minerEntityId = game.State.Ecs.MinerEntityId;
        if (minerEntityId == null) return;

        var minerPosition = game.State.Ecs.GetComponent<PositionComponent>(minerEntityId.Value);
        var distanceTraveled = _minerStartPosition.Y - minerPosition.Position.Y;
        var distanceRemaining = LauncherGridHeight - distanceTraveled;

        if (distanceRemaining <= 0)
        {
            // We're at the edge of the launcher, set to max speed
            _minerLaunchSpeed = GameConfig.Launch.AsteroidSpeed;
            return;
        }

        // Calculate time remaining based on current speed and acceleration
        // Using the formula: v² = u² + 2as
        // Where v is final velocity (MaxLaunchSpeed), u is current velocity (_minerLaunchSpeed)
        // a is acceleration, and s is distance remaining

        // Rearranging to find acceleration: a = (v² - u²) / (2s)
        var requiredAcceleration =
            (GameConfig.Launch.AsteroidSpeed * GameConfig.Launch.AsteroidSpeed -
             _minerLaunchSpeed * _minerLaunchSpeed) / (2 * distanceRemaining);

        // Apply the calculated acceleration for this frame
        var deltaTime = gameTime.ElapsedGameTime.Milliseconds / 1000f;
        _minerLaunchSpeed += requiredAcceleration * deltaTime;

        // Cap the speed at MaxLaunchSpeed just to be safe
        _minerLaunchSpeed = Math.Min(_minerLaunchSpeed, GameConfig.Launch.AsteroidSpeed);
    }

    private void UpdateMinerAndLaunchPadPosition(GameTime gameTime)
    {
        var minerEntityId = game.State.Ecs.MinerEntityId;
        if (minerEntityId == null) return;

        var distance = _minerLaunchSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
        var movement = DirectionHelpers.GetDirectionalVector(distance, Direction.Top);
        var minerPosition = game.State.Ecs.GetComponent<PositionComponent>(minerEntityId.Value);
        minerPosition.Position += movement;

        if (minerPosition.Position.Y < GameConfig.Launch.HomeToAsteroidPointY)
        {
            _minerLaunchSpeed = 0f; /// TODO set relative to new speed
            game.State.SetActiveWorldAndInitialize(World.Asteroid);
        }

        var launchPadFrontPosition = game.State.Ecs.GetComponent<PositionComponent>(_launchPadFrontEntityId);
        var launchPadRearPosition = game.State.Ecs.GetComponent<PositionComponent>(_launchPadRearEntityId);

        launchPadFrontPosition.Position = Vector2.Max(launchPadFrontPosition.Position + movement,
            _launchPadFrontStartPos - new Vector2(0, LauncherGridHeight));
        launchPadRearPosition.Position = Vector2.Max(launchPadRearPosition.Position + movement,
            _launchPadRearStartPos - new Vector2(0, LauncherGridHeight));
    }

    private void ClearLightEntities()
    {
        foreach (var entityId in _launchLightEntities) game.State.Ecs.DestroyEntity(entityId);
        _launchLightEntities.Clear();
    }
}