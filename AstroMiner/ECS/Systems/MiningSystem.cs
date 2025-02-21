using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class MiningSystem : System
{
    public MiningSystem(Ecs ecs, GameState gameState) : base(ecs, gameState)
    {
    }

    private const float DrillDistance = 0.2f;

    // public virtual void Disembark()
    // {
    //     _drillingMs = 0;
    // }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeMiningControls)
    {
        var miningComponent = Ecs.GetComponent<MiningComponent>(Ecs.ActiveControllableEntityId.Value);

        if (miningComponent == null)
        {
            return;
        }

        if (activeMiningControls.Contains(MiningControls.Drill))
            UseDrill(gameTime, miningComponent);
        else
            ResetDrill(miningComponent);
    }

    private void UseDrillOnCell(int x, int y, MiningComponent miningComponent)
    {
        if (!ViewHelpers.IsValidGridPosition(x, y))
            return;

        var wallType = GameState.AsteroidWorld.Grid.GetWallType(x, y);

        if (wallType == WallType.ExplosiveRock)
        {
            GameState.AsteroidWorld.Grid.ActivateExplosiveRockCell(x, y, 100);
            return; // No point adding to drilling time
        }

        var wallTypeConfig = GameState.AsteroidWorld.Grid.GetWallTypeConfig(x, y);
        if (wallTypeConfig is { IsMineable: true })
            miningComponent.DrillingTotalTimeRequired += wallTypeConfig.DrillTimeMs;
        miningComponent.DrillingCells.Add((x, y));
    }

    /// <summary>
    ///     Drills a row or column of cells. All cells in that row/column have their
    ///     drill times summed. Once <see cref="_drillingMs" /> meets/exceeds that sum,
    ///     they are all mined simultaneously.
    /// </summary>
    private void UseDrill(GameTime gameTime, MiningComponent miningComponent)
    {
        var drillPos = GetDrillPosition();
        var gridPos = ViewHelpers.ToGridPosition(drillPos);

        var directionComponent = Ecs.GetComponent<DirectionComponent>(Ecs.ActiveControllableEntityId.Value);

        // If we're still drilling the same row/column as last time, just accumulate time.
        if (gridPos == miningComponent.DrillingPos)
        {
            miningComponent.DrillingMs += gameTime.ElapsedGameTime.Milliseconds;
        }
        else
        {
            // Starting to drill a new row/column
            miningComponent.DrillingPos = gridPos;
            miningComponent.DrillingMs = gameTime.ElapsedGameTime.Milliseconds;
            miningComponent.DrillingCellsMined = false;
            miningComponent.DrillingCells.Clear();
            miningComponent.DrillingTotalTimeRequired = 0;

            if (directionComponent.Direction is Direction.Top or Direction.Bottom)
            {
                var leftX = ViewHelpers.ToXorYCoordinate(drillPos.X - miningComponent.DrillingWidth / 2);
                var rightX = ViewHelpers.ToXorYCoordinate(drillPos.X + miningComponent.DrillingWidth / 2);
                for (var iX = leftX; iX <= rightX; iX++) UseDrillOnCell(iX, gridPos.y, miningComponent);
            }
            else // Direction.Left or Direction.Right
            {
                var topY = ViewHelpers.ToXorYCoordinate(drillPos.Y - miningComponent.DrillingWidth / 2);
                var bottomY = ViewHelpers.ToXorYCoordinate(drillPos.Y + miningComponent.DrillingWidth / 2);
                for (var iY = topY; iY <= bottomY; iY++) UseDrillOnCell(gridPos.x, iY, miningComponent);
            }
        }

        // Once we've drilled long enough to exceed the sum of all drill times, mine them all.
        if (!miningComponent.DrillingCellsMined && miningComponent.DrillingTotalTimeRequired > 0 && miningComponent.DrillingMs >= miningComponent.DrillingTotalTimeRequired)
        {
            foreach (var (x, y) in miningComponent.DrillingCells)
            {
                var wallTypeConfig = GameState.AsteroidWorld.Grid.GetWallTypeConfig(x, y);
                if (wallTypeConfig is { IsMineable: true })
                    GameState.AsteroidWorld.Grid.MineWall(x, y, miningComponent.CanAddToInventory);
            }

            miningComponent.DrillingCellsMined = true;
        }
    }

    private Vector2 GetDrillPosition()
    {
        var positionComponent = Ecs.GetComponent<PositionComponent>(Ecs.ActiveControllableEntityId.Value);
        var directionComponent = Ecs.GetComponent<DirectionComponent>(Ecs.ActiveControllableEntityId.Value);

        var drillDistanceFromCenter = positionComponent.GridBoxSize / 2 + DrillDistance;
        return positionComponent.CenterPosition + DirectionHelpers.GetDirectionalVector(drillDistanceFromCenter, directionComponent.Direction);
    }

    private void ResetDrill(MiningComponent miningComponent)
    {
        miningComponent.DrillingPos = null;
        miningComponent.DrillingMs = 0;
        miningComponent.DrillingCellsMined = false;
        miningComponent.DrillingCells.Clear();
        miningComponent.DrillingTotalTimeRequired = 0;
    }
}