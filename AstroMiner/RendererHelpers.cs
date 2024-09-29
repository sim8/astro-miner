using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class RendererHelpers(
    ViewHelpers viewHelpers,
    Dictionary<string, Texture2D> textures)
{
    public void RenderRadialLightSource(SpriteBatch spriteBatch, Vector2 pos, int size = 256, float opacity = 1)
    {
        var offset = -(size / 2);
        var destinationRect = viewHelpers.GetVisibleRectForObject(pos, size, size, offset, offset);
        spriteBatch.Draw(textures["radial-light"], destinationRect, Color.White * opacity);
    }

    public void RenderDirectionalLightSource(SpriteBatch spriteBatch, Vector2 pos, Direction dir, int size = 256)
    {
        var destinationRect = viewHelpers.GetVisibleRectForObject(pos, size, size);


        var radians = dir switch
        {
            Direction.Top => 0f,
            Direction.Right => (float)Math.PI / 2,
            Direction.Bottom => (float)Math.PI,
            Direction.Left => (float)(3 * Math.PI / 2),
            _ => 0
        };

        var origin = new Vector2(textures["directional-light"].Width / 2f, textures["directional-light"].Height);

        spriteBatch.Draw(textures["directional-light"], destinationRect, null, Color.White, radians, origin,
            SpriteEffects.None, 0);
    }
}