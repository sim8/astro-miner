using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class Renderer(GraphicsDeviceManager graphics, Dictionary<string, Texture2D> textures, MiningState miningState)
{
    private const int CellTextureSizePx = 64;
    private const int MinerTextureSizePx = 38;
    private const int ScaleMultiplier = 4;
    private const int CellDisplayedSizePx = CellTextureSizePx * ScaleMultiplier;


    private Rectangle GetVisibleRectForGridCell(int gridX, int gridY, int widthOnGrid = 1, int heightOnGrid = 1)
    {
        return AdjustRectForCamera(gridX * CellDisplayedSizePx, gridY * CellDisplayedSizePx,
            widthOnGrid * CellDisplayedSizePx,
            heightOnGrid * CellDisplayedSizePx);
    }

    private Rectangle GetVisibleRectForObject(float gridX, float gridY, int textureWidth, int textureHeight)
    {
        var xPx = (int)(gridX * CellDisplayedSizePx);
        var yPx = (int)(gridY * CellDisplayedSizePx);
        return AdjustRectForCamera(xPx, yPx, textureWidth * ScaleMultiplier, textureHeight * ScaleMultiplier);
    }

    private Rectangle AdjustRectForCamera(int x, int y, int width, int height)
    {
        var minerXPx = (int)(miningState.MinerPos.X * CellDisplayedSizePx);
        var minerYPx = (int)(miningState.MinerPos.Y * CellDisplayedSizePx);
        var minerVisibleRadius = MinerTextureSizePx * ScaleMultiplier / 2;
        return new Rectangle(
            x - minerXPx - minerVisibleRadius + graphics.GraphicsDevice.Viewport.Width / 2,
            y - minerYPx - minerVisibleRadius + graphics.GraphicsDevice.Viewport.Height / 2, width, height);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < MiningState.Rows; row++)
        for (var col = 0; col < MiningState.Columns; col++)
        {
            var isRock = miningState.GetCellState(row, col) == CellState.Rock;
            spriteBatch.Draw(isRock ? textures["rock"] : textures["floor"], GetVisibleRectForGridCell(row, col),
                Color.White);
        }

        var sourceRectangle = new Rectangle(
            miningState.MinerDirection == Direction.Top || miningState.MinerDirection == Direction.Left
                ? 0
                : MinerTextureSizePx,
            miningState.MinerDirection == Direction.Top || miningState.MinerDirection == Direction.Right
                ? 0
                : MinerTextureSizePx,
            MinerTextureSizePx,
            MinerTextureSizePx);
        var destinationRectangle = GetVisibleRectForObject(miningState.MinerPos.X, miningState.MinerPos.Y,
            MinerTextureSizePx, MinerTextureSizePx);
        spriteBatch.Draw(textures["miner"], destinationRectangle, sourceRectangle, Color.White);
    }
}