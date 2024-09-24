using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class PlayerRenderer(
    Dictionary<string, Texture2D> textures,
    MiningState miningState,
    ViewHelpers viewHelpers)
{
    private const int PlayerBoxOffsetX = -16;
    private const int PlayerBoxOffsetY = -22;
    private const int PlayerTextureSizePx = 50;

    public void RenderPlayer(SpriteBatch spriteBatch)
    {
        var textureFrameOffsetX = miningState.Player.Direction switch
        {
            Direction.Bottom => 0,
            Direction.Left => PlayerTextureSizePx * 1,
            Direction.Top => PlayerTextureSizePx * 2,
            Direction.Right => PlayerTextureSizePx * 3,
            _ => 0
        };
        var sourceRectangle = new Rectangle(
            textureFrameOffsetX, 0,
            PlayerTextureSizePx,
            PlayerTextureSizePx);
        var destinationRectangle = viewHelpers.GetVisibleRectForObject(miningState.Player.Position,
            PlayerTextureSizePx, PlayerTextureSizePx, PlayerBoxOffsetX, PlayerBoxOffsetY);

        spriteBatch.Draw(textures["player"], destinationRectangle, sourceRectangle, Color.White);
    }
}