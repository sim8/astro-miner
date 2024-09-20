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
            miningState.MinerDirection is Direction.Bottom or Direction.Left
                ? 0
                : GameConfig.CellTextureSizePx,
            miningState.MinerDirection is Direction.Top or Direction.Left
                ? 0
                : GameConfig.CellTextureSizePx,
            GameConfig.CellTextureSizePx,
            GameConfig.CellTextureSizePx);
        var destinationRectangle = viewHelpers.GetVisibleRectForObject(miningState.MinerPos.X,
            miningState.MinerPos.Y,
            GameConfig.CellTextureSizePx, GameConfig.CellTextureSizePx, MinerTextureOffsetX, MinerTextureOffsetY);

        spriteBatch.Draw(textures["miner-2"], destinationRectangle, sourceRectangle, Color.White);
    }
}