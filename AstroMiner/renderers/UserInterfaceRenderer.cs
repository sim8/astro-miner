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
            RenderString(spriteBatch, 0, 0, "UR DEAD");
        }
        else
        {
            var minutes = timeLeft / 60000;
            var seconds = timeLeft % 60000 / 1000;
            RenderString(spriteBatch, 0, 0, minutes.ToString("D2") + " " + seconds.ToString("D2"), 6);
        }

        RenderString(spriteBatch, 0, 80, "DIAMOND " + shared.GameState.Inventory.NumDiamonds);
        RenderString(spriteBatch, 0, 120, "RUBY " + shared.GameState.Inventory.NumRubies);

        RenderMinimap(spriteBatch);


        RenderString(spriteBatch, 0, 300, "FPS " + frameCounter.AverageFramesPerSecond.ToString("F0"));


        RenderString(spriteBatch, 0, 340, "SEED " + shared.GameState.Seed);
    }

    // TODO change back to private
    public void RenderString(SpriteBatch spriteBatch, int startX, int startY, string str, int scale = 3)
    {
        var linePxCount = 0;
        foreach (var (x, y, width) in FontHelpers.TransformString(str))
        {
            var sourceRect = new Rectangle(x, y, width, 8);
            var destRect = new Rectangle(startX + linePxCount * scale, startY + 10, width * scale, 8 * scale);
            spriteBatch.Draw(shared.Textures["dogica-font"], destRect, sourceRect, Color.LimeGreen);
            linePxCount += width;
        }
    }

    private void RenderMinimap(SpriteBatch spriteBatch)
    {
        var xOffset = 10;
        var yOffset = 180;
        var playerSize = 26;
        var asteroidLineThickness = 2;
        foreach (var gameStateEdgeCell in shared.GameState.EdgeCells)
        {
            var edgeCellDestRect = new Rectangle(xOffset + gameStateEdgeCell.x, yOffset + gameStateEdgeCell.y,
                asteroidLineThickness, asteroidLineThickness);
            spriteBatch.Draw(shared.Textures["white"], edgeCellDestRect, Color.White);
        }

        var playerGridPos = ViewHelpers.ToGridPosition(shared.GameState.ActiveControllableEntity.CenterPosition);
        var playerDestRect = new Rectangle(xOffset + playerGridPos.x - playerSize / 2,
            yOffset + playerGridPos.y - playerSize / 2, playerSize, playerSize);
        spriteBatch.Draw(shared.Textures["radial-light"], playerDestRect, Color.Red);
    }
}