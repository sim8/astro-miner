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

        var percentComplete = shared.GameStateManager.GameTime.TotalGameTime.TotalMilliseconds % AnimationTime /
                              (float)AnimationTime;

        var currentOffset = (int)(RenderedWidth * percentComplete);

        var numCols = (int)Math.Ceiling(viewportWidth / (double)RenderedWidth) + 1;
        var numRows = (int)Math.Ceiling(viewportHeight / (double)RenderedHeight) + 1;

        var startX = -currentOffset;

        for (var row = 0; row < numRows; row++)
        {
            var yPos = row * RenderedHeight;

            for (var col = 0; col < numCols; col++)
            {
                var xPos = startX + col * RenderedWidth;
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
        shared.GameStateManager.CloudEffects.Render(spriteBatch, shared.Textures);
    }
}