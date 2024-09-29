using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class MinerRenderer(
    Dictionary<string, Texture2D> textures,
    GameState gameState,
    ViewHelpers viewHelpers)
{
    private const int MinerBoxOffsetX = -13;
    private const int MinerBoxOffsetY = -20;

    public void RenderMiner(SpriteBatch spriteBatch)
    {
        var sourceRectangle = new Rectangle(
            gameState.Miner.Direction is Direction.Bottom or Direction.Left
                ? 0
                : GameConfig.CellTextureSizePx,
            gameState.Miner.Direction is Direction.Top or Direction.Left
                ? 0
                : GameConfig.CellTextureSizePx,
            GameConfig.CellTextureSizePx,
            GameConfig.CellTextureSizePx);
        var destinationRectangle = viewHelpers.GetVisibleRectForObject(gameState.Miner.Position,
            GameConfig.CellTextureSizePx, GameConfig.CellTextureSizePx, MinerBoxOffsetX, MinerBoxOffsetY);

        spriteBatch.Draw(GetTracksTexture(), destinationRectangle, sourceRectangle, Color.White);
        spriteBatch.Draw(textures["miner-no-tracks"], destinationRectangle, sourceRectangle, Color.White);
    }

    private Texture2D GetTracksTexture()
    {
        var (gridX, gridY) = ViewHelpers.GridPosToTexturePx(gameState.Miner.Position);
        if (gameState.Miner.Direction is Direction.Top) return textures["tracks-" + (2 - gridY % 3)];
        if (gameState.Miner.Direction is Direction.Right) return textures["tracks-" + gridX % 3];
        if (gameState.Miner.Direction is Direction.Bottom) return textures["tracks-" + gridY % 3];
        if (gameState.Miner.Direction is Direction.Left) return textures["tracks-" + (2 - gridX % 3)];
        return textures["tracks-1"];
    }
}