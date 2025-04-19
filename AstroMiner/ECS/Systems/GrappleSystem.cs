using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class GrappleSystem : System
{
    public GrappleSystem(Ecs ecs, AstroMinerGame game) : base(ecs, game)
    {
    }

    // TODO
    // public float DistanceToTarget => GrappleTarget.HasValue ? Vector2.Distance(FrontPosition, GrappleTarget.Value) : 0f;

    // public override void Disembark()
    // {
    //     base.Disembark();
    //     ResetGrapple();
    // }

    public float GetDistanceToTarget(GrappleComponent grappleComponent)
    {
        var frontPosition = Ecs.MovementSystem.GetFrontPosition(grappleComponent.EntityId);
        return grappleComponent.GrappleTarget.HasValue
            ? Vector2.Distance(frontPosition, grappleComponent.GrappleTarget.Value)
            : 0f;
    }

    private void ResetGrapple(GrappleComponent grappleComponent)
    {
        grappleComponent.GrappleTarget = null;
        grappleComponent.GrappleDirection = null;
        grappleComponent.GrappleTargetIsValid = false;
        grappleComponent.GrapplePercentToTarget = 0f;
    }

    private void SetGrappleTarget(GrappleComponent grappleComponent)
    {
        var minGrappleLength = 1;
        var directionComponent = Ecs.GetComponent<DirectionComponent>(Ecs.ActiveControllableEntityId.Value);
        var frontPosition = Ecs.MovementSystem.GetFrontPosition(grappleComponent.EntityId);
        grappleComponent.GrappleDirection = directionComponent.Direction;

        var distanceToEdgeOfCell = directionComponent.Direction switch
        {
            Direction.Top => frontPosition.Y - (int)frontPosition.Y,
            Direction.Right => (int)frontPosition.X + 1 - frontPosition.X,
            Direction.Bottom => (int)frontPosition.Y + 1 - frontPosition.Y,
            Direction.Left => frontPosition.X - (int)frontPosition.X,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Calculate offset vectors for the two grapples
        var perpendicularDir = directionComponent.Direction switch
        {
            Direction.Top or Direction.Bottom => new Vector2(1, 0),
            Direction.Left or Direction.Right => new Vector2(0, 1),
            _ => throw new ArgumentOutOfRangeException()
        };
        var leftGrappleOffset = -perpendicularDir * (GrappleComponent.GrapplesWidth / 2);
        var rightGrappleOffset = perpendicularDir * (GrappleComponent.GrapplesWidth / 2);
        var leftGrappleStart = frontPosition + leftGrappleOffset;
        var rightGrappleStart = frontPosition + rightGrappleOffset;

        for (var i = minGrappleLength; i <= GameConfig.MaxGrappleLength; i++)
        {
            var distanceToNearEdgeOfCell = Math.Max(0, distanceToEdgeOfCell - 0.1f);
            var distance = Math.Min(GameConfig.MaxGrappleLength, i + distanceToNearEdgeOfCell);

            var leftTargetToCheck = leftGrappleStart +
                                    DirectionHelpers.GetDirectionalVector(distance, directionComponent.Direction);
            var rightTargetToCheck = rightGrappleStart +
                                     DirectionHelpers.GetDirectionalVector(distance, directionComponent.Direction);

            var leftCellState = game.State.AsteroidWorld.Grid.GetCellState(leftTargetToCheck);
            var rightCellState = game.State.AsteroidWorld.Grid.GetCellState(rightTargetToCheck);

            // If either grapple hits a wall, break
            if (leftCellState.WallType != WallType.Empty || rightCellState.WallType != WallType.Empty) break;

            // Only valid if both grapples hit valid floor
            var bothHitValidFloor = FloorTypes.IsFloorLikeTileset(leftCellState.FloorType) &&
                                    FloorTypes.IsFloorLikeTileset(rightCellState.FloorType);

            if (bothHitValidFloor)
            {
                grappleComponent.GrappleTargetIsValid = true;
                // Use the midpoint between the two grapples as the target
                grappleComponent.GrappleTarget = (leftTargetToCheck + rightTargetToCheck) / 2;
            }

            // If no valid grapple targets, still use GrappleTarget for animation
            // to show checked cells
            if (!grappleComponent.GrappleTargetIsValid)
                grappleComponent.GrappleTarget = (leftTargetToCheck + rightTargetToCheck) / 2;
        }

        if (grappleComponent.GrappleTargetIsValid)
            grappleComponent.GrappleCooldownRemaining = GameConfig.GrappleCooldownMs;
    }

    private void UseGrapple(GameTime gameTime, GrappleComponent grappleComponent)
    {
        if (!grappleComponent.PrevPressedUsedGrapple && grappleComponent.GrappleAvailable)
            SetGrappleTarget(grappleComponent);

        if (!grappleComponent.GrappleTarget.HasValue) return;

        var directionComponent = Ecs.GetComponent<DirectionComponent>(Ecs.ActiveControllableEntityId.Value);

        if (directionComponent.Direction != grappleComponent.GrappleDirection)
        {
            // Has diverged from straight line
            ResetGrapple(grappleComponent);
            return;
        }

        if (grappleComponent.IsReelingIn)
        {
            if (GetDistanceToTarget(grappleComponent) < 0.1f) ResetGrapple(grappleComponent);
        }
        else // Firing grapple
        {
            var grappleTravelDistance = gameTime.ElapsedGameTime.Milliseconds / 20f;
            grappleComponent.GrapplePercentToTarget =
                Math.Min(1f,
                    grappleComponent.GrapplePercentToTarget +
                    grappleTravelDistance / GetDistanceToTarget(grappleComponent));

            if (grappleComponent.GrapplePercentToTarget == 1f && !grappleComponent.GrappleTargetIsValid)
                ResetGrapple(grappleComponent);
        }
    }

    private void UpdateReelingSpeed(GrappleComponent grappleComponent, GameTime gameTime)
    {
        if (!grappleComponent.IsReelingIn) return;

        var movementComponent = Ecs.GetComponent<MovementComponent>(grappleComponent.EntityId);
        if (movementComponent == null) return;

        // Set minimum speed
        movementComponent.CurrentSpeed = Math.Max(GrappleComponent.ReelingBaseSpeed, movementComponent.CurrentSpeed);

        // Gradually ramp up to max reeling speed
        var speedIncrease = (GrappleComponent.ReelingMaxSpeed - GrappleComponent.ReelingBaseSpeed) *
                            (gameTime.ElapsedGameTime.Milliseconds / 5000f);
        movementComponent.CurrentSpeed = Math.Min(GrappleComponent.ReelingMaxSpeed,
            movementComponent.CurrentSpeed + speedIncrease);
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeMiningControls)
    {
        if (game.State.ActiveWorld != World.Asteroid) return;

        var grappleComponent = Ecs.GetComponent<GrappleComponent>(Ecs.ActiveControllableEntityId.Value);

        if (grappleComponent is null) return;

        // Update cooldown
        grappleComponent.GrappleCooldownRemaining = Math.Max(0,
            grappleComponent.GrappleCooldownRemaining - gameTime.ElapsedGameTime.Milliseconds);

        if (activeMiningControls.Contains(MiningControls.UseGrapple))
        {
            UseGrapple(gameTime, grappleComponent);
            grappleComponent.PrevPressedUsedGrapple = true;
        }
        else
        {
            ResetGrapple(grappleComponent);
            grappleComponent.PrevPressedUsedGrapple = false;
        }

        // Update reeling speed if needed
        UpdateReelingSpeed(grappleComponent, gameTime);
    }
}