using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class UserInterfaceRenderer(
    Dictionary<string, Texture2D> textures,
    MiningState miningState,
    ViewHelpers viewHelpers)
{
    public void RenderUserInterface(SpriteBatch spriteBatch)
    {
        RenderString(spriteBatch, "HEY THERE 0123456789 NICE 12");
    }

    private void RenderString(SpriteBatch spriteBatch, string str)
    {
        var linePxCount = 0;
        var scale = 6;
        foreach (var (x, y, width) in FontHelpers.TransformString(str))
        {
            var sourceRect = new Rectangle(x, y, width, 8);
            var destRect = new Rectangle(linePxCount * scale, 10, width * scale, 8 * scale);
            spriteBatch.Draw(textures["dogica-font"], destRect, sourceRect, Color.Green);
            linePxCount += width;
        }
    }
}