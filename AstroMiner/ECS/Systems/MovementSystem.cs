using System;
using System.Collections.Generic;
using System.Drawing;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class MovementSystem : System
{
    private readonly Dictionary<MiningControls, Direction> _directionsControlsMapping = new()
    {
        { MiningControls.MoveUp, Direction.Top },
        { MiningControls.MoveRight, Direction.Right },
        { MiningControls.MoveDown, Direction.Bottom },
        { MiningControls.MoveLeft, Direction.Left }
    };

    private readonly List<MiningControls> _activeControlsOrder = new();

    public MovementSystem(Ecs ecs, GameState gameState) : base(ecs, gameState)
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
                movement.CurrentSpeed - MovementComponent.ExcessSpeedLossPerSecond * (gameTime.ElapsedGameTime.Milliseconds / 1000f));
            return;
        }

        // Existing speed update logic
        if (!selectedDirection.HasValue && movement.CurrentSpeed > 0)
            movement.CurrentSpeed = Math.Max(0,
                movement.CurrentSpeed - movement.MaxSpeed * (gameTime.ElapsedGameTime.Milliseconds / (float)movement.TimeToStopMs));

        if ((movement.CurrentSpeed > 0 || selectedDirection.HasValue) && selectedDirection.HasValue)
            movement.CurrentSpeed = Math.Min(movement.MaxSpeed,
                movement.CurrentSpeed + movement.MaxSpeed * (gameTime.ElapsedGameTime.Milliseconds / (float)movement.TimeToReachMaxSpeedMs));
    }

    private void HandleDirectionChange(MovementComponent movement, Direction? selectedDirection)
    {
        // Zero speed if turn 180
        if (selectedDirection.HasValue && selectedDirection.Value == DirectionHelpers.GetOppositeDirection(movement.Direction))
            movement.CurrentSpeed = 0f;

        movement.Direction = selectedDirection ?? movement.Direction;
    }

    public static Direction GetRotatedDirection(Direction baseDirection, Direction rotation)
    {
        var directionInt = (int)baseDirection;
        var rotationInt = (int)rotation;
        var newDirectionInt = (directionInt + rotationInt) % 4;
        return (Direction)newDirectionInt;
    }

    private bool IsNewPositionIntersectingWithFilledCells(Vector2 position, float gridBoxSize)
    {
        var topLeftCell = ViewHelpers.ToGridPosition(position);
        var bottomRightCell = ViewHelpers.ToGridPosition(position + new Vector2(gridBoxSize, gridBoxSize));

        for (var x = topLeftCell.x; x <= bottomRightCell.x; x++)
            for (var y = topLeftCell.y; y <= bottomRightCell.y; y++)
                if (GameState.ActiveWorldState.CellIsCollideable(x, y))
                    return true;
        return false;
    }

    private bool ApplyVectorToPosIfNoCollisions(int entityId, Vector2 vector, PositionComponent positionComponent)
    {
        var newPosition = positionComponent.Position + vector;
        var newPositionHasCollisions = IsNewPositionIntersectingWithFilledCells(newPosition, positionComponent.GridBoxSize);

        if (newPositionHasCollisions) return false;

        // Check collisions with other entities
        var newRectangle = new RectangleF(newPosition.X, newPosition.Y, positionComponent.GridBoxSize, positionComponent.GridBoxSize);
        var currentRectangle = positionComponent.Rectangle;

        foreach (var otherPositionComponent in Ecs.GetAllComponents<PositionComponent>())
        {
            // Skip self and non-collideable entities
            if (otherPositionComponent.EntityId == entityId || !otherPositionComponent.IsCollideable)
                continue;

            if (otherPositionComponent.World != positionComponent.World) continue;

            if (otherPositionComponent.EntityId == Ecs.PlayerEntityId && GameState.AsteroidWorld.IsInMiner) continue;

            if (newRectangle.IntersectsWith(otherPositionComponent.Rectangle) &&
                !currentRectangle.IntersectsWith(otherPositionComponent.Rectangle)) // Allow movement if currently clipping
            {
                return false;
            }
        }

        positionComponent.Position += vector;
        return true;
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        var pressedDirection = GetDirectionFromActiveControls(activeControls);

        foreach (var movementComponent in Ecs.GetAllComponents<MovementComponent>())
        {
            var entityId = movementComponent.EntityId;
            var activeDirection = entityId == GameState.Ecs.ActiveControllableEntityId ? pressedDirection : null;
            var positionComponent = Ecs.GetComponent<PositionComponent>(entityId);

            if (positionComponent == null)
                continue;

            // Update direction and speed
            HandleDirectionChange(movementComponent, activeDirection);
            UpdateSpeed(movementComponent, activeDirection, gameTime);

            // Update position
            if (movementComponent.CurrentSpeed > 0)
            {
                var distance = movementComponent.CurrentSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
                var movement = DirectionHelpers.GetDirectionalVector(distance, movementComponent.Direction);

                var hasCollisions = !ApplyVectorToPosIfNoCollisions(entityId, movement, positionComponent);
                if (hasCollisions)
                    movementComponent.CurrentSpeed = 0;
            }
        }
    }

    public void SetPositionRelativeToDirectionalEntity(PositionComponent positionComponent, int directionalEntityId, Direction rotation,
    bool insideEdge = false)
    {

        var directionalEntityMovementComponent = Ecs.GetComponent<MovementComponent>(directionalEntityId);
        var directionalEntityPositionComponent = Ecs.GetComponent<PositionComponent>(directionalEntityId);
        if (directionalEntityMovementComponent == null || directionalEntityPositionComponent == null) return;



        var centerToCenterDistance =
            directionalEntityPositionComponent.GridBoxSize / 2 + (insideEdge ? -(positionComponent.GridBoxSize / 2) : positionComponent.GridBoxSize / 2);
        var actualDirection = GetRotatedDirection(directionalEntityMovementComponent.Direction, rotation);
        var newCenterPos = actualDirection switch
        {
            Direction.Top => directionalEntityPositionComponent.CenterPosition + new Vector2(0, -centerToCenterDistance),
            Direction.Right => directionalEntityPositionComponent.CenterPosition + new Vector2(centerToCenterDistance, 0),
            Direction.Bottom => directionalEntityPositionComponent.CenterPosition + new Vector2(0, centerToCenterDistance),
            Direction.Left => directionalEntityPositionComponent.CenterPosition + new Vector2(-centerToCenterDistance, 0),
            _ => directionalEntityPositionComponent.CenterPosition
        };

        positionComponent.Position = newCenterPos - new Vector2(positionComponent.GridBoxSize / 2, positionComponent.GridBoxSize / 2);
    }

    public Vector2 GetFrontPosition(int directionalEntityId)
    {
        var positionComponent = Ecs.GetComponent<PositionComponent>(directionalEntityId);
        var movementComponent = Ecs.GetComponent<MovementComponent>(directionalEntityId);
        return positionComponent.CenterPosition + DirectionHelpers.GetDirectionalVector(positionComponent.GridBoxSize / 2f, movementComponent.Direction);
    }
}