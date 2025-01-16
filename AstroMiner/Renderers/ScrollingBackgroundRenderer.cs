using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class ScrollingBackgroundRenderer
{
    private const int LandTextureWidthPx = 64;
    private const int LandTextureHeightPx = 64;

    private const int CloudTextureWidthPx = 128;
    private const int CloudTextureHeightPx = 128;

    private const int Scale = 4;

    private const int RenderedWidth = LandTextureWidthPx * Scale;
    private const int RenderedHeight = LandTextureHeightPx * Scale;

    private const int AnimationTime = 1000;
    private readonly RendererShared shared;

    public ScrollingBackgroundRenderer(RendererShared shared)
    {
        this.shared = shared;
    }

    public void RenderBackground(SpriteBatch spriteBatch)
    {
        var (viewportWidth, viewportHeight) = shared.ViewHelpers.GetViewportSize();

        var percentComplete = shared.GameState.MsSinceStart % AnimationTime / (float)AnimationTime;

        var currentOffset = (int)(RenderedHeight * (1f - percentComplete));

        var numCols = viewportWidth / RenderedWidth + 1;
        var numRows = viewportHeight / RenderedHeight + 1;

        var startY = -currentOffset;

        for (var row = 0; row < numRows; row++)
        {
            var yPos = startY + row * RenderedHeight;

            for (var col = 0; col < numCols; col++)
            {
                var xPos = col * RenderedWidth;
                spriteBatch.Draw(
                    shared.Textures["land-background"],
                    new Rectangle(xPos, yPos, RenderedWidth, RenderedHeight),
                    Color.White
                );
            }
        }
    }
}