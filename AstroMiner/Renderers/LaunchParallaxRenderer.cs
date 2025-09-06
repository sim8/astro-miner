using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class LaunchParallaxRenderer(RendererShared shared)
{
    private const int BackgroundWidth = 25;

    // Center camera within background to prevent left/right shift while changing parallax
    private static readonly float
        BackgroundX = Coordinates.Grid.MinerHomeStartPosCenter.x - (float)BackgroundWidth / 2;


    private void RenderLayer(SpriteBatch spriteBatch, float gridY, float parallaxLayer, float opacity)
    {
        spriteBatch.Draw(shared.Textures[Tx.MountainsNice],
            shared.ViewHelpers.GetVisibleRectForGridCell(BackgroundX, gridY, 25,
                20, parallaxLayer), Color.White);
        spriteBatch.Draw(shared.Textures[Tx.MountainsNiceMask],
            shared.ViewHelpers.GetVisibleRectForGridCell(BackgroundX, gridY, 25,
                20, parallaxLayer), Color.White * opacity);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        RenderLayer(spriteBatch, Coordinates.Grid.ParallaxMountains3Y, 0.1f, 0.85f);
        RenderLayer(spriteBatch, Coordinates.Grid.ParallaxMountains2Y, 0.15f, 0.6f);
        RenderLayer(spriteBatch, Coordinates.Grid.ParallaxMountains1Y, 0.2f, 0.3f);

        spriteBatch.Draw(shared.Textures[Tx.OizusRocksUnder],
            shared.ViewHelpers.GetVisibleRectForGridCell(Coordinates.Grid.UnderRocksX, Coordinates.Grid.UnderRocksY, 23,
                20, 0.6f), Color.White);
    }
}