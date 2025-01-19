using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class MinerEntity(GameState gameState) : MiningControllableEntity(gameState)
{
    private float _grapplePercentToTarget;
    private Vector2? _grappleTarget;
    private bool _grappleTargetIsValid;

    private bool _prevPressedUsedGrapple;
    protected override bool CanAddToInventory { get; } = false;
    protected override float MaxSpeed => 5f;
    protected override int TimeToReachMaxSpeedMs { get; } = 1200;
    protected override float MaxHealth => GameConfig.MinerMaxHealth;
    protected override int TimeToStopMs { get; } = 400;
    protected override int BoxSizePx { get; } = GameConfig.MinerBoxSizePx;

    protected override float DrillingWidth { get; } = 0.9f;

    private bool IsReelingIn => _grapplePercentToTarget == 1f;

    public override void Disembark()
    {
        base.Disembark();
        ResetGrapple();
    }

    private void ResetGrapple()
    {
        _grappleTarget = null;
        _grappleTargetIsValid = false;
        _grapplePercentToTarget = 0f;
    }

    private void SetGrappleTarget()
    {
        // TODO collision detection etc. Should "walk" until max or wall and return max
        var offset = GetDirectionalVector(GameConfig.MaxGrappleLength, Direction);
        _grappleTarget = Position + offset;
        _grappleTargetIsValid = true;
    }

    private void UseGrapple(int elapsedMs)
    {
        if (!_prevPressedUsedGrapple) SetGrappleTarget();

        if (!_grappleTarget.HasValue) return;

        if (_grappleTarget.Value.X != Position.X && _grappleTarget.Value.Y != Position.Y)
        {
            // Has diverged from straight line
            ResetGrapple();
            return;
        }

        var distanceToTarget = Vector2.Distance(Position, _grappleTarget.Value);

        if (!IsReelingIn)
        {
            var grappleTravelDistance = elapsedMs / 100f;
            _grapplePercentToTarget =
                Math.Min(1f, _grapplePercentToTarget + grappleTravelDistance / distanceToTarget);
        }
        else if (distanceToTarget < 0.1f)
        {
            ResetGrapple();
        }
    }

    protected override void UpdateSpeed(Direction? selectedDirection, int elapsedGameTimeMs)
    {
        if (IsReelingIn)
            CurrentSpeed = MaxSpeed * 2;
        else
            base.UpdateSpeed(selectedDirection, elapsedGameTimeMs);
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