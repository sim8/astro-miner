using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class UserInterfaceRenderer(
    RendererShared shared)
{
    public void RenderUserInterface(SpriteBatch spriteBatch, FrameCounter frameCounter)
    {
        var timeLeft = shared.GameState.TimeUntilAsteroidExplodesMs;
        if (timeLeft < 0)
        {
            shared.RenderString(spriteBatch, 0, 0, "UR DEAD");
        }
        else
        {
            var minutes = timeLeft / 60000;
            var seconds = timeLeft % 60000 / 1000;
            shared.RenderString(spriteBatch, 0, 0, minutes.ToString("D2") + " " + seconds.ToString("D2"), 6);
        }

        shared.RenderString(spriteBatch, 0, 80, "DIAMOND " + shared.GameState.Inventory.NumDiamonds);
        shared.RenderString(spriteBatch, 0, 120, "RUBY " + shared.GameState.Inventory.NumRubies);

        RenderMinimap(spriteBatch);


        shared.RenderString(spriteBatch, 0, 300, "FPS " + frameCounter.AverageFramesPerSecond.ToString("F0"));


        shared.RenderString(spriteBatch, 0, 340, "SEED " + shared.GameState.Seed);
    }

    private void RenderMinimap(SpriteBatch spriteBatch)
    {
        var xOffset = 10;
        var yOffset = 180;
        var playerSize = 26;
        var scale = 0.5f;
        var asteroidLineThickness = 1;

        var dividingLinesThickness = 1;
        var dividingLines = 4;

        var borderThickness = 2;

        var minimapGridSizePx = (int)(GameConfig.GridSize * scale);
        var minimapTotalSizePx = minimapGridSizePx + borderThickness * 2;

        var gridColor = new Color(70, 125, 149);

        // Top border
        spriteBatch.Draw(shared.Textures["white"], new Rectangle(xOffset, yOffset, minimapTotalSizePx, borderThickness),
            gridColor);
        // Bottom border
        spriteBatch.Draw(shared.Textures["white"],
            new Rectangle(xOffset, yOffset + minimapGridSizePx + borderThickness, minimapTotalSizePx, borderThickness),
            gridColor);
        // Left border
        spriteBatch.Draw(shared.Textures["white"],
            new Rectangle(xOffset, yOffset, borderThickness, minimapTotalSizePx),
            gridColor);
        // Right border
        spriteBatch.Draw(shared.Textures["white"],
            new Rectangle(xOffset + minimapGridSizePx + borderThickness, yOffset, borderThickness, minimapTotalSizePx),
            gridColor);

        // Draw dividing lines
        var cellWidth = minimapGridSizePx / (dividingLines + 1);
        var cellHeight = minimapGridSizePx / (dividingLines + 1);

        // Vertical dividing lines
        for (var i = 1; i <= dividingLines; i++)
        {
            var x = xOffset + borderThickness + cellWidth * i;
            spriteBatch.Draw(shared.Textures["white"],
                new Rectangle(x, yOffset, dividingLinesThickness, minimapTotalSizePx),
                gridColor);
        }

        // Horizontal dividing lines
        for (var i = 1; i <= dividingLines; i++)
        {
            var y = yOffset + borderThickness + cellHeight * i;
            spriteBatch.Draw(shared.Textures["white"],
                new Rectangle(xOffset, y, minimapTotalSizePx, dividingLinesThickness),
                gridColor);
        }

        foreach (var gameStateEdgeCell in shared.GameState.EdgeCells)
        {
            var x = xOffset + gameStateEdgeCell.x * scale;
            var y = yOffset + gameStateEdgeCell.y * scale;
            var edgeCellDestRect = new Rectangle((int)x, (int)y,
                asteroidLineThickness, asteroidLineThickness);
            spriteBatch.Draw(shared.Textures["white"], edgeCellDestRect, Color.White);
        }

        var playerGridPos = ViewHelpers.ToGridPosition(shared.GameState.ActiveControllableEntity.CenterPosition);
        var playerX = xOffset + playerGridPos.x * scale - playerSize / 2;
        var playerY = yOffset + playerGridPos.y * scale - playerSize / 2;
        var playerDestRect = new Rectangle((int)playerX, (int)playerY, playerSize, playerSize);
        spriteBatch.Draw(shared.Textures["radial-light"], playerDestRect, Color.Red);
    }
}