using System;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class SlidingDoorSystem : System
{
    private float frontOpenDistance = 1f;
    private float rearOpenDistance = 2f;
    private float minOpenTimeMs = 2000;

    // Animation tuning variables
    private float doorAnimationSpeedMs = 800f; // Time to fully open/close
    private float easeInOutPower = 2f; // Higher = more pronounced ease effect

    public SlidingDoorSystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }

    /// <summary>
    /// Ease-in-out function for smooth animations
    /// </summary>
    private float EaseInOut(float t)
    {
        if (t < 0.5f)
        {
            return (float)(Math.Pow(2 * t, easeInOutPower) / 2);
        }
        else
        {
            return (float)(1 - Math.Pow(2 * (1 - t), easeInOutPower) / 2);
        }
    }


    public override void Update(GameTime gameTime, ActiveControls activeControls)
    {
        foreach (var slidingDoorComponent in game.StateManager.Ecs.GetAllComponentsInActiveWorld<SlidingDoorComponent>())
        {
            bool shouldOpen = false;

            // Check all entities with movement components to see if any are within range
            foreach (var movementComponent in game.StateManager.Ecs.GetAllComponentsInActiveWorld<MovementComponent>())
            {
                // Get the position component for the same entity
                var positionComponent = game.StateManager.Ecs.GetComponent<PositionComponent>(movementComponent.EntityId);

                if (positionComponent != null)
                {
                    // Calculate distance between the sliding door and the moving entity
                    var distance = EntityHelpers.GetDistanceBetween(game.StateManager.Ecs,
                        slidingDoorComponent.EntityId, movementComponent.EntityId);

                    // Get door position to determine if entity is behind or in front
                    var slidingDoorPositionComponent = game.StateManager.Ecs.GetComponent<PositionComponent>(slidingDoorComponent.EntityId);

                    if (slidingDoorPositionComponent != null)
                    {
                        float thresholdDistance = positionComponent.CenterPosition.Y < slidingDoorPositionComponent.CenterPosition.Y ? rearOpenDistance : frontOpenDistance;

                        // If within range, mark door to open
                        if (distance <= thresholdDistance)
                        {
                            shouldOpen = true;
                            break; // No need to check other entities once we found one in range
                        }
                    }
                }
            }

            // Determine target door state with minimum open time logic
            bool targetShouldBeOpen = false;

            if (shouldOpen)
            {
                // Entity is nearby - door should open
                targetShouldBeOpen = true;

                // Record when door was opened if it wasn't already targeting open
                if (!slidingDoorComponent.TargetOpen)
                {
                    slidingDoorComponent.LastOpenedTimeMs = gameTime.TotalGameTime.TotalMilliseconds;
                }
            }
            else
            {
                // No entity nearby - check if door can close yet
                var timeSinceOpened = gameTime.TotalGameTime.TotalMilliseconds - slidingDoorComponent.LastOpenedTimeMs;

                // Keep door open if it hasn't been open for minimum time and was previously targeting open
                if (slidingDoorComponent.TargetOpen && timeSinceOpened < minOpenTimeMs)
                {
                    targetShouldBeOpen = true;
                }
            }

            // Start new animation if target state changed
            if (targetShouldBeOpen != slidingDoorComponent.TargetOpen)
            {
                slidingDoorComponent.TargetOpen = targetShouldBeOpen;
                slidingDoorComponent.AnimationTimeMs = 0;
                slidingDoorComponent.AnimationStartPercent = slidingDoorComponent.OpenPercent;
            }

            // Update animation time and calculate progress
            slidingDoorComponent.AnimationTimeMs += gameTime.ElapsedGameTime.TotalMilliseconds;
            var animationProgress = Math.Min(1.0, slidingDoorComponent.AnimationTimeMs / doorAnimationSpeedMs);

            if (animationProgress < 1.0)
            {
                // Animation in progress
                var easedProgress = EaseInOut((float)animationProgress);
                var targetPercent = slidingDoorComponent.TargetOpen ? 1f : 0f;
                slidingDoorComponent.OpenPercent = slidingDoorComponent.AnimationStartPercent +
                    (targetPercent - slidingDoorComponent.AnimationStartPercent) * easedProgress;
            }
            else
            {
                // Animation complete - snap to target
                slidingDoorComponent.OpenPercent = slidingDoorComponent.TargetOpen ? 1f : 0f;
            }

            // Set collision state - door should not be collideable when fully open
            var doorPositionComponent = game.StateManager.Ecs.GetComponent<PositionComponent>(slidingDoorComponent.EntityId);
            if (doorPositionComponent != null)
            {
                doorPositionComponent.IsCollideable = slidingDoorComponent.OpenPercent < 0.2f;
            }
        }
    }

}