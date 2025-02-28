using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class LaunchParallaxRenderer(RendererShared shared)
{
    private const int BackgroundWidth = 25;
    private const int RepeatingBackgroundHeight = 25;
    private const int BackgroundHeight = 125;

    private const int NonRepeatingBackgroundY = -100; // TODO calculate proper value for this

    // Center camera within background to prevent left/right shift while changing parallax
    private static readonly float BackgroundX = GameConfig.Launch.MinerHomeStartPosCenter.x - (float)BackgroundWidth / 2;

    private const float InitialParallax = 0.5f;

    public void Render(SpriteBatch spriteBatch)
    {
        var parallax = InitialParallax + (GameConfig.Launch.BackgroundParallax - InitialParallax) * shared.GameState.Ecs.LaunchSystem.GetLaunchPercentage();

        spriteBatch.Draw(shared.Textures["launch-background"],
        shared.ViewHelpers.GetVisibleRectForGridCell(BackgroundX, NonRepeatingBackgroundY, BackgroundWidth, BackgroundHeight, parallax, GameConfig.Launch.MinerHomeStartPosCenter),
        Color.White);

        RenderRepeatingBackground(spriteBatch, parallax);
    }

    private void RenderRepeatingBackground(SpriteBatch spriteBatch, float parallax)
    {
        var (startCol, startRow, endCol, endRow) = shared.ViewHelpers.GetVisibleGrid(RepeatingBackgroundHeight);

        spriteBatch.Draw(shared.Textures["launch-background-repeating"],
        shared.ViewHelpers.GetVisibleRectForGridCell(BackgroundX, NonRepeatingBackgroundY - RepeatingBackgroundHeight, BackgroundWidth, RepeatingBackgroundHeight, parallax, GameConfig.Launch.MinerHomeStartPosCenter),
    Color.Red);
    }
}