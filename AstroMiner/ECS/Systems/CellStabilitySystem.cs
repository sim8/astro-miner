using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class CellStabilitySystem : System
{
    public CellStabilitySystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }


    public override void Update(GameTime gameTime, ActiveControls activeControls)
    {
        if (game.Model.ActiveWorld != World.Asteroid) return;

    }

    public void UpdateCellStabilityForMovement(PositionComponent position, float distanceMoved)
    {

        if (game.Model.ActiveWorld != World.Asteroid) return;

        if (!game.StateManager.AsteroidWorld.IsInMiner) return;

        var maxUpdateDistance = 2f;
        var damageMultiplier = 0.1f;

        var cellsInRadius = AsteroidGridHelpers.GetCellsInRadius(position.CenterPosition.X, position.CenterPosition.Y, maxUpdateDistance);
        foreach (var (x, y, cellDistance) in cellsInRadius)
        {
            var percentageOfDamageToApply = cellDistance / maxUpdateDistance;
            var damageToApply = percentageOfDamageToApply * distanceMoved * damageMultiplier;
            var cellState = game.StateManager.AsteroidWorld.Grid.GetCellState(x, y);
            cellState.Stability -= damageToApply;
            if (cellState.Stability < .2f)
            {
                game.StateManager.AsteroidWorld.Grid.ActivateCollapsingFloorCell(x, y);
            }
        }
    }
}