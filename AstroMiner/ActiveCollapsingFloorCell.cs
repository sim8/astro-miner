using AstroMiner.Definitions;

namespace AstroMiner;

public class ActiveCollapsingFloorCell(GameState gameState, (int x, int y) gridPos)
{
    public int TimeToCollapse = GameConfig.CollapsingFloorSpreadTime;

    public void Update(int elapsedMs)
    {
        TimeToCollapse -= elapsedMs;
        if (TimeToCollapse <= 0)
        {
            GridState.Map4Neighbors(gridPos.x, gridPos.y,
                (nx, ny) => { gameState.Grid.ActivateCollapsingFloorCell(nx, ny); });

            gameState.Grid.GetCellState(gridPos.x, gridPos.y).FloorType = FloorType.Lava;

            gameState.Grid.DeactiveCollapsingFloorCell(gridPos.x, gridPos.y);
        }
    }
}