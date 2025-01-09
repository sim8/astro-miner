using System;
using System.Collections.Generic;
using System.Drawing;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class ControllableEntity : Entity
{
    private readonly Dictionary<MiningControls, Direction> _directionsControlsMapping = new()
    {
        { MiningControls.MoveUp, Direction.Top },
        { MiningControls.MoveRight, Direction.Right },
        { MiningControls.MoveDown, Direction.Bottom },
        { MiningControls.MoveLeft, Direction.Left }
    };

    protected readonly GameState GameState;
    private float _currentSpeed;

    public ControllableEntity(GameState gameState)
    {
        GameState = gameState;
    }

    // Includes time taking damage + time since last damage
    public int TotalDamageAnimationTimeMs { get; private set; }

    public bool IsAnimatingDamage { get; private set; }

    // Caps at GameConfig.DamageAnimationTimeMs
    private int TimeSinceLastDamage { get; set; }

    public float Health { get; private set; }
    public bool IsDead { get; set; }
    public bool IsOffAsteroid { get; set; }

    protected virtual float MaxSpeed => 1f;
    protected virtual int TimeToReachMaxSpeedMs { get; } = 0;
    protected virtual int TimeToStopMs { get; } = 0;
    protected virtual float MaxHealth { get; } = 100;

    public Direction Direction { get; private set; } = Direction.Top;

    public void Initialize(Vector2 pos)
    {
        Position = pos;
        Health = MaxHealth;
        IsDead = false;
        IsOffAsteroid = false;
    }

    protected virtual void OnDead()
    {
    }

    public void TakeDamage(float damage)
    {
        Health = Math.Max(0, Health - damage);

        if (Health == 0 && !IsDead)
        {
            IsDead = true;
            OnDead();
        }

        IsAnimatingDamage = true;
        TimeSinceLastDamage = 0;
    }

    private Direction? GetDirectionFromActiveControls(HashSet<MiningControls> activeMiningControls)
    {
        foreach (var control in activeMiningControls)
            if (_directionsControlsMapping.TryGetValue(control, out var direction))
                return direction;
        return null;
    }

    private bool IsNewPositionIntersectingWithFilledCells(Vector2 position)
    {
        var topLeftCell = ViewHelpers.ToGridPosition(position);
        var bottomRightCell = ViewHelpers.ToGridPosition(position + new Vector2(GridBoxSize, GridBoxSize));

        for (var x = topLeftCell.x; x <= bottomRightCell.x; x++)
        for (var y = topLeftCell.y; y <= bottomRightCell.y; y++)
            if (GameState.Grid.GetWallType(x, y) != null)
                return true;

        return false;
    }

    private bool ApplyVectorToPosIfNoCollisions(Vector2 vector)
    {
        var newVector = Position + vector;
        var newPositionHasCollisions = IsNewPositionIntersectingWithFilledCells(newVector);

        if (newPositionHasCollisions) return false;

        var newRectangle = new RectangleF(newVector.X, newVector.Y, GridBoxSize, GridBoxSize);

        foreach (var entity in GameState.ActiveEntitiesSortedByDistance)
            if (entity != this && entity.CanCollide && newRectangle.IntersectsWith(entity.Rectangle) &&
                !Rectangle.IntersectsWith(entity.Rectangle)) // Allow movement if currently clipping
                return false;

        Position += vector;
        return true;
    }

    public override void Update(int elapsedMs, HashSet<MiningControls> activeMiningControls)
    {
        CheckIfShouldFallOrTakeDamage(elapsedMs);

        if (IsAnimatingDamage)
        {
            // We need total for smooth animation, and timeSinceLastDamage to know when to stop showing
            TotalDamageAnimationTimeMs += elapsedMs;
            TimeSinceLastDamage += elapsedMs;
            if (TimeSinceLastDamage >= GameConfig.DamageAnimationTimeMs)
            {
                TotalDamageAnimationTimeMs = 0;
                TimeSinceLastDamage = 0;
                IsAnimatingDamage = false;
            }
        }

        var direction = GetDirectionFromActiveControls(activeMiningControls);
        UpdateMinerPosAndSpeed(direction, elapsedMs);
    }

    private void CheckIfShouldFallOrTakeDamage(int elapsedMs)
    {
        var (topLeftX, topLeftY) = ViewHelpers.ToGridPosition(Position);
        var (bottomRightX, bottomRightY) = ViewHelpers.ToGridPosition(Position + new Vector2(GridBoxSize, GridBoxSize));

        var allCellsAreEmpty = true;
        var someCellsAreLava = false;

        for (var x = topLeftX; x <= bottomRightX; x++)
        for (var y = topLeftY; y <= bottomRightY; y++)
        {
            var floorType = GameState.Grid.GetFloorType(x, y);
            if (floorType != null) allCellsAreEmpty = false;
            if (floorType == FloorType.Lava) someCellsAreLava = true;
        }

        if (someCellsAreLava) TakeDamage((float)GameConfig.LavaDamagePerSecond / 1000 * elapsedMs);
        if (allCellsAreEmpty) IsOffAsteroid = true;
    }

    private void UpdateMinerPosAndSpeed(Direction? selectedDirection, int elapsedGameTimeMs)
    {
        Direction = selectedDirection ?? Direction;

        // Decelerate if nothing pressed
        if (!selectedDirection.HasValue && _currentSpeed > 0)
            _currentSpeed = Math.Max(0,
                _currentSpeed - MaxSpeed * (elapsedGameTimeMs / (float)TimeToStopMs));

        if (_currentSpeed > 0 || selectedDirection.HasValue)
        {
            var hasCollisions = UpdateMinerPos(elapsedGameTimeMs);
            if (hasCollisions)
                _currentSpeed = 0;
            // Accelerate if direction pressed
            else if (selectedDirection.HasValue)
                _currentSpeed = Math.Min(MaxSpeed,
                    _currentSpeed + MaxSpeed * (elapsedGameTimeMs / (float)TimeToReachMaxSpeedMs));
        }
    }

    private bool UpdateMinerPos(int elapsedGameTimeMs)
    {
        var distance = _currentSpeed * (elapsedGameTimeMs / 1000f);

        var movement = Direction switch
        {
            Direction.Top => new Vector2(0, -distance),
            Direction.Right => new Vector2(distance, 0),
            Direction.Bottom => new Vector2(0, distance),
            Direction.Left => new Vector2(-distance, 0),
            _ => Vector2.Zero
        };

        return !ApplyVectorToPosIfNoCollisions(movement);
    }


    public Direction GetRotatedDirection(Direction rotation)
    {
        var directionInt = (int)Direction;
        var rotationInt = (int)rotation;
        var newDirectionInt = (directionInt + rotationInt) % 4;
        return (Direction)newDirectionInt;
    }

    public virtual Vector2 GetDirectionalLightSource()
    {
        // Can be adjusted if lightsource has different placements per rotation
        return Position;
    }
}