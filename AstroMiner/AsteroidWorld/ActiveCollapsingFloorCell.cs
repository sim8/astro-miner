using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.AsteroidWorld;

public class ActiveCollapsingFloorCell(GameState gameState, (int x, int y) gridPos)
{
    public int TimeToCollapse = GameConfig.CollapsingFloorSpreadTime;

    public void Update(GameTime gameTime)
    {
        TimeToCollapse -= gameTime.ElapsedGameTime.Milliseconds;
        if (TimeToCollapse <= 0)
        {
            GridState.Map4Neighbors(gridPos.x, gridPos.y,
                (nx, ny) => { gameState.AsteroidWorld.Grid.ActivateCollapsingFloorCell(nx, ny); });

            gameState.AsteroidWorld.Grid.GetCellState(gridPos.x, gridPos.y).FloorType = FloorType.Lava;

            gameState.AsteroidWorld.Grid.DeactiveCollapsingFloorCell(gridPos.x, gridPos.y);
        }
    }
}