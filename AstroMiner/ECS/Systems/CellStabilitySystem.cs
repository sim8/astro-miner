using System;
using AstroMiner.AsteroidWorld;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class CellStabilitySystem : System
{
    public const float MaxUpdateDistance = 2f;
    public const float DamageMultiplier = 0.1f;

    public const float CriticalStabilityThreshold = 0.2f; // Could be different thresholds for collapsing/explosive


    public CellStabilitySystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }


    public override void Update(GameTime gameTime, ActiveControls activeControls)
    {
        if (game.Model.ActiveWorld != World.Asteroid) return;

        foreach (var (x, y) in game.Model.Asteroid.CriticalStabilityCells)
        {
            var newStability = UpdateCellStability(x, y, (stability) => stability - gameTime.ElapsedGameTime.Milliseconds / GameConfig.CollapsingFloorSpreadTime);
            if (newStability == 0)
            {
                game.Model.Asteroid.CriticalStabilityCells.Remove((x, y));
            }
        }

        // TODO - modify above to have same burndown as collapsing
        // TODO - add trigger neighbours logic (-= .1?)
        // TODO - update animation to read percentages
        // TODO - move onto explosives

    }

    public float UpdateCellStability(int x, int y, Func<float, float> stabilityFunc)
    {
        var cellState = game.StateManager.AsteroidWorld.Grid.GetCellState(x, y);
        var prevStability = cellState.Stability;
        var newStability = stabilityFunc(cellState.Stability);
        cellState.Stability = Math.Max(0, newStability);

        if (prevStability == cellState.Stability)
        {
            return cellState.Stability;
        }

        if (cellState.Stability < CriticalStabilityThreshold && !game.Model.Asteroid.CriticalStabilityCells.Contains((x, y)))
        {
            // TODO do we still need FloorType.CollapsingLavaCracks?
            if (cellState.FloorType == FloorType.LavaCracks)
            {
                cellState.FloorType = FloorType.CollapsingLavaCracks;
            }
            game.Model.Asteroid.CriticalStabilityCells.Add((x, y));
        }
        if (cellState.Stability == 0)
        {
            GridState.Map4Neighbors(x, y,
                (nx, ny) => { UpdateCellStability(nx, ny, (stability) => stability - 0.1f); });
        }

        return cellState.Stability;
    }

    public void UpdateCellStabilityForMovement(PositionComponent position, float distanceMoved)
    {

        if (game.Model.ActiveWorld != World.Asteroid) return;

        if (!game.StateManager.AsteroidWorld.IsInMiner) return;


        var cellsInRadius = AsteroidGridHelpers.GetCellsInRadius(position.CenterPosition.X, position.CenterPosition.Y, MaxUpdateDistance);
        foreach (var (x, y, cellDistance) in cellsInRadius)
        {
            var percentageOfDamageToApply = cellDistance / MaxUpdateDistance;
            var damageToApply = percentageOfDamageToApply * distanceMoved * DamageMultiplier;
            UpdateCellStability(x, y, (stability) => stability - damageToApply);
        }
    }
}