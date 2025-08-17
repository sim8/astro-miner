using System;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public class UIMap : UIElement
{
    private const int MapScreenBaseXPadding = 60;
    private const int MapScreenBaseYPadding = 100;
    private const int CellBorderPx = 1;
    private readonly BaseGame _game;

    public UIMap(BaseGame game) : base(game)
    {
        _game = game;
        FixedWidth = game.Graphics.GraphicsDevice.Viewport.Width -
                     MapScreenBaseXPadding * game.StateManager.Ui.UIScale;
        FixedHeight = game.Graphics.GraphicsDevice.Viewport.Height -
                      MapScreenBaseYPadding * game.StateManager.Ui.UIScale;
        BackgroundColor = Colors.VeryDarkBlue;
        Children =
        [
            // Wrapper
            new UITextElement(game)
            {
                Text = "MAP GOES HERE"
            }
        ];
    }

    private Color GetCellColorForMap(int x, int y)
    {
        var cell = _game.StateManager.AsteroidWorld.Grid.GetCellState(x, y);
        if (cell.isEmpty) return Colors.VeryDarkBlue;

        if (cell.WallType == WallType.Empty && cell.DistanceToExploredFloor == 0)
            return cell.FloorType == FloorType.Lava ? Color.Orange : Colors.LightBlue;

        return Colors.DarkBlue;
    }

    private Rectangle GetGridCellRect(int col, int row)
    {
        var gridSizePx = Math.Min(ComputedWidth, ComputedHeight);
        var cellSizePx = gridSizePx / GameConfig.GridSize; // Purposefully round down
        var gridX = col * cellSizePx + CellBorderPx;
        var gridY = row * cellSizePx + CellBorderPx;
        var cellSizePxActual = cellSizePx - CellBorderPx;
        return new Rectangle(X + gridX, Y + gridY, cellSizePxActual, cellSizePxActual);
    }


    public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);
        if (_game.Model.ActiveWorld != World.Asteroid)
            return;


        for (var row = 0; row < GameConfig.GridSize; row++)
        for (var col = 0; col < GameConfig.GridSize; col++)
        {
            var cellState = _game.StateManager.AsteroidWorld.Grid.GetCellState(col, row);

            if (cellState.FloorType != FloorType.Empty)
                spriteBatch.Draw(_game.Textures["white"], GetGridCellRect(col, row),
                    GetCellColorForMap(col, row));
        }
    }
}