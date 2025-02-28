using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class LaunchParallaxRenderer(RendererShared shared)
{
    public void Render(SpriteBatch spriteBatch)
    {
        var parallax = 0.5f;
        spriteBatch.Draw(shared.Textures["launch-background"],
            shared.ViewHelpers.GetVisibleRectForGridCell(-15, GameConfig.HomeToAsteroidPointY, 25, 125, parallax),
            Color.White);

        // spriteBatch.Draw(shared.Textures["launch-background-repeating"],
        //     shared.ViewHelpers.GetVisibleRectForGridCell(-7, GameConfig.HomeToAsteroidPointY - 25, 25, 25, parallax),
        //     Color.White);
    }
}