using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class MinerRenderer(
    Dictionary<string, Texture2D> textures,
    MiningState miningState,
    ViewHelpers viewHelpers)
{
    private const int MinerTextureOffsetX = -13;
    private const int MinerTextureOffsetY = -20;

    public void RenderMiner(SpriteBatch spriteBatch)
    {
        var sourceRectangle = new Rectangle(
            miningState.Miner.Direction is Direction.Bottom or Direction.Left
                ? 0
                : GameConfig.CellTextureSizePx,
            miningState.Miner.Direction is Direction.Top or Direction.Left
                ? 0
                : GameConfig.CellTextureSizePx,
            GameConfig.CellTextureSizePx,
            GameConfig.CellTextureSizePx);
        var destinationRectangle = viewHelpers.GetVisibleRectForObject(miningState.Miner.Position,
            GameConfig.CellTextureSizePx, GameConfig.CellTextureSizePx, MinerTextureOffsetX, MinerTextureOffsetY);

        spriteBatch.Draw(GetTracksTexture(), destinationRectangle, sourceRectangle, Color.White);
        spriteBatch.Draw(textures["miner-no-tracks"], destinationRectangle, sourceRectangle, Color.White);
    }

    private Texture2D GetTracksTexture()
    {
        var (gridX, gridY) = ViewHelpers.GridPosToTexturePx(miningState.Miner.Position);
        if (miningState.Miner.Direction is Direction.Top) return textures["tracks-" + (2 - gridY % 3)];
        if (miningState.Miner.Direction is Direction.Right) return textures["tracks-" + gridX % 3];
        if (miningState.Miner.Direction is Direction.Bottom) return textures["tracks-" + gridY % 3];
        if (miningState.Miner.Direction is Direction.Left) return textures["tracks-" + (2 - gridX % 3)];
        return textures["tracks-1"];
    }
}