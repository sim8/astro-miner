using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class PortalSystem : System
{
    public PortalSystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }

    private void MoveToTargetWorld(PortalConfig config, PositionComponent position, MovementComponent movement)
    {
        position.World = config.TargetWorld;
        movement.PortalStatus = PortalStatus.Arriving;

        var (targetX, targetY) = config.Coordinates;

        var centerPosition = config.Direction switch
        {
            Direction.Top => new Vector2(targetX + 0.5f, targetY + 0.99f), // Bottom center of target cell
            Direction.Bottom => new Vector2(targetX + 0.5f, targetY + 0.01f), // Top center of target cell
            Direction.Left => new Vector2(targetX + 0.99f, targetY + 0.5f), // Right center of target cell
            Direction.Right => new Vector2(targetX + 0.01f, targetY + 0.5f), // Left center of target cell
            _ => new Vector2(targetX + 0.5f, targetY + 0.5f)
        };

        position.SetCenterPosition(centerPosition);

        // Only change active world if this entity is the player.
        if (position.EntityId == game.State.Ecs.ActiveControllableEntityId)
            game.State.SetActiveWorldAndInitialize(config.TargetWorld);
    }

    private void MoveToDeparturePoint(MovementComponent movement, PositionComponent position,
        DirectionComponent dirComp,
        PortalConfig config, GameTime gameTime)
    {
        var center = position.CenterPosition;
        // Compute how far we can move in this frame.
        var deltaDistance = movement.CurrentSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000f);

        // Compute the center of the cell for alignment purposes.
        var cellCenter = new Vector2((float)Math.Floor(center.X) + 0.5f, (float)Math.Floor(center.Y) + 0.5f);

        // For vertical portals, align on X then move on Y.
        if (config.Direction == Direction.Top || config.Direction == Direction.Bottom)
        {
            var offsetX = center.X - cellCenter.X;
            // If not horizontally aligned, adjust on the X axis.
            if (Math.Abs(offsetX) > 0.001f)
            {
                // Determine whether to move left or right to center.
                var alignDir = offsetX > 0 ? Direction.Left : Direction.Right;
                var travel = Math.Min(deltaDistance, Math.Abs(offsetX));
                position.Position += DirectionHelpers.GetDirectionalVector(travel, alignDir);
                // Set directional indicator for aesthetics.
                dirComp.Direction = alignDir;
            }
            else
            {
                // Already centered horizontally, so now move vertically.
                var halfCell = position.GridHeight / 2f;
                // Calculate the target Y edge of the portal cell.
                var targetY = config.Direction == Direction.Top
                    ? cellCenter.Y - halfCell
                    : cellCenter.Y + halfCell;
                // Remaining distance along Y (note: for Top, distance is positive if center.Y is above target).
                var remaining = config.Direction == Direction.Top
                    ? center.Y - targetY
                    : targetY - center.Y;
                var travel = Math.Min(deltaDistance, remaining);
                dirComp.Direction = config.Direction;
                position.Position += DirectionHelpers.GetDirectionalVector(travel, config.Direction);
                // If reached the edge, trigger portal teleport.
                if (Math.Abs(remaining - travel) < 0.001f) MoveToTargetWorld(config, position, movement);
            }
        }
        // For horizontal portals, align on Y then move on X.
        else if (config.Direction == Direction.Left || config.Direction == Direction.Right)
        {
            var offsetY = center.Y - cellCenter.Y;
            if (Math.Abs(offsetY) > 0.001f)
            {
                var alignDir = offsetY > 0 ? Direction.Top : Direction.Bottom;
                var travel = Math.Min(deltaDistance, Math.Abs(offsetY));
                position.Position += DirectionHelpers.GetDirectionalVector(travel, alignDir);
                dirComp.Direction = alignDir;
            }
            else
            {
                var halfCell = position.GridWidth / 2f;
                var targetX = config.Direction == Direction.Left
                    ? cellCenter.X - halfCell
                    : cellCenter.X + halfCell;
                var remaining = config.Direction == Direction.Left
                    ? center.X - targetX
                    : targetX - center.X;
                var travel = Math.Min(deltaDistance, remaining);
                dirComp.Direction = config.Direction;
                position.Position += DirectionHelpers.GetDirectionalVector(travel, config.Direction);
                if (Math.Abs(remaining - travel) < 0.001f) MoveToTargetWorld(config, position, movement);
            }
        }
    }

    private void MoveToArrivalPoint(PositionComponent position, MovementComponent movement,
        DirectionComponent direction, GameTime gameTime)
    {
        var (topLeftGridX, topLeftGridY) = ViewHelpers.ToGridPosition(position.Position);
        var (bottomRightGridX, bottomRightGridY) =
            ViewHelpers.ToGridPosition(position.Position + new Vector2(position.GridWidth, position.GridHeight));
        if (!game.State.ActiveWorldState.CellIsPortal(topLeftGridX, topLeftGridY) &&
            !game.State.ActiveWorldState.CellIsPortal(bottomRightGridX, bottomRightGridY))
        {
            movement.PortalStatus = PortalStatus.None;
        }
        else
        {
            var travelDistance = movement.CurrentSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
            position.Position += DirectionHelpers.GetDirectionalVector(travelDistance, direction.Direction);
        }
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        foreach (var movement in Ecs.GetAllComponents<MovementComponent>())
        {
            var entityId = movement.EntityId;
            var position = Ecs.GetComponent<PositionComponent>(entityId);
            var dirComp = Ecs.GetComponent<DirectionComponent>(entityId);

            var center = position.CenterPosition;
            var (gridX, gridY) = ViewHelpers.ToGridPosition(center);


            if (movement.PortalStatus == PortalStatus.Arriving)
                MoveToArrivalPoint(position, movement, dirComp, gameTime);

            if (!game.State.ActiveWorldState.CellIsPortal(gridX, gridY))
                continue;

            var config = WorldGrid.GetPortalConfig(position.World, (gridX, gridY));

            if (movement.PortalStatus == PortalStatus.Departing)
                MoveToDeparturePoint(movement, position, dirComp, config, gameTime);


            if (movement.PortalStatus == PortalStatus.None)
            {
                movement.PortalStatus = PortalStatus.Departing;
                movement.CurrentSpeed = GameConfig.Speeds.Walking;
            }
        }
    }
}