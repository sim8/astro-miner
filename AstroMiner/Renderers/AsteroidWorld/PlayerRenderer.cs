using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.AsteroidWorld;

public class PlayerRenderer(
    RendererShared shared)
{
    private const int PlayerBoxOffsetX = -16;
    private const int PlayerBoxOffsetY = -22;
    private const int PlayerTextureSizePx = 50;

    public void RenderPlayer(SpriteBatch spriteBatch)
    {
        var player = shared.GameState.IsOnAsteroid
            ? shared.GameState.AsteroidWorld.Player
            : shared.GameState.HomeWorld.Player;

        var textureFrameOffsetX = player.Direction switch
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

        var destinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(player.Position,
            PlayerTextureSizePx, PlayerTextureSizePx, PlayerBoxOffsetX, PlayerBoxOffsetY);

        var tintColor = ViewHelpers.GetEntityTintColor(player);

        spriteBatch.Draw(shared.Textures["player"], destinationRectangle, sourceRectangle, tintColor);
    }
}