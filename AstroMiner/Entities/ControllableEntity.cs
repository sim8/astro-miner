using System;
using System.Collections.Generic;
using System.Drawing;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class ControllableEntity(GameState gameState) : Entity
{
    // If speed exceeds max, gradually ramp back down
    private const float ExcessSpeedLossPerSecond = 3f;
    private const int LavaDamageDelayMs = 1000; // 1 second delay before taking damage

    private readonly List<MiningControls> _activeControlsOrder = new();

    private readonly Dictionary<MiningControls, Direction> _directionsControlsMapping = new()
    {
        { MiningControls.MoveUp, Direction.Top },
        { MiningControls.MoveRight, Direction.Right },
        { MiningControls.MoveDown, Direction.Bottom },
        { MiningControls.MoveLeft, Direction.Left }
    };

    protected readonly GameState GameState = gameState;

    private int _timeOnLavaMs;
    protected float CurrentSpeed;

    public float LavaTimePercentToTakingDamage => _timeOnLavaMs / (float)LavaDamageDelayMs;

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

    public Vector2 FrontPosition => CenterPosition + DirectionHelpers.GetDirectionalVector(GridBoxSize / 2f, Direction);

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

    private bool IsNewPositionIntersectingWithFilledCells(Vector2 position)
    {
        var topLeftCell = ViewHelpers.ToGridPosition(position);
        var bottomRightCell = ViewHelpers.ToGridPosition(position + new Vector2(GridBoxSize, GridBoxSize));

        for (var x = topLeftCell.x; x <= bottomRightCell.x; x++)
        for (var y = topLeftCell.y; y <= bottomRightCell.y; y++)
            if (GameState.ActiveWorld.CellIsCollideable(x, y))
                return true;
        return false;
    }

    private bool ApplyVectorToPosIfNoCollisions(Vector2 vector)
    {
        var newVector = Position + vector;
        var newPositionHasCollisions = IsNewPositionIntersectingWithFilledCells(newVector);

        if (newPositionHasCollisions) return false;

        var newRectangle = new RectangleF(newVector.X, newVector.Y, GridBoxSize, GridBoxSize);

        foreach (var entity in GameState.ActiveWorld.ActiveEntitiesSortedByDistance)
            if (entity != this && entity.CanCollide && newRectangle.IntersectsWith(entity.Rectangle) &&
                !Rectangle.IntersectsWith(entity.Rectangle)) // Allow movement if currently clipping
                return false;

        Position += vector;
        return true;
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeMiningControls)
    {
        CheckIfShouldFallOrTakeDamage(gameTime);

        if (IsAnimatingDamage)
        {
            // We need total for smooth animation, and timeSinceLastDamage to know when to stop showing
            TotalDamageAnimationTimeMs += gameTime.ElapsedGameTime.Milliseconds;
            TimeSinceLastDamage += gameTime.ElapsedGameTime.Milliseconds;
            if (TimeSinceLastDamage >= GameConfig.DamageAnimationTimeMs)
            {
                TotalDamageAnimationTimeMs = 0;
                TimeSinceLastDamage = 0;
                IsAnimatingDamage = false;
            }
        }

        var selectedDirection = GetDirectionFromActiveControls(activeMiningControls);

        // zero speed if turn 180
        if (selectedDirection.HasValue && selectedDirection.Value == DirectionHelpers.GetOppositeDirection(Direction))
            CurrentSpeed = 0f;
        Direction = selectedDirection ?? Direction;
        UpdateSpeed(selectedDirection, gameTime);
        UpdatePos(gameTime);
    }

    private void CheckIfShouldFallOrTakeDamage(GameTime gameTime)
    {
        // TODO remove
        if (!GameState.IsOnAsteroid) return;

        var (topLeftX, topLeftY) = ViewHelpers.ToGridPosition(Position);
        var (bottomRightX, bottomRightY) = ViewHelpers.ToGridPosition(Position + new Vector2(GridBoxSize, GridBoxSize));

        var allCellsAreEmpty = true;
        var someCellsAreLava = false;

        for (var x = topLeftX; x <= bottomRightX; x++)
        for (var y = topLeftY; y <= bottomRightY; y++)
        {
            var floorType = GameState.AsteroidWorld.Grid.GetFloorType(x, y);
            if (floorType != FloorType.Empty) allCellsAreEmpty = false;
            if (floorType == FloorType.Lava) someCellsAreLava = true;
        }

        if (someCellsAreLava)
        {
            _timeOnLavaMs += gameTime.ElapsedGameTime.Milliseconds;
            if (_timeOnLavaMs >= LavaDamageDelayMs)
                TakeDamage((float)GameConfig.LavaDamagePerSecond / 1000 * gameTime.ElapsedGameTime.Milliseconds);
        }
        else if (_timeOnLavaMs > 0)
        {
            _timeOnLavaMs = Math.Max(0, _timeOnLavaMs - gameTime.ElapsedGameTime.Milliseconds);
        }

        if (allCellsAreEmpty) IsOffAsteroid = true;
    }

    protected virtual void UpdateSpeed(Direction? selectedDirection, GameTime gameTime)
    {
        if (CurrentSpeed > MaxSpeed)
        {
            CurrentSpeed = Math.Max(MaxSpeed,
                CurrentSpeed - ExcessSpeedLossPerSecond * (gameTime.ElapsedGameTime.Milliseconds / 1000f));
            return;
        }

        // Existing speed update logic
        if (!selectedDirection.HasValue && CurrentSpeed > 0)
            CurrentSpeed = Math.Max(0,
                CurrentSpeed - MaxSpeed * (gameTime.ElapsedGameTime.Milliseconds / (float)TimeToStopMs));

        if ((CurrentSpeed > 0 || selectedDirection.HasValue) && selectedDirection.HasValue)
            CurrentSpeed = Math.Min(MaxSpeed,
                CurrentSpeed + MaxSpeed * (gameTime.ElapsedGameTime.Milliseconds / (float)TimeToReachMaxSpeedMs));
    }

    private void UpdatePos(GameTime gameTime)
    {
        if (CurrentSpeed > 0)
        {
            var distance = CurrentSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
            var movement = DirectionHelpers.GetDirectionalVector(distance, Direction);

            var hasCollisions = !ApplyVectorToPosIfNoCollisions(movement);
            if (hasCollisions)
                CurrentSpeed = 0;
        }
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