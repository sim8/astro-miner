using System;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public class UIMap : UIElement
{
    private const int MapScreenBaseXPadding = 60;
    private const int MapScreenBaseYPadding = 100;
    private const int CellBorderPx = 1;
    private const int playerMarkerSize = 52;
    private readonly BaseGame _game;

    public UIMap(BaseGame game) : base(game)
    {
        _game = game;
        FixedWidth = game.Graphics.GraphicsDevice.Viewport.Width -
                     MapScreenBaseXPadding * game.StateManager.Ui.UIScale;
        FixedHeight = game.Graphics.GraphicsDevice.Viewport.Height -
                      MapScreenBaseYPadding * game.StateManager.Ui.UIScale;
        BackgroundColor = Colors.VeryDarkBlue;
    }

    private Color GetCellColorForMap(int x, int y)
    {
        var cell = _game.StateManager.AsteroidWorld.Grid.GetCellState(x, y);
        if (cell.isEmpty) return Colors.VeryDarkBlue;

        if (cell.WallType == WallType.Empty && cell.DistanceToExploredFloor == 0)
            return cell.FloorType == FloorType.Lava ? Color.Orange : Colors.LightBlue;

        return Colors.DarkBlue;
    }

    private Rectangle GetGridCellRect(int col, int row, AsteroidBoundingBox boundingBox)
    {
        // Round down division to ensure fit
        var cellSizePxToFitWidth = ComputedWidth / boundingBox.Width;
        var cellSizePxToFitHeight = ComputedHeight / boundingBox.Height;
        // Prioritize shortest to ensure map fits on screen
        var cellSizePx = Math.Min(cellSizePxToFitWidth, cellSizePxToFitHeight);

        // Map from world coordinates to bounded coordinates
        var boundedCol = col - boundingBox.X;
        var boundedRow = row - boundingBox.Y;

        // Center the bounded grid within the available space
        var totalGridWidth = boundingBox.Width * cellSizePx;
        var totalGridHeight = boundingBox.Height * cellSizePx;
        var gridOffsetX = (ComputedWidth - totalGridWidth) / 2;
        var gridOffsetY = (ComputedHeight - totalGridHeight) / 2;

        var gridX = boundedCol * cellSizePx + CellBorderPx + gridOffsetX;
        var gridY = boundedRow * cellSizePx + CellBorderPx + gridOffsetY;
        var cellSizePxActual = cellSizePx - CellBorderPx;
        return new Rectangle(X + gridX, Y + gridY, cellSizePxActual, cellSizePxActual);
    }

    private void RenderPlayer(SpriteBatch spriteBatch, AsteroidBoundingBox boundingBox)
    {
        var playerGridPosVector = _game.StateManager.Ecs.ActiveControllableEntityCenterPosition;
        var (playerGridX, playerGridY) = ViewHelpers.ToGridPosition(playerGridPosVector);

        // Only render player if they're within the bounded area
        if (playerGridX < boundingBox.X || playerGridX > boundingBox.Right ||
            playerGridY < boundingBox.Y || playerGridY > boundingBox.Bottom)
            return;

        // Get the cell rectangle where the player is located
        var playerCellRect = GetGridCellRect(playerGridX, playerGridY, boundingBox);

        // Position player marker at the center of the cell
        var playerCenterX = playerCellRect.X + playerCellRect.Width / 2;
        var playerCenterY = playerCellRect.Y + playerCellRect.Height / 2;

        var playerDestRect = new Rectangle(playerCenterX - playerMarkerSize / 2, playerCenterY - playerMarkerSize / 2,
            playerMarkerSize, playerMarkerSize);
        spriteBatch.Draw(_game.Textures[Tx.RadialLight], playerDestRect, Color.Red);
    }


    public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);
        if (_game.Model.ActiveWorld != World.Asteroid)
            return;


        var boundingBox = AsteroidGridHelpers.GetAsteroidBoundingBoxWithinGrid(_game.Model.Asteroid.Grid);


        // Only iterate through the bounded area of the asteroid
        for (var row = boundingBox.Y; row <= boundingBox.Bottom; row++)
            for (var col = boundingBox.X; col <= boundingBox.Right; col++)
            {
                var cellState = _game.StateManager.AsteroidWorld.Grid.GetCellState(col, row);

                if (cellState.FloorType != FloorType.Empty)
                    spriteBatch.Draw(_game.Textures[Tx.White], GetGridCellRect(col, row, boundingBox),
                        GetCellColorForMap(col, row));
            }

        RenderPlayer(spriteBatch, boundingBox);
    }
}