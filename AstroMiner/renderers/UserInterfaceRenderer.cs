using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class UserInterfaceRenderer(
    Dictionary<string, Texture2D> textures,
    GameState gameState,
    ViewHelpers viewHelpers)
{
    public void RenderUserInterface(SpriteBatch spriteBatch)
    {
        var timeLeft = gameState.TimeUntilAsteroidExplodesMs;
        if (timeLeft < 0)
        {
            RenderString(spriteBatch, 0, 0, "UR DEAD");
        }
        else
        {
            var minutes = timeLeft / 60000;
            var seconds = timeLeft % 60000 / 1000;
            RenderString(spriteBatch, 0, 0, minutes.ToString("D2") + " " + seconds.ToString("D2"));
        }

        RenderString(spriteBatch, 0, 100, "DIAMONDS " + gameState.Inventory.NumDiamonds);
        RenderString(spriteBatch, 0, 160, "RUBIES " + gameState.Inventory.NumRubies);
    }

    private void RenderString(SpriteBatch spriteBatch, int startX, int startY, string str)
    {
        var linePxCount = 0;
        var scale = 6;
        foreach (var (x, y, width) in FontHelpers.TransformString(str))
        {
            var sourceRect = new Rectangle(x, y, width, 8);
            var destRect = new Rectangle(startX + linePxCount * scale, startY + 10, width * scale, 8 * scale);
            spriteBatch.Draw(textures["dogica-font"], destRect, sourceRect, Color.Green);
            linePxCount += width;
        }
    }
}