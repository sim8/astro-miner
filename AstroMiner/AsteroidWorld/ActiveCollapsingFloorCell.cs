using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.AsteroidWorld;

public class ActiveCollapsingFloorCell(BaseGame game, (int x, int y) gridPos)
{
    public int TimeToCollapse = GameConfig.CollapsingFloorSpreadTime;

    public void Update(GameTime gameTime)
    {
        TimeToCollapse -= gameTime.ElapsedGameTime.Milliseconds;
        if (TimeToCollapse <= 0)
        {
            GridState.Map4Neighbors(gridPos.x, gridPos.y,
                (nx, ny) => { game.State.AsteroidWorld.Grid.ActivateCollapsingFloorCell(nx, ny); });

            game.State.AsteroidWorld.Grid.GetCellState(gridPos.x, gridPos.y).FloorType = FloorType.Lava;

            game.State.AsteroidWorld.Grid.DeactiveCollapsingFloorCell(gridPos.x, gridPos.y);
        }
    }
}