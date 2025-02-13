using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class ScrollingBackgroundRenderer(RendererShared shared)
{
    private const int LandTextureWidthPx = 64;
    private const int LandTextureHeightPx = 64;

    private const int Scale = 4;

    private const int RenderedWidth = LandTextureWidthPx * Scale;
    private const int RenderedHeight = LandTextureHeightPx * Scale;

    private const int AnimationTime = 1300;

    public void RenderBackground(SpriteBatch spriteBatch)
    {
        var (viewportWidth, viewportHeight) = shared.ViewHelpers.GetViewportSize();

        var percentComplete = shared.GameState.MsSinceStart % AnimationTime / (float)AnimationTime;

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
                    shared.Textures["land-background"],
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