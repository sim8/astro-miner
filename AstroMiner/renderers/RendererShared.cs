using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.renderers;

public class RendererShared(
    GameState gameState,
    GraphicsDeviceManager graphics,
    Dictionary<string, Texture2D> textures)
{
    public readonly GameState GameState = gameState;
    public readonly Dictionary<string, Texture2D> Textures = textures;
    public readonly ViewHelpers ViewHelpers = new(gameState, graphics);

    public void RenderRadialLightSource(SpriteBatch spriteBatch, Vector2 pos, Color color, int size = 256,
        float opacity = 1)
    {
        var offset = -(size / 2);
        var destinationRect = ViewHelpers.GetVisibleRectForObject(pos, size, size, offset, offset);
        spriteBatch.Draw(Textures["radial-light"], destinationRect, color * opacity);
    }

    public void RenderRadialLightSource(SpriteBatch spriteBatch, Vector2 pos, int size = 256, float opacity = 1)
    {
        RenderRadialLightSource(spriteBatch, pos, Color.White, size, opacity);
    }

    public void RenderDirectionalLightSource(SpriteBatch spriteBatch, Vector2 pos, Direction dir, int size = 512,
        float opacity = 1)
    {
        var destinationRect = ViewHelpers.GetVisibleRectForObject(pos, size, size);


        var radians = dir switch
        {
            Direction.Top => 0f,
            Direction.Right => (float)Math.PI / 2,
            Direction.Bottom => (float)Math.PI,
            Direction.Left => (float)(3 * Math.PI / 2),
            _ => 0
        };

        var origin = new Vector2(Textures["directional-light"].Width / 2f, Textures["directional-light"].Height);

        spriteBatch.Draw(Textures["directional-light"], destinationRect, null, Color.White * opacity, radians, origin,
            SpriteEffects.None, 0);
    }

    public void RenderString(SpriteBatch spriteBatch, int startX, int startY, string str, int scale = 3)
    {
        var linePxCount = 0;
        foreach (var (x, y, width) in FontHelpers.TransformString(str))
        {
            var sourceRect = new Rectangle(x, y, width, 8);
            var destRect = new Rectangle(startX + linePxCount * scale, startY + 10, width * scale, 8 * scale);
            spriteBatch.Draw(Textures["dogica-font"], destRect, sourceRect, Color.LimeGreen);
            linePxCount += width;
        }
    }
}