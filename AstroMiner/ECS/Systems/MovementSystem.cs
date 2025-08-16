using System;
using System.Collections.Generic;
using System.Drawing;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class MovementSystem : System
{
    private readonly List<MiningControls> _activeControlsOrder = new();

    private readonly Dictionary<MiningControls, Direction> _directionsControlsMapping = new()
    {
        { MiningControls.MoveUp, Direction.Top },
        { MiningControls.MoveRight, Direction.Right },
        { MiningControls.MoveDown, Direction.Bottom },
        { MiningControls.MoveLeft, Direction.Left }
    };

    public MovementSystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }

    private Direction? GetDirectionFromActiveControls(HashSet<MiningControls> activeMiningControls)
    {
        // Remove any controls that are no longer active
        _activeControlsOrder.RemoveAll(control => !activeMiningControls.Contains(control));

        // Add any new controls to the end of the list
        foreach (var control in activeMiningControls)
            if (!_activeControlsOrder.Contains(control))
                _activeControlsOrder.Add(control);

        // Return the direction of the most recently pressed control
        for (var i = _activeControlsOrder.Count - 1; i >= 0; i--)
            if (_directionsControlsMapping.TryGetValue(_activeControlsOrder[i], out var direction))
                return direction;

        return null;
    }

    private void UpdateSpeed(MovementComponent movement, Direction? selectedDirection, GameTime gameTime)
    {
        // Skip speed update if entity is reeling in (handled by GrappleSystem)
        var grappleComponent = Ecs.GetComponent<GrappleComponent>(movement.EntityId);
        if (grappleComponent?.IsReelingIn == true) return;

        if (movement.CurrentSpeed > movement.MaxSpeed)
        {
            movement.CurrentSpeed = Math.Max(movement.MaxSpeed,
                movement.CurrentSpeed - MovementComponent.ExcessSpeedLossPerSecond *
                (gameTime.ElapsedGameTime.Milliseconds / 1000f));
            return;
        }

        // Existing speed update logic
        if (!selectedDirection.HasValue && movement.CurrentSpeed > 0)
            movement.CurrentSpeed = Math.Max(0,
                movement.CurrentSpeed - movement.MaxSpeed *
                (gameTime.ElapsedGameTime.Milliseconds / (float)movement.TimeToStopMs));

        if ((movement.CurrentSpeed > 0 || selectedDirection.HasValue) && selectedDirection.HasValue)
            movement.CurrentSpeed = Math.Min(movement.MaxSpeed,
                movement.CurrentSpeed + movement.MaxSpeed *
                (gameTime.ElapsedGameTime.Milliseconds / (float)movement.TimeToReachMaxSpeedMs));
    }

    private void HandleDirectionChange(MovementComponent movement, Direction? selectedDirection)
    {
        var directionComponent = Ecs.GetComponent<DirectionComponent>(movement.EntityId);
        if (directionComponent == null) return;

        // Zero speed if turn 180
        if (selectedDirection.HasValue && selectedDirection.Value ==
            DirectionHelpers.GetOppositeDirection(directionComponent.Direction))
            movement.CurrentSpeed = 0f;

        if (selectedDirection.HasValue)
            directionComponent.Direction = selectedDirection.Value;
    }

    public static Direction GetRotatedDirection(Direction baseDirection, Direction rotation)
    {
        var directionInt = (int)baseDirection;
        var rotationInt = (int)rotation;
        var newDirectionInt = (directionInt + rotationInt) % 4;
        return (Direction)newDirectionInt;
    }

    private bool IsNewPositionIntersectingWithFilledCells(World world, Vector2 position, float gridWidth,
        float gridHeight)
    {
        var topLeftCell = ViewHelpers.ToGridPosition(position);
        var bottomRightCell = ViewHelpers.ToGridPosition(position + new Vector2(gridWidth, gridHeight));

        for (var x = topLeftCell.x; x <= bottomRightCell.x; x++)
        for (var y = topLeftCell.y; y <= bottomRightCell.y; y++)
            if (game.StateManager.GetWorldState(world).CellIsCollideable(x, y))
                return true;
        return false;
    }

    private bool SetNewPosIfNoCollisions(int entityId, Vector2 newPosition, PositionComponent positionComponent)
    {
        var newPositionHasCollisions = IsNewPositionIntersectingWithFilledCells(positionComponent.World, newPosition,
            positionComponent.GridWidth, positionComponent.GridHeight);

        if (newPositionHasCollisions) return false;

        // Check collisions with other entities
        var newRectangle = new RectangleF(newPosition.X, newPosition.Y, positionComponent.GridWidth,
            positionComponent.GridHeight);
        var currentRectangle = positionComponent.Rectangle;

        foreach (var otherPositionComponent in Ecs.GetAllComponents<PositionComponent>())
        {
            // Skip self and non-collideable entities
            if (otherPositionComponent.EntityId == entityId || !otherPositionComponent.IsCollideable)
                continue;

            if (otherPositionComponent.World != positionComponent.World) continue;

            if (otherPositionComponent.EntityId == Ecs.PlayerEntityId &&
                game.StateManager.AsteroidWorld.IsInMiner) continue;

            if (newRectangle.IntersectsWith(otherPositionComponent.Rectangle) &&
                !currentRectangle.IntersectsWith(otherPositionComponent
                    .Rectangle)) // Allow movement if currently clipping
                return false;
        }

        positionComponent.Position = newPosition;
        return true;
    }


    private bool ApplyVectorToPosIfNoCollisions(int entityId, Vector2 vector, PositionComponent positionComponent)
    {
        var newPosition = positionComponent.Position + vector;
        return SetNewPosIfNoCollisions(entityId, newPosition, positionComponent);
    }

    public override void Update(GameTime gameTime, ActiveControls activeControls)
    {
        var pressedDirection = GetDirectionFromActiveControls(activeControls.Mining);

        foreach (var movementComponent in Ecs.GetAllComponents<MovementComponent>())
        {
            // PortalSystem controls movement
            if (movementComponent.PortalStatus != PortalStatus.None) continue;

            var entityId = movementComponent.EntityId;
            var activeDirection =
                entityId == game.Model.Ecs.ActiveControllableEntityId.Value ? pressedDirection : null;
            var positionComponent = Ecs.GetComponent<PositionComponent>(entityId);
            var directionComponent = Ecs.GetComponent<DirectionComponent>(entityId);

            if (positionComponent == null || directionComponent == null)
                continue;

            HandleDirectionChange(movementComponent, activeDirection);
            UpdateSpeed(movementComponent, activeDirection, gameTime);

            // Update position
            if (movementComponent.CurrentSpeed > 0)
            {
                var distance = movementComponent.CurrentSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
                var movement = DirectionHelpers.GetDirectionalVector(distance, directionComponent.Direction);

                var hasCollisions = !ApplyVectorToPosIfNoCollisions(entityId, movement, positionComponent);
                if (hasCollisions)
                    movementComponent.CurrentSpeed = 0;
            }
        }
    }

    /// <summary>
    ///     Sets the position of an entity relative to another.
    /// </summary>
    /// <param name="positionComponent">The position to modify</param>
    /// <param name="directionalEntityId">The entity to move in relation to</param>
    /// <param name="rotation">The side of the origin entity to move the position to</param>
    /// <param name="insideEdge">Sets the position to inside the origin entity</param>
    /// <param name="cancelIfCollisions"></param>
    /// <returns>Whether the move was successful (false means cancelled due to collisions)</returns>
    public bool SetPositionRelativeToDirectionalEntity(PositionComponent positionComponent, int directionalEntityId,
        Direction rotation,
        bool insideEdge = false, bool cancelIfCollisions = false)
    {
        var directionalEntityMovementComponent = Ecs.GetComponent<MovementComponent>(directionalEntityId);
        var directionalEntityPositionComponent = Ecs.GetComponent<PositionComponent>(directionalEntityId);
        var directionalEntityDirectionComponent = Ecs.GetComponent<DirectionComponent>(directionalEntityId);
        if (directionalEntityMovementComponent == null || directionalEntityPositionComponent == null ||
            directionalEntityDirectionComponent == null) return true;

        var centerToCenterDistance =
            directionalEntityPositionComponent.GridWidth / 2 +
            (insideEdge
                ? -(positionComponent.GridWidth / 2)
                : positionComponent.GridWidth /
                  2); // TODO should be width OR height depending on direction. Is the same anyway for mining components
        var actualDirection = GetRotatedDirection(directionalEntityDirectionComponent.Direction, rotation);
        var newCenterPos = actualDirection switch
        {
            Direction.Top => directionalEntityPositionComponent.CenterPosition +
                             new Vector2(0, -centerToCenterDistance),
            Direction.Right => directionalEntityPositionComponent.CenterPosition +
                               new Vector2(centerToCenterDistance, 0),
            Direction.Bottom => directionalEntityPositionComponent.CenterPosition +
                                new Vector2(0, centerToCenterDistance),
            Direction.Left => directionalEntityPositionComponent.CenterPosition +
                              new Vector2(-centerToCenterDistance, 0),
            _ => directionalEntityPositionComponent.CenterPosition
        };

        var newPosition = newCenterPos - new Vector2(positionComponent.GridWidth / 2, positionComponent.GridHeight / 2);

        if (cancelIfCollisions)
            return SetNewPosIfNoCollisions(positionComponent.EntityId, newPosition, positionComponent);

        positionComponent.Position = newPosition;
        return true;
    }

    public Vector2 GetFrontPosition(int directionalEntityId)
    {
        var positionComponent = Ecs.GetComponent<PositionComponent>(directionalEntityId);
        var directionComponent = Ecs.GetComponent<DirectionComponent>(directionalEntityId);
        return positionComponent.CenterPosition +
               DirectionHelpers.GetDirectionalVector(positionComponent.GridHeight / 2f, directionComponent.Direction);
    }
}