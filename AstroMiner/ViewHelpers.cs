using Microsoft.Xna.Framework;

namespace AstroMiner;

public class ViewHelpers(MiningState miningState, GraphicsDeviceManager graphics)
{
    public Rectangle AdjustRectForCamera(int x, int y, int width, int height)
    {
        var minerXPx = (int)(miningState.MinerPos.X * GameConfig.CellDisplayedSizePx);
        var minerYPx = (int)(miningState.MinerPos.Y * GameConfig.CellDisplayedSizePx);

        return new Rectangle(
            x - minerXPx - GameConfig.MinerVisibleRadius + graphics.GraphicsDevice.Viewport.Width / 2,
            y - minerYPx - GameConfig.MinerVisibleRadius + graphics.GraphicsDevice.Viewport.Height / 2, width,
            height);
    }

    public Rectangle GetVisibleRectForGridCell(int gridX, int gridY, int widthOnGrid = 1, int heightOnGrid = 1)
    {
        return AdjustRectForCamera(gridX * GameConfig.CellDisplayedSizePx, gridY * GameConfig.CellDisplayedSizePx,
            widthOnGrid * GameConfig.CellDisplayedSizePx,
            heightOnGrid * GameConfig.CellDisplayedSizePx);
    }

    public Rectangle GetVisibleRectForObject(float gridX, float gridY, int textureWidth, int textureHeight,
        int textureOffsetX = 0, int textureOffsetY = 0)
    {
        var xPx = (int)(gridX * GameConfig.CellDisplayedSizePx) + textureOffsetX * GameConfig.ScaleMultiplier;
        var yPx = (int)(gridY * GameConfig.CellDisplayedSizePx) + textureOffsetY * GameConfig.ScaleMultiplier;
        return AdjustRectForCamera(xPx, yPx, textureWidth * GameConfig.ScaleMultiplier,
            textureHeight * GameConfig.ScaleMultiplier);
    }
}