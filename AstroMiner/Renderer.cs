using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class Renderer(GraphicsDeviceManager graphics, Dictionary<string, Texture2D> textures)
{
    private const int CellTextureSizePx = 64;
    private const int MinerTextureSizePx = 38;
    private const int ScaleMultiplier = 4;
    private const int CellDisplayedSizePx = CellTextureSizePx * ScaleMultiplier;


    private Rectangle GetVisibleRectForGridCell(int gridX, int gridY, int widthOnGrid = 1, int heightOnGrid = 1)
    {
        return new Rectangle(gridX * CellDisplayedSizePx, gridY * CellDisplayedSizePx, widthOnGrid * CellDisplayedSizePx,
            heightOnGrid * CellDisplayedSizePx);
    }
    
    private Rectangle GetVisibleRectForObject(float gridX, float gridY, int textureWidth, int textureHeight)
    {
        int xPx = (int)(gridX * CellDisplayedSizePx);
        int yPx = (int)(gridY * CellDisplayedSizePx);
        return new Rectangle(xPx, yPx, textureWidth * ScaleMultiplier, textureHeight * ScaleMultiplier);
    }

    // private Vector2 GetGridCameraPosition()
    // {
    //     float viewportWidthOnGrid = PxToGridCoordinate(graphics.GraphicsDevice.Viewport.Width);
    //     float viewportHeightOnGrid = PxToGridCoordinate(graphics.GraphicsDevice.Viewport.Height);
    //     return new Vector2();
    // }
    
    public void Render(SpriteBatch spriteBatch, MiningState miningState)
    {
        for (int row = 0; row < MiningState.Rows; row++)
        {
            for (int col = 0; col < MiningState.Columns; col++)
            {
                bool isRock = miningState.GetCellState(row, col) == CellState.Rock;
                spriteBatch.Draw(isRock ? textures["rock"] : textures["floor"], GetVisibleRectForGridCell(row, col), Color.White);
            }
        }
        Rectangle sourceRectangle = new Rectangle(
            miningState.MinerDirection == Direction.Top ||miningState.MinerDirection == Direction.Left ? 0 : MinerTextureSizePx,
            miningState.MinerDirection == Direction.Top ||miningState.MinerDirection == Direction.Right ? 0 : MinerTextureSizePx,
            MinerTextureSizePx,
            MinerTextureSizePx);
        Rectangle destinationRectangle = GetVisibleRectForObject(miningState.MinerPos.X, miningState.MinerPos.Y, MinerTextureSizePx, MinerTextureSizePx);
        spriteBatch.Draw(textures["miner"], destinationRectangle, sourceRectangle, Color.White);
    }
}