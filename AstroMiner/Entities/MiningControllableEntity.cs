using System;
using System.Collections.Generic;
using System.Drawing;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class MiningControllableEntity : Entity
{
    private const float DrillDistance = 0.2f;

    private readonly Dictionary<MiningControls, Direction> _directionsControlsMapping = new()
    {
        { MiningControls.MoveUp, Direction.Top },
        { MiningControls.MoveRight, Direction.Right },
        { MiningControls.MoveDown, Direction.Bottom },
        { MiningControls.MoveLeft, Direction.Left }
    };

    private readonly GameState _gameState;
    private float _currentSpeed;

    private int _drillingMs;

    // TODO is this needed?
    private (int x, int y)? _drillingPos;

    public MiningControllableEntity(GameState gameState)
    {
        _gameState = gameState;
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
    protected virtual float DrillingWidth { get; } = 0f;
    protected virtual bool CanAddToInventory { get; } = true;


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

    public void Disembark()
    {
        _drillingMs = 0;
    }

    private bool IsNewPositionIntersectingWithFilledCells(Vector2 position)
    {
        var topLeftCell = ViewHelpers.ToGridPosition(position);
        var bottomRightCell = ViewHelpers.ToGridPosition(position + new Vector2(GridBoxSize, GridBoxSize));

        for (var x = topLeftCell.x; x <= bottomRightCell.x; x++)
        for (var y = topLeftCell.y; y <= bottomRightCell.y; y++)
            if (_gameState.Grid.GetCellConfig(x, y).IsCollideable)
                return true;

        return false;
    }

    private bool ApplyVectorToPosIfNoCollisions(Vector2 vector)
    {
        var newVector = Position + vector;
        var newPositionHasCollisions = IsNewPositionIntersectingWithFilledCells(newVector);

        if (newPositionHasCollisions) return false;

        var newRectangle = new RectangleF(newVector.X, newVector.Y, GridBoxSize, GridBoxSize);

        foreach (var entity in _gameState.ActiveEntitiesSortedByDistance)
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

        if (activeMiningControls.Contains(MiningControls.Drill))
            UseDrill(elapsedMs);
        else
            ResetDrill();
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
            var cellType = _gameState.Grid.GetCellType(x, y);
            if (cellType != CellType.Empty) allCellsAreEmpty = false;
            if (cellType == CellType.Lava) someCellsAreLava = true;
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

    private void UseDrill(int elapsedGameTimeMs)
    {
        var drillPos = GetDrillPosition();
        var gridPos = ViewHelpers.ToGridPosition(drillPos);

        if (gridPos == _drillingPos)
        {
            _drillingMs += elapsedGameTimeMs;
        }
        else
        {
            _drillingPos = gridPos;
            _drillingMs = elapsedGameTimeMs;
        }

        if (Direction is Direction.Top or Direction.Bottom)
        {
            var leftX = ViewHelpers.ToXorYCoordinate(drillPos.X - DrillingWidth / 2);
            var rightX = ViewHelpers.ToXorYCoordinate(drillPos.X + DrillingWidth / 2);
            for (var iX = leftX; iX <= rightX; iX++)
            {
                var (nice, y) = gridPos;
                ApplyDrillToCell(iX, y);
            }
        }
        else // left or right
        {
            var topY = ViewHelpers.ToXorYCoordinate(drillPos.Y - DrillingWidth / 2);
            var bottomY = ViewHelpers.ToXorYCoordinate(drillPos.Y + DrillingWidth / 2);
            for (var iY = topY; iY <= bottomY; iY++)
            {
                var (x, nice) = gridPos;
                ApplyDrillToCell(x, iY);
            }
        }
    }

    private void ApplyDrillToCell(int x, int y)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            return;

        var cellTypeConfig = _gameState.Grid.GetCellConfig(x, y);

        if (cellTypeConfig is MineableCellConfig mineableConfig && _drillingMs > mineableConfig.DrillTimeMs)
            _gameState.Grid.MineCell(x, y, CanAddToInventory);
    }

    private Vector2 GetDrillPosition()
    {
        var drillDistanceFromCenter = GridBoxSize / 2 + DrillDistance;
        return Direction switch
        {
            Direction.Top => CenterPosition + new Vector2(0, -drillDistanceFromCenter),
            Direction.Right => CenterPosition + new Vector2(drillDistanceFromCenter, 0),
            Direction.Bottom => CenterPosition + new Vector2(0, drillDistanceFromCenter),
            Direction.Left => CenterPosition + new Vector2(-drillDistanceFromCenter, 0),
            _ => Position
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

    private void ResetDrill()
    {
        _drillingPos = null;
        _drillingMs = 0;
    }
}