using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class MinerEntity(GameState gameState) : MiningControllableEntity(gameState)
{
    private const float ReelingBaseSpeed = 7f;
    private const float ReelingMaxSpeed = 11f;
    public const float GrapplesWidth = 0.4f;
    private int _grappleCooldownRemaining;

    private Direction? _grappleDirection;
    private bool _grappleTargetIsValid;
    private bool _prevPressedUsedGrapple;
    public float GrapplePercentToTarget;
    public Vector2? GrappleTarget;
    protected override bool CanAddToInventory { get; } = false;
    protected override float MaxSpeed => 4f;
    protected override int TimeToReachMaxSpeedMs { get; } = 600;
    protected override float MaxHealth => GameConfig.MinerMaxHealth;
    protected override int TimeToStopMs { get; } = 400;
    protected override int BoxSizePx { get; } = GameConfig.MinerBoxSizePx;

    protected override float DrillingWidth { get; } = 0.9f;

    public float DistanceToTarget => GrappleTarget.HasValue ? Vector2.Distance(FrontPosition, GrappleTarget.Value) : 0f;

    public bool GrappleAvailable => _grappleCooldownRemaining == 0;

    private bool IsReelingIn => GrapplePercentToTarget == 1f && _grappleTargetIsValid;

    public override void Disembark()
    {
        base.Disembark();
        ResetGrapple();
    }

    private void ResetGrapple()
    {
        GrappleTarget = null;
        _grappleDirection = null;
        _grappleTargetIsValid = false;
        GrapplePercentToTarget = 0f;
    }

    private void SetGrappleTarget()
    {
        var minGrappleLength = 1;
        _grappleDirection = Direction;

        var distanceToEdgeOfCell = Direction switch
        {
            Direction.Top => FrontPosition.Y - (int)FrontPosition.Y,
            Direction.Right => (int)FrontPosition.X + 1 - FrontPosition.X,
            Direction.Bottom => (int)FrontPosition.Y + 1 - FrontPosition.Y,
            Direction.Left => FrontPosition.X - (int)FrontPosition.X,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Calculate offset vectors for the two grapples
        var perpendicularDir = Direction switch
        {
            Direction.Top or Direction.Bottom => new Vector2(1, 0),
            Direction.Left or Direction.Right => new Vector2(0, 1),
            _ => throw new ArgumentOutOfRangeException()
        };
        var leftGrappleOffset = -perpendicularDir * (GrapplesWidth / 2);
        var rightGrappleOffset = perpendicularDir * (GrapplesWidth / 2);
        var leftGrappleStart = FrontPosition + leftGrappleOffset;
        var rightGrappleStart = FrontPosition + rightGrappleOffset;

        for (var i = minGrappleLength; i <= GameConfig.MaxGrappleLength; i++)
        {
            var distanceToNearEdgeOfCell = Math.Max(0, distanceToEdgeOfCell - 0.1f);
            var distance = Math.Min(GameConfig.MaxGrappleLength, i + distanceToNearEdgeOfCell);

            var leftTargetToCheck = leftGrappleStart + DirectionHelpers.GetDirectionalVector(distance, Direction);
            var rightTargetToCheck = rightGrappleStart + DirectionHelpers.GetDirectionalVector(distance, Direction);

            var leftCellState = gameState.AsteroidWorld.Grid.GetCellState(leftTargetToCheck);
            var rightCellState = gameState.AsteroidWorld.Grid.GetCellState(rightTargetToCheck);

            // If either grapple hits a wall, break
            if (leftCellState.WallType != WallType.Empty || rightCellState.WallType != WallType.Empty) break;

            // Only valid if both grapples hit valid floor
            var bothHitValidFloor = FloorTypes.IsFloorLikeTileset(leftCellState.FloorType) &&
                                    FloorTypes.IsFloorLikeTileset(rightCellState.FloorType);

            if (bothHitValidFloor)
            {
                _grappleTargetIsValid = true;
                // Use the midpoint between the two grapples as the target
                GrappleTarget = (leftTargetToCheck + rightTargetToCheck) / 2;
            }

            // If no valid grapple targets, still use GrappleTarget for animation
            // to show checked cells
            if (!_grappleTargetIsValid) GrappleTarget = (leftTargetToCheck + rightTargetToCheck) / 2;
        }

        if (_grappleTargetIsValid) _grappleCooldownRemaining = GameConfig.GrappleCooldownMs;
    }

    private void UseGrapple(GameTime gameTime)
    {
        if (!_prevPressedUsedGrapple && GrappleAvailable) SetGrappleTarget();

        if (!GrappleTarget.HasValue) return;

        if (Direction != _grappleDirection)
        {
            // Has diverged from straight line
            ResetGrapple();
            return;
        }

        if (IsReelingIn)
        {
            if (DistanceToTarget < 0.1f) ResetGrapple();
        }
        else // Firing grapple
        {
            var grappleTravelDistance = gameTime.ElapsedGameTime.Milliseconds / 20f;
            GrapplePercentToTarget =
                Math.Min(1f, GrapplePercentToTarget + grappleTravelDistance / DistanceToTarget);

            if (GrapplePercentToTarget == 1f && !_grappleTargetIsValid) ResetGrapple();
        }
    }

    protected override void UpdateSpeed(Direction? selectedDirection, GameTime gameTime)
    {
        if (IsReelingIn)
        {
            CurrentSpeed = Math.Max(ReelingBaseSpeed, CurrentSpeed);
            // Gradually ramp up to max reeling speed
            var speedIncrease = (ReelingMaxSpeed - ReelingBaseSpeed) * (gameTime.ElapsedGameTime.Milliseconds / 5000f);
            CurrentSpeed = Math.Min(ReelingMaxSpeed, CurrentSpeed + speedIncrease);
        }
        else
        {
            base.UpdateSpeed(selectedDirection, gameTime);
        }
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeMiningControls)
    {
        base.Update(gameTime, activeMiningControls);

        // Update cooldown
        _grappleCooldownRemaining = Math.Max(0, _grappleCooldownRemaining - gameTime.ElapsedGameTime.Milliseconds);

        if (activeMiningControls.Contains(MiningControls.UseGrapple))
        {
            UseGrapple(gameTime);
            _prevPressedUsedGrapple = true;
        }
        else
        {
            ResetGrapple();
            _prevPressedUsedGrapple = false;
        }
    }

    public override Vector2 GetDirectionalLightSource()
    {
        return Direction switch
        {
            Direction.Top => Position + new Vector2(1.06f, 0.34f),
            Direction.Right => Position + new Vector2(0.70f, 0.66f),
            Direction.Bottom => Position + new Vector2(0.12f, 0.58f),
            Direction.Left => Position + new Vector2(0.48f, -0.28f),
            _ => Position
        };
    }

    protected override void OnDead()
    {
        gameState.Ecs.ExplosionSystem.CreateExplosion(CenterPosition);
    }
}