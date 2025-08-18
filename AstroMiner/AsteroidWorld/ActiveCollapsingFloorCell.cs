using System;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.AsteroidWorld;

[Serializable]
public class ActiveCollapsingFloorCell
{
    private BaseGame _game;
    private (int x, int y) _gridPos;
    public int TimeToCollapse = GameConfig.CollapsingFloorSpreadTime;

    public ActiveCollapsingFloorCell(BaseGame game, (int x, int y) gridPos)
    {
        _game = game;
        _gridPos = gridPos;
        _game.StateManager.AsteroidWorld.Grid.GetCellState(_gridPos.x, _gridPos.y).FloorType =
            FloorType.CollapsingLavaCracks;
    }

    public float PercentageComplete => 1 - TimeToCollapse / (float)GameConfig.CollapsingFloorSpreadTime;

    public void Update(GameTime gameTime)
    {
        TimeToCollapse -= gameTime.ElapsedGameTime.Milliseconds;
        if (TimeToCollapse <= 0)
        {
            GridState.Map4Neighbors(_gridPos.x, _gridPos.y,
                (nx, ny) => { _game.StateManager.AsteroidWorld.Grid.ActivateCollapsingFloorCell(nx, ny); });

            _game.StateManager.AsteroidWorld.Grid.GetCellState(_gridPos.x, _gridPos.y).FloorType = FloorType.Lava;

            _game.StateManager.AsteroidWorld.Grid.DeactiveCollapsingFloorCell(_gridPos.x, _gridPos.y);
        }
    }
}