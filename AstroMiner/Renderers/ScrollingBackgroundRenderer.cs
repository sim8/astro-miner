using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class ScrollingBackgroundRenderer(RendererShared shared)
{
    private const int LandTextureWidthPx = 1600;
    private const int LandTextureHeightPx = 1600;

    private const float Scale = 0.5f;

    private const int AnimationTime = 3000;
    private const int RenderedWidth = (int)(LandTextureWidthPx * Scale);
    private const int RenderedHeight = (int)(LandTextureHeightPx * Scale);

    public void RenderBackground(SpriteBatch spriteBatch)
    {
        var (viewportWidth, viewportHeight) = shared.ViewHelpers.GetViewportSize();

        var percentComplete = shared.GameState.GameTime.TotalGameTime.TotalMilliseconds % AnimationTime /
                              (float)AnimationTime;

        var currentOffset = (int)(RenderedHeight * (1f - percentComplete));

        var numCols = (int)Math.Ceiling(viewportWidth / (double)RenderedWidth) + 1;
        var numRows = (int)Math.Ceiling(viewportHeight / (double)RenderedHeight) + 1;

        var startY = -currentOffset;

        for (var row = 0; row < numRows; row++)
        {
            var yPos = startY + row * RenderedHeight;

            for (var col = 0; col < numCols; col++)
            {
                var xPos = col * RenderedWidth;
                spriteBatch.Draw(
                    shared.Textures["mountains-nice-tiled"],
                    new Rectangle(xPos, yPos, RenderedWidth, RenderedHeight),
                    Color.White
                );
            }
        }

        RenderClouds(spriteBatch);
    }

    private void RenderClouds(SpriteBatch spriteBatch)
    {
        foreach (var cloud in shared.GameState.CloudManager.CloudsBg)
            spriteBatch.Draw(
                shared.Textures["cloud-background"],
                new Rectangle((int)cloud.X, (int)cloud.Y, CloudManager.BackgroundCloudSizePx,
                    CloudManager.BackgroundCloudSizePx),
                Color.White
            );
        foreach (var cloud in shared.GameState.CloudManager.CloudsFg)
            spriteBatch.Draw(
                shared.Textures["cloud-background"],
                new Rectangle((int)cloud.X, (int)cloud.Y, CloudManager.ForegroundCloudSizePx,
                    CloudManager.ForegroundCloudSizePx),
                Color.White
            );
    }
}