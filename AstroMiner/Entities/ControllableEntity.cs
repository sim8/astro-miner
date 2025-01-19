using System;
using System.Collections.Generic;
using System.Drawing;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class ControllableEntity : Entity
{
    // If speed exceeds max, gradually ramp back down
    private const int TimeToSlowToMax = 2000;

    private readonly Dictionary<MiningControls, Direction> _directionsControlsMapping = new()
    {
        { MiningControls.MoveUp, Direction.Top },
        { MiningControls.MoveRight, Direction.Right },
        { MiningControls.MoveDown, Direction.Bottom },
        { MiningControls.MoveLeft, Direction.Left }
    };

    protected readonly GameState GameState;
    protected float CurrentSpeed;

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

    public Vector2 FrontPosition => CenterPosition + GetDirectionalVector(GridBoxSize / 2f, Direction);

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
            if (GameState.Grid.GetWallType(x, y) != WallType.Empty)
                return true;

        return false;
    }

    // TODO move to entity
    // Spin up entity in 
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

        var selectedDirection = GetDirectionFromActiveControls(activeMiningControls);
        Direction = selectedDirection ?? Direction;
        UpdateSpeed(selectedDirection, elapsedMs);
        UpdatePos(elapsedMs);
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
            if (floorType != FloorType.Empty) allCellsAreEmpty = false;
            if (floorType == FloorType.Lava) someCellsAreLava = true;
        }

        if (someCellsAreLava) TakeDamage((float)GameConfig.LavaDamagePerSecond / 1000 * elapsedMs);
        if (allCellsAreEmpty) IsOffAsteroid = true;
    }

    protected virtual void UpdateSpeed(Direction? selectedDirection, int elapsedGameTimeMs)
    {
        // If speed exceeds MaxSpeed, slowly ramp it down
        if (CurrentSpeed > MaxSpeed)
        {
            CurrentSpeed = Math.Max(MaxSpeed,
                CurrentSpeed - MaxSpeed * (elapsedGameTimeMs / (float)TimeToSlowToMax));
            return;
        }

        // Existing speed update logic
        if (!selectedDirection.HasValue && CurrentSpeed > 0)
            CurrentSpeed = Math.Max(0,
                CurrentSpeed - MaxSpeed * (elapsedGameTimeMs / (float)TimeToStopMs));

        if ((CurrentSpeed > 0 || selectedDirection.HasValue) && selectedDirection.HasValue)
            CurrentSpeed = Math.Min(MaxSpeed,
                CurrentSpeed + MaxSpeed * (elapsedGameTimeMs / (float)TimeToReachMaxSpeedMs));
    }

    private void UpdatePos(int elapsedGameTimeMs)
    {
        if (CurrentSpeed > 0)
        {
            var distance = CurrentSpeed * (elapsedGameTimeMs / 1000f);
            var movement = GetDirectionalVector(distance, Direction);

            var hasCollisions = !ApplyVectorToPosIfNoCollisions(movement);
            if (hasCollisions)
                CurrentSpeed = 0;
        }
    }

    // TODO move to util?
    public static Vector2 GetDirectionalVector(float distance, Direction direction)
    {
        return direction switch
        {
            Direction.Top => new Vector2(0, -distance),
            Direction.Right => new Vector2(distance, 0),
            Direction.Bottom => new Vector2(0, distance),
            Direction.Left => new Vector2(-distance, 0),
            _ => Vector2.Zero
        };
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