using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class LaunchParallaxRenderer(RendererShared shared)
{
    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(shared.Textures["scrolling-mountains"], shared.ViewHelpers.GetVisibleRectForGridCell(-7, -110, 25, 125, 0.5f), Color.White);
    }
}