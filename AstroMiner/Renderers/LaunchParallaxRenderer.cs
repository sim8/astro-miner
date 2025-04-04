using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class LaunchParallaxRenderer(RendererShared shared)
{
    private const int BackgroundWidth = 25;

    // Center camera within background to prevent left/right shift while changing parallax
    private static readonly float
        BackgroundX = GameConfig.Launch.MinerHomeStartPosCenter.x - (float)BackgroundWidth / 2;


    private void RenderLayer(SpriteBatch spriteBatch, float gridY, float parallaxLayer, float opacity)
    {
        spriteBatch.Draw(shared.Textures["mountains-nice"],
            shared.ViewHelpers.GetVisibleRectForGridCell(BackgroundX, gridY, 25,
                15, parallaxLayer), Color.White);
        spriteBatch.Draw(shared.Textures["mountains-nice-mask"],
            shared.ViewHelpers.GetVisibleRectForGridCell(BackgroundX, gridY, 25,
                15, parallaxLayer), Color.White * opacity);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        RenderLayer(spriteBatch, -100, 0.1f, 0.85f);
        RenderLayer(spriteBatch, -50, 0.15f, 0.6f);
        RenderLayer(spriteBatch, -0, 0.2f, 0.3f);
    }
}