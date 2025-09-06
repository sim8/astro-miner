using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class RendererShared(
    BaseGame game)
{
    public readonly BaseGame Game = game;
    public readonly GameStateManager GameStateManager = game.StateManager;
    public readonly GraphicsDeviceManager Graphics = game.Graphics;
    public readonly Dictionary<string, Texture2D> Textures = game.Textures;
    public readonly ViewHelpers ViewHelpers = new(game, game.Graphics);

    public void RenderRadialLightSource(SpriteBatch spriteBatch, Vector2 pos, Color color, int size = 256,
        float opacity = 1)
    {
        var offset = -(size / 2);
        var destinationRect = ViewHelpers.GetVisibleRectForObject(pos, size, size, offset, offset);
        spriteBatch.Draw(Textures[Tx.RadialLight], destinationRect, color * opacity);
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

        var origin = new Vector2(Textures[Tx.DirectionalLight].Width / 2f, Textures[Tx.DirectionalLight].Height);

        spriteBatch.Draw(Textures[Tx.DirectionalLight], destinationRect, null, Color.White * opacity, radians, origin,
            SpriteEffects.None, 0);
    }

    public void RenderString(SpriteBatch spriteBatch, int startX, int startY, string str, int scale = 3)
    {
        var letterXOffset = 0;
        foreach (var (x, y, width, height) in FontHelpers.TransformString(str))
        {
            var sourceRect = new Rectangle(x, y, width, 8);
            var destRect = new Rectangle(startX + letterXOffset * scale, startY + 10, width * scale, height * scale);
            spriteBatch.Draw(Textures[Tx.DogicaFont], destRect, sourceRect, Color.LimeGreen);
            letterXOffset += width;
        }
    }
}