using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class Renderer(
    GraphicsDeviceManager graphics,
    Dictionary<string, Texture2D> textures,
    MiningState miningState,
    int scaleMultiplier,
    int minerTextureSizePx,
    int cellTextureSizePx)
{
    private readonly int _cellDisplayedSizePx = cellTextureSizePx * scaleMultiplier;

    // private RenderTilesetCell()

    private Rectangle GetVisibleRectForGridCell(int gridX, int gridY, int widthOnGrid = 1, int heightOnGrid = 1)
    {
        return AdjustRectForCamera(gridX * _cellDisplayedSizePx, gridY * _cellDisplayedSizePx,
            widthOnGrid * _cellDisplayedSizePx,
            heightOnGrid * _cellDisplayedSizePx);
    }

    private Rectangle GetVisibleRectForObject(float gridX, float gridY, int textureWidth, int textureHeight)
    {
        var xPx = (int)(gridX * _cellDisplayedSizePx);
        var yPx = (int)(gridY * _cellDisplayedSizePx);
        return AdjustRectForCamera(xPx, yPx, textureWidth * scaleMultiplier, textureHeight * scaleMultiplier);
    }

    private Rectangle AdjustRectForCamera(int x, int y, int width, int height)
    {
        var minerXPx = (int)(miningState.MinerPos.X * _cellDisplayedSizePx);
        var minerYPx = (int)(miningState.MinerPos.Y * _cellDisplayedSizePx);
        var minerVisibleRadius = minerTextureSizePx * scaleMultiplier / 2;

        // return new Rectangle(
        //     (x - minerXPx - minerVisibleRadius + graphics.GraphicsDevice.Viewport.Width / 2) / 4 + 500,
        //     (y - minerYPx - minerVisibleRadius + graphics.GraphicsDevice.Viewport.Height / 2) / 4 + 500, width / 4,
        //     height / 4);

        return new Rectangle(
            x - minerXPx - minerVisibleRadius + graphics.GraphicsDevice.Viewport.Width / 2,
            y - minerYPx - minerVisibleRadius + graphics.GraphicsDevice.Viewport.Height / 2, width,
            height);
    }


    public void Render(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < MiningState.GridSize; row++)
        for (var col = 0; col < MiningState.GridSize; col++)
        {
            var cellState = miningState.GetCellState(col, row);
            if (Tilesets.TilesetTextureNames.TryGetValue(cellState, out var name))
            {
                var offset = Tilesets.GetTileCoords(miningState, col, row);
                var tilesetSourceRect = new Rectangle(offset.Item1 * cellTextureSizePx,
                    offset.Item2 * cellTextureSizePx,
                    cellTextureSizePx, cellTextureSizePx);
                spriteBatch.Draw(textures[name], GetVisibleRectForGridCell(col, row),
                    tilesetSourceRect, Color.White);
                if (offset == (1, 2)) // Top piece
                {
                    var overlayOffset = (5, 2);
                    var overlaySourceRect = new Rectangle(overlayOffset.Item1 * cellTextureSizePx,
                        overlayOffset.Item2 * cellTextureSizePx,
                        cellTextureSizePx, cellTextureSizePx);
                    var overlayOpacity = HasFloorWithinTwoTiles(col, row) ? 0.8f : 1;
                    spriteBatch.Draw(textures[name], GetVisibleRectForGridCell(col, row),
                        overlaySourceRect, Color.White * overlayOpacity);
                }
            }
            else if (miningState.GetCellState(col, row) == CellState.Floor)
            {
                var tilesetSourceRect = new Rectangle(3 * cellTextureSizePx,
                    cellTextureSizePx,
                    cellTextureSizePx, cellTextureSizePx);
                spriteBatch.Draw(textures["rock-tileset"], GetVisibleRectForGridCell(col, row), tilesetSourceRect,
                    Color.White);
            }
        }

        var sourceRectangle = new Rectangle(
            miningState.MinerDirection == Direction.Top || miningState.MinerDirection == Direction.Left
                ? 0
                : minerTextureSizePx,
            miningState.MinerDirection == Direction.Top || miningState.MinerDirection == Direction.Right
                ? 0
                : minerTextureSizePx,
            minerTextureSizePx,
            minerTextureSizePx);
        var destinationRectangle = GetVisibleRectForObject(miningState.MinerPos.X, miningState.MinerPos.Y,
            minerTextureSizePx, minerTextureSizePx);
        spriteBatch.Draw(textures["miner"], destinationRectangle, sourceRectangle, Color.White);
    }

    private bool HasFloorWithinTwoTiles(int col, int row)
    {
        for (var x = col - 1; x <= col + 1; x++)
            if ((miningState.IsValidGridPosition(x, row - 2) &&
                 miningState.GetCellState(x, row - 2) == CellState.Floor) ||
                (miningState.IsValidGridPosition(x, row + 2) &&
                 miningState.GetCellState(x, row + 2) == CellState.Floor))
                return true;
        for (var y = row - 1; y <= row + 1; y++)
            if ((miningState.IsValidGridPosition(col + 2, y) &&
                 miningState.GetCellState(col + 2, y) == CellState.Floor) ||
                (miningState.IsValidGridPosition(col - 2, y) &&
                 miningState.GetCellState(col - 2, y) == CellState.Floor))
                return true;
        return false;
    }
}