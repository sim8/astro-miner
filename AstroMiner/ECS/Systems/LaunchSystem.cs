using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Model;
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


    private Vector2 _minerStartPosition;
    private double _startedAt = -1;

    private LaunchModel Launch => game.Model?.Launch;

    private bool HasJustPassedSeconds(GameTime gameTime, double seconds)
    {
        var secondsPastThreshold = gameTime.TotalGameTime.TotalSeconds - _startedAt;
        return secondsPastThreshold >= seconds &&
               secondsPastThreshold - gameTime.ElapsedGameTime.TotalSeconds < seconds;
    }

    public void Reset()
    {
        Launch.IsLaunching = false;
        _startedAt = -1;
        Launch.MinerLaunchSpeed = 0f;
        ClearLightEntities();

        if (Launch.LaunchPadFrontEntityId == -1)
            Launch.LaunchPadFrontEntityId =
                game.StateManager.Ecs.Factories.CreateLaunchPadFrontEntity(_launchPadFrontStartPos);
        else
            game.StateManager.Ecs.GetComponent<PositionComponent>(Launch.LaunchPadFrontEntityId).Position =
                _launchPadFrontStartPos;
        if (Launch.LaunchPadRearEntityId == -1)
            Launch.LaunchPadRearEntityId =
                game.StateManager.Ecs.Factories.CreateLaunchPadRearEntity(_launchPadRearStartPos);
        else
            game.StateManager.Ecs.GetComponent<PositionComponent>(Launch.LaunchPadRearEntityId).Position =
                _launchPadRearStartPos;
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        // TODO ideally these'd be in an init()
        if (Launch.LaunchPadFrontEntityId == -1)
            Launch.LaunchPadFrontEntityId =
                game.StateManager.Ecs.Factories.CreateLaunchPadFrontEntity(_launchPadFrontStartPos);
        if (Launch.LaunchPadRearEntityId == -1)
            Launch.LaunchPadRearEntityId =
                game.StateManager.Ecs.Factories.CreateLaunchPadRearEntity(_launchPadRearStartPos);


        if (game.StateManager.AsteroidWorld.IsInMiner)
        {
            if (_startedAt == -1)
            {
                _startedAt = gameTime.TotalGameTime.TotalSeconds;
                _minerStartPosition = game.StateManager.Ecs
                    .GetComponent<PositionComponent>(game.StateManager.Ecs.MinerEntityId.Value)
                    .Position;
            }

            if (HasJustPassedSeconds(gameTime, 1))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(Coordinates.Px.LaunchLight1X,
                    Coordinates.Px.LaunchLightsY);
                var id = game.StateManager.Ecs.Factories.CreateLaunchLightEntity(pos);
                _launchLightEntities.Add(id);
            }

            if (HasJustPassedSeconds(gameTime, 2))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(Coordinates.Px.LaunchLight2X,
                    Coordinates.Px.LaunchLightsY);
                var id = game.StateManager.Ecs.Factories.CreateLaunchLightEntity(pos);
                _launchLightEntities.Add(id);
            }

            if (HasJustPassedSeconds(gameTime, 3))
            {
                var pos = ViewHelpers.AbsoluteXyPxToGridPos(Coordinates.Px.LaunchLight3X,
                    Coordinates.Px.LaunchLightsY);
                var id = game.StateManager.Ecs.Factories.CreateLaunchLightEntity(pos);
                _launchLightEntities.Add(id);
            }

            if (HasJustPassedSeconds(gameTime, 4))
            {
                Launch.IsLaunching = true;
                ClearLightEntities();
            }

            if (Launch.IsLaunching)
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
        if (game.Model.ActiveWorld == World.Asteroid) return true;
        var minerEntityId = game.StateManager.Ecs.MinerEntityId;
        if (minerEntityId == null) return false;

        var minerPosition = game.StateManager.Ecs.GetComponent<PositionComponent>(minerEntityId.Value);
        return _minerStartPosition.Y - minerPosition.Position.Y > LauncherGridHeight;
    }

    private void UpdateMinerLaunchSpeed(GameTime gameTime)
    {
        if (HasLeftLauncher()) return;

        // Calculate how far the miner has traveled within the launcher
        var minerEntityId = game.StateManager.Ecs.MinerEntityId;
        if (minerEntityId == null) return;

        var minerPosition = game.StateManager.Ecs.GetComponent<PositionComponent>(minerEntityId.Value);
        var distanceTraveled = _minerStartPosition.Y - minerPosition.Position.Y;
        var distanceRemaining = LauncherGridHeight - distanceTraveled;

        if (distanceRemaining <= 0)
        {
            // We're at the edge of the launcher, set to max speed
            Launch.MinerLaunchSpeed = GameConfig.Launch.AsteroidSpeed;
            return;
        }

        // Calculate time remaining based on current speed and acceleration
        // Using the formula: v² = u² + 2as
        // Where v is final velocity (MaxLaunchSpeed), u is current velocity (_launch.MinerLaunchSpeed)
        // a is acceleration, and s is distance remaining

        // Rearranging to find acceleration: a = (v² - u²) / (2s)
        var requiredAcceleration =
            (GameConfig.Launch.AsteroidSpeed * GameConfig.Launch.AsteroidSpeed -
             Launch.MinerLaunchSpeed * Launch.MinerLaunchSpeed) / (2 * distanceRemaining);

        // Apply the calculated acceleration for this frame
        var deltaTime = gameTime.ElapsedGameTime.Milliseconds / 1000f;
        Launch.MinerLaunchSpeed += requiredAcceleration * deltaTime;

        // Cap the speed at MaxLaunchSpeed just to be safe
        Launch.MinerLaunchSpeed = Math.Min(Launch.MinerLaunchSpeed, GameConfig.Launch.AsteroidSpeed);
    }

    private void UpdateMinerAndLaunchPadPosition(GameTime gameTime)
    {
        var minerEntityId = game.StateManager.Ecs.MinerEntityId;
        if (minerEntityId == null) return;

        var distance = Launch.MinerLaunchSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
        var movement = DirectionHelpers.GetDirectionalVector(distance, Direction.Top);
        var minerPosition = game.StateManager.Ecs.GetComponent<PositionComponent>(minerEntityId.Value);
        minerPosition.Position += movement;

        if (minerPosition.Position.Y < GameConfig.Launch.HomeToAsteroidPointY)
        {
            Launch.MinerLaunchSpeed = 0f; /// TODO set relative to new speed
            game.StateManager.SetActiveWorldAndInitialize(World.Asteroid);
        }

        var launchPadFrontPosition =
            game.StateManager.Ecs.GetComponent<PositionComponent>(Launch.LaunchPadFrontEntityId);
        var launchPadRearPosition =
            game.StateManager.Ecs.GetComponent<PositionComponent>(Launch.LaunchPadRearEntityId);

        launchPadFrontPosition.Position = Vector2.Max(launchPadFrontPosition.Position + movement,
            _launchPadFrontStartPos - new Vector2(0, LauncherGridHeight));
        launchPadRearPosition.Position = Vector2.Max(launchPadRearPosition.Position + movement,
            _launchPadRearStartPos - new Vector2(0, LauncherGridHeight));
    }

    private void ClearLightEntities()
    {
        foreach (var entityId in _launchLightEntities) game.StateManager.Ecs.DestroyEntity(entityId);
        _launchLightEntities.Clear();
    }
}