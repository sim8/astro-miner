using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class MinerEntity(GameState gameState) : MiningControllableEntity(gameState)
{
    private const float ReelingBaseSpeed = 8f;
    private const float ReelingMaxSpeed = 16f;
    private Direction? _grappleDirection;
    private bool _grappleTargetIsValid;

    private bool _prevPressedUsedGrapple;
    public float GrapplePercentToTarget;
    public Vector2? GrappleTarget;
    protected override bool CanAddToInventory { get; } = false;
    protected override float MaxSpeed => 4f;
    protected override int TimeToReachMaxSpeedMs { get; } = 1200;
    protected override float MaxHealth => GameConfig.MinerMaxHealth;
    protected override int TimeToStopMs { get; } = 400;
    protected override int BoxSizePx { get; } = GameConfig.MinerBoxSizePx;

    protected override float DrillingWidth { get; } = 0.9f;

    public float DistanceToTarget => GrappleTarget.HasValue ? Vector2.Distance(FrontPosition, GrappleTarget.Value) : 0f;

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

        for (var i = minGrappleLength; i <= GameConfig.MaxGrappleLength; i++)
        {
            var distanceToNearEdgeOfCell = Math.Max(0, distanceToEdgeOfCell - 0.1f);
            var distance = Math.Min(GameConfig.MaxGrappleLength, i + distanceToNearEdgeOfCell);

            var targetToCheck = FrontPosition + DirectionHelpers.GetDirectionalVector(distance, Direction);
            var cellState = gameState.Grid.GetCellState(targetToCheck);

            // Collision, early return
            if (cellState.WallType != WallType.Empty) return;

            // If is valid target, set target
            if (FloorTypes.IsFloorLikeTileset(cellState.FloorType))
            {
                // TODO set to far side of cell (if under max)
                _grappleTargetIsValid = true;
                GrappleTarget = targetToCheck;
            }

            // If no valid grapple targets, still use GrappleTarget for animation
            // to show checked cells
            if (!_grappleTargetIsValid) GrappleTarget = targetToCheck;
        }
    }

    private void UseGrapple(int elapsedMs)
    {
        if (!_prevPressedUsedGrapple) SetGrappleTarget();

        if (!GrappleTarget.HasValue) return;

        if (Direction != _grappleDirection)
        {
            // Has diverged from straight line
            ResetGrapple();
            return;
        }

        if (!IsReelingIn)
        {
            var grappleTravelDistance = elapsedMs / 20f;
            GrapplePercentToTarget =
                Math.Min(1f, GrapplePercentToTarget + grappleTravelDistance / DistanceToTarget);

            if (GrapplePercentToTarget == 1f && !_grappleTargetIsValid) ResetGrapple();
        }
        else if (DistanceToTarget < 0.1f)
        {
            ResetGrapple();
        }
    }

    protected override void UpdateSpeed(Direction? selectedDirection, int elapsedGameTimeMs)
    {
        if (IsReelingIn)
        {
            CurrentSpeed = Math.Max(ReelingBaseSpeed, CurrentSpeed);
            // Gradually ramp up to max reeling speed
            var speedIncrease = (ReelingMaxSpeed - ReelingBaseSpeed) * (elapsedGameTimeMs / 5000f);
            CurrentSpeed = Math.Min(ReelingMaxSpeed, CurrentSpeed + speedIncrease);
        }
        else
        {
            base.UpdateSpeed(selectedDirection, elapsedGameTimeMs);
        }
    }

    public override void Update(int elapsedMs, HashSet<MiningControls> activeMiningControls)
    {
        base.Update(elapsedMs, activeMiningControls);
        if (activeMiningControls.Contains(MiningControls.UseGrapple))
        {
            UseGrapple(elapsedMs);
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
        var explosionEntity = new ExplosionEntity(gameState, CenterPosition);
        gameState.ActivateEntity(explosionEntity);
    }
}