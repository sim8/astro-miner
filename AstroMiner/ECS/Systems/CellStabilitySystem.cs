using System;
using System.Linq;
using AstroMiner.AsteroidWorld;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Model;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class CellStabilitySystem(Ecs ecs, BaseGame game) : System(ecs, game)
{
    private const float MovementStabilityRadius = 3f;
    private const float MiningStabilityRadius = 2f;
    private const float MiningDamagePerSecond = 0.02f;
    private const float DamageMultiplier = 0.1f;

    public const float CriticalStabilityThreshold = 0.5f; // Could be different thresholds for collapsing/explosive


    public override void Update(GameTime gameTime, ActiveControls activeControls)
    {
        if (game.Model.ActiveWorld != World.Asteroid) return;

        foreach (var (x, y) in game.Model.Asteroid.CriticalStabilityCells.ToList())
        {
            // Reduce stability of critical cells at constant rate
            var cellState = game.StateManager.AsteroidWorld.Grid.GetCellState(x, y);
            var prevStability = cellState.Stability;

            cellState.Stability = Math.Max(0,
                cellState.Stability -
                gameTime.ElapsedGameTime.Milliseconds / (float)GameConfig.CollapsingFloorSpreadTime);

            if (prevStability > 0 && cellState.Stability == 0)
            {
                HandleStabilityZero(x, y);
                game.Model.Asteroid.CriticalStabilityCells.Remove((x, y));
            }
        }
    }

    private void HandleStabilityZero(int x, int y)
    {
        var cellState = game.StateManager.AsteroidWorld.Grid.GetCellState(x, y);
        if (cellState.WallType == WallType.ExplosiveRock)
        {
            var explosionPos = new Vector2(x + 0.5f, y + 0.5f);
            game.StateManager.Ecs.Factories.CreateExplosionEntity(explosionPos);
        }
        else if (cellState.WallType == WallType.Empty && (cellState.FloorType == FloorType.LavaCracks || cellState.FloorType == FloorType.CollapsingLavaCracks))
        {
            cellState.FloorType = FloorType.Lava;
            GridState.Map4Neighbors(x, y, (nx, ny) => { UpdateCellStability(nx, ny, stability => stability - 0.1f); });
        }

    }

    private static bool CellCanDeteriorate(CellState cellState)
    {
        if (cellState.WallType == WallType.ExplosiveRock) return true;
        if (cellState.WallType != WallType.Empty) return false;

        return cellState.FloorType is FloorType.LavaCracks or FloorType.CollapsingLavaCracks;
    }


    /***
     * Updates cell stability via callback func. Cell will become critical if new stability is below threshold.
     * Is a noop if cell is not valid type to have modified stability or if cell is already critical.
     */
    public void UpdateCellStability(int x, int y, Func<float, float> stabilityFunc)
    {
        var cellState = game.StateManager.AsteroidWorld.Grid.GetCellState(x, y);

        if (!CellCanDeteriorate(cellState) || game.Model.Asteroid.CriticalStabilityCells.Contains((x, y)) ||
            cellState.Stability <= CriticalStabilityThreshold)
            return;

        cellState.Stability = Math.Max(CriticalStabilityThreshold, stabilityFunc(cellState.Stability));

        if (cellState.Stability == CriticalStabilityThreshold)
        {
            // TODO do we still need FloorType.CollapsingLavaCracks?
            if (cellState.FloorType == FloorType.LavaCracks) cellState.FloorType = FloorType.CollapsingLavaCracks;
            game.Model.Asteroid.CriticalStabilityCells.Add((x, y));
        }
    }

    public void UpdateCellStabilityForMining(Vector2 drillPos, int drillingMs)
    {
        UpdateCellStabilityInRadius(drillPos, (float)drillingMs / 1000f * MiningDamagePerSecond, MiningStabilityRadius);
    }

    public void UpdateCellStabilityForMovement(PositionComponent position, float distanceMoved)
    {
        if (game.Model.ActiveWorld != World.Asteroid) return;

        if (!game.StateManager.AsteroidWorld.IsInMiner) return;

        UpdateCellStabilityInRadius(position.CenterPosition, distanceMoved * DamageMultiplier, MovementStabilityRadius);

    }

    private void UpdateCellStabilityInRadius(Vector2 position, float maxDamage, float radius)
    {
        var cellsInRadius =
            AsteroidGridHelpers.GetCellsInRadius(position.X, position.Y, radius);
        foreach (var (x, y, cellDistance) in cellsInRadius)
        {
            var distanceModifier = cellDistance / radius;
            var damageToApply = maxDamage * distanceModifier;
            UpdateCellStability(x, y, stability => stability - damageToApply);
        }
    }
}