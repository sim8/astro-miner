using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class LaunchParallaxRenderer(RendererShared shared)
{
    private const int BackgroundWidth = 25;
    private const int BackgroundHeight = 125;

    private const float InitialParallax = 0.5f;
    private const float FinalParallax = 0.3f;

    public void Render(SpriteBatch spriteBatch)
    {
        var parallax = InitialParallax + (FinalParallax - InitialParallax) * shared.GameState.Ecs.LaunchSystem.GetLaunchPercentage();

        // Center camera within background to prevent left/right shift while changing parallax
        var x = GameConfig.MinerHomeStartPosCenter.x - (float)BackgroundWidth / 2;

        var y = -150; // TODO calculate proper value for this

        spriteBatch.Draw(shared.Textures["launch-background"],
        shared.ViewHelpers.GetVisibleRectForGridCell(x, y, BackgroundWidth, BackgroundHeight, parallax),
        Color.White);

        // TODO repeat
        // spriteBatch.Draw(shared.Textures["launch-background-repeating"],
        //     shared.ViewHelpers.GetVisibleRectForGridCell(-7, GameConfig.HomeToAsteroidPointY - 25, 25, 25, parallax),
        //     Color.White);
    }
}