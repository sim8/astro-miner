using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class MiningControllableEntity(GameState gameState) : ControllableEntity(gameState)
{
    private const float DrillDistance = 0.2f;

    private readonly List<(int x, int y)> _drillingCells = new();

    private bool _drillingCellsMined;

    private int _drillingMs;

    private (int x, int y)? _drillingPos;
    private int _drillingTotalTimeRequired;

    protected virtual float DrillingWidth { get; } = 0f;
    protected virtual bool CanAddToInventory { get; } = true;

    public void Disembark()
    {
        _drillingMs = 0;
    }

    public override void Update(int elapsedMs, HashSet<MiningControls> activeMiningControls)
    {
        base.Update(elapsedMs, activeMiningControls);

        if (activeMiningControls.Contains(MiningControls.Drill))
            UseDrill(elapsedMs);
        else
            ResetDrill();
    }

    /// <summary>
    ///     Drills a row or column of cells. All cells in that row/column have their
    ///     drill times summed. Once <see cref="_drillingMs" /> meets/exceeds that sum,
    ///     they are all mined simultaneously.
    /// </summary>
    private void UseDrill(int elapsedGameTimeMs)
    {
        var drillPos = GetDrillPosition();
        var gridPos = ViewHelpers.ToGridPosition(drillPos);

        // If we're still drilling the same row/column as last time, just accumulate time.
        if (gridPos == _drillingPos)
        {
            _drillingMs += elapsedGameTimeMs;
        }
        else
        {
            // Starting to drill a new row/column
            _drillingPos = gridPos;
            _drillingMs = elapsedGameTimeMs;
            _drillingCellsMined = false;
            _drillingCells.Clear();
            _drillingTotalTimeRequired = 0;

            if (Direction is Direction.Top or Direction.Bottom)
            {
                var leftX = ViewHelpers.ToXorYCoordinate(drillPos.X - DrillingWidth / 2);
                var rightX = ViewHelpers.ToXorYCoordinate(drillPos.X + DrillingWidth / 2);
                for (var iX = leftX; iX <= rightX; iX++)
                {
                    if (!ViewHelpers.IsValidGridPosition(iX, gridPos.y))
                        continue;

                    _drillingCells.Add((iX, gridPos.y));

                    var cellTypeConfig = GameState.Grid.GetCellConfig(iX, gridPos.y);
                    if (cellTypeConfig is MineableCellConfig mineableConfig)
                        _drillingTotalTimeRequired += mineableConfig.DrillTimeMs;
                }
            }
            else // Direction.Left or Direction.Right
            {
                var topY = ViewHelpers.ToXorYCoordinate(drillPos.Y - DrillingWidth / 2);
                var bottomY = ViewHelpers.ToXorYCoordinate(drillPos.Y + DrillingWidth / 2);
                for (var iY = topY; iY <= bottomY; iY++)
                {
                    if (!ViewHelpers.IsValidGridPosition(gridPos.x, iY))
                        continue;

                    _drillingCells.Add((gridPos.x, iY));

                    var cellTypeConfig = GameState.Grid.GetCellConfig(gridPos.x, iY);
                    if (cellTypeConfig is MineableCellConfig mineableConfig)
                        _drillingTotalTimeRequired += mineableConfig.DrillTimeMs;
                }
            }
        }

        // Once we've drilled long enough to exceed the sum of all drill times, mine them all.
        if (!_drillingCellsMined && _drillingTotalTimeRequired > 0 && _drillingMs >= _drillingTotalTimeRequired)
        {
            foreach (var (x, y) in _drillingCells)
            {
                var cellTypeConfig = GameState.Grid.GetCellConfig(x, y);
                if (cellTypeConfig is MineableCellConfig)
                    GameState.Grid.MineCell(x, y, CanAddToInventory);
            }

            _drillingCellsMined = true;
        }
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
        _drillingCellsMined = false;
        _drillingCells.Clear();
        _drillingTotalTimeRequired = 0;
    }
}