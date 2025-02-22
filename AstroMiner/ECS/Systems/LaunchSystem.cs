using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class LaunchSystem(Ecs ecs, GameState gameState) : System(ecs, gameState)
{
    private const int LauncherHeightPx = 120;
    private const float LauncherGridHeight = LauncherHeightPx / (float)GameConfig.CellTextureSizePx;

    private Vector2 _minerStartPosition;
    private readonly List<int> _launchLightEntities = new();
    private double _startedAt = -1;
    private float _minerLaunchSpeed = 0f;
    private bool _isLaunching = false;

    private bool HasJustPassedSeconds(GameTime gameTime, double seconds)
    {
        var secondsPastThreshold = gameTime.TotalGameTime.TotalSeconds - _startedAt;
        return secondsPastThreshold >= seconds &&
               secondsPastThreshold - gameTime.ElapsedGameTime.TotalSeconds < seconds;
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        if (GameState.AsteroidWorld.IsInMiner)
        {
            if (_startedAt == -1)
            {
                _startedAt = gameTime.TotalGameTime.TotalSeconds;
                _minerStartPosition = gameState.Ecs.GetComponent<PositionComponent>(gameState.Ecs.MinerEntityId.Value).Position;
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
                UpdateMinerPosition(gameTime);
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

    private void UpdateMinerLaunchSpeed(GameTime gameTime)
    {
        var minerEntityId = gameState.Ecs.MinerEntityId;
        if (minerEntityId == null) return;

        var minerPosition = gameState.Ecs.GetComponent<PositionComponent>(minerEntityId.Value);

        _minerLaunchSpeed += 30f * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
    }

    private void UpdateMinerPosition(GameTime gameTime)
    {
        var minerEntityId = gameState.Ecs.MinerEntityId;
        if (minerEntityId == null) return;

        var distance = _minerLaunchSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
        var movement = DirectionHelpers.GetDirectionalVector(distance, Direction.Top);
        var minerPosition = gameState.Ecs.GetComponent<PositionComponent>(minerEntityId.Value);
        minerPosition.Position += movement;
    }

    private void ClearLightEntities()
    {
        foreach (var entityId in _launchLightEntities) GameState.Ecs.DestroyEntity(entityId);
        _launchLightEntities.Clear();
    }
}