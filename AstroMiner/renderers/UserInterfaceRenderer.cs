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