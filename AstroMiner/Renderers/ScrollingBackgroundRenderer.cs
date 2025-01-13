using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class ScrollingBackgroundRenderer(
    RendererShared shared)
{
    private const int TextureWidthPx = 64;
    private const int TextureHeightPx = 64;

    private const int Scale = 4;

    private const int RenderedWidth = TextureWidthPx * Scale;
    private const int RenderedHeight = TextureHeightPx * Scale;

    private const int AnimationTime = 1000;

    public void RenderBackground(SpriteBatch spriteBatch)
    {
        var (viewportWidth, viewportHeight) = shared.ViewHelpers.GetViewportSize();
        var percentComplete = shared.GameState.MsSinceStart % AnimationTime / (float)AnimationTime;

        Console.WriteLine(percentComplete);

        var currentOffset = (int)(RenderedHeight * percentComplete);

        spriteBatch.Draw(shared.Textures["land-background"],
            new Rectangle(0, currentOffset, RenderedWidth, RenderedHeight),
            Color.White);
    }
}