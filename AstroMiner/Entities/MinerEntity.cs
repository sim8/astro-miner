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
    private bool _grappleTargetIsValid;

    private bool _prevPressedUsedGrapple;
    public float GrapplePercentToTarget;
    public Vector2? GrappleTarget;
    protected override bool CanAddToInventory { get; } = false;
    protected override float MaxSpeed => 5f;
    protected override int TimeToReachMaxSpeedMs { get; } = 1200;
    protected override float MaxHealth => GameConfig.MinerMaxHealth;
    protected override int TimeToStopMs { get; } = 400;
    protected override int BoxSizePx { get; } = GameConfig.MinerBoxSizePx;

    protected override float DrillingWidth { get; } = 0.9f;

    public float DistanceToTarget => GrappleTarget.HasValue ? Vector2.Distance(Position, GrappleTarget.Value) : 0f;

    private bool IsReelingIn => GrapplePercentToTarget == 1f;

    public override void Disembark()
    {
        base.Disembark();
        ResetGrapple();
    }

    private void ResetGrapple()
    {
        GrappleTarget = null;
        _grappleTargetIsValid = false;
        GrapplePercentToTarget = 0f;
    }

    private void SetGrappleTarget()
    {
        // TODO collision detection etc. Should "walk" until max or wall and return max
        var offset = DirectionHelpers.GetDirectionalVector(GameConfig.MaxGrappleLength, Direction);
        GrappleTarget = Position + offset;
        _grappleTargetIsValid = true;
    }

    private void UseGrapple(int elapsedMs)
    {
        if (!_prevPressedUsedGrapple) SetGrappleTarget();

        if (!GrappleTarget.HasValue) return;

        if (GrappleTarget.Value.X != Position.X && GrappleTarget.Value.Y != Position.Y)
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