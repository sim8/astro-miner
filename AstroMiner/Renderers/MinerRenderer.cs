using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class MinerRenderer(
    RendererShared shared)
{
    private const int MinerBoxOffsetX = -13;
    private const int MinerBoxOffsetY = -20;
    private const int MinerTextureSize = 64;

    public void RenderMiner(SpriteBatch spriteBatch)
    {
        var sourceRectangle = new Rectangle(
            shared.GameState.Miner.Direction is Direction.Bottom or Direction.Left
                ? 0
                : MinerTextureSize,
            shared.GameState.Miner.Direction is Direction.Top or Direction.Left
                ? 0
                : MinerTextureSize,
            MinerTextureSize,
            MinerTextureSize);
        var destinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(shared.GameState.Miner.Position,
            MinerTextureSize, MinerTextureSize, MinerBoxOffsetX, MinerBoxOffsetY);

        var tintColor = shared.GameState.Miner.IsDead
            ? Color.Gray
            : ViewHelpers.GetEntityTintColor(shared.GameState.Miner);

        spriteBatch.Draw(GetTracksTexture(), destinationRectangle, sourceRectangle, tintColor);
        spriteBatch.Draw(shared.Textures["miner-no-tracks"], destinationRectangle, sourceRectangle, tintColor);
    }

    private Texture2D GetTracksTexture()
    {
        var (gridX, gridY) = ViewHelpers.GridPosToTexturePx(shared.GameState.Miner.Position);
        switch (shared.GameState.Miner.Direction)
        {
            case Direction.Top: return shared.Textures["tracks-" + (2 - gridY % 3)];
            case Direction.Right: return shared.Textures["tracks-" + gridX % 3];
            case Direction.Bottom: return shared.Textures["tracks-" + gridY % 3];
            case Direction.Left: return shared.Textures["tracks-" + (2 - gridX % 3)];
        }

        return shared.Textures["tracks-1"];
    }
}