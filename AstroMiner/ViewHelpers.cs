using Microsoft.Xna.Framework;

namespace AstroMiner;

public class ViewHelpers(MiningState miningState, GraphicsDeviceManager graphics)
{
    private Rectangle AdjustRectForCamera(int x, int y, int width, int height)
    {
        var (minerXPx, minerYPx) = GridPosToPx(miningState.MinerPos);
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

    public Rectangle GetVisibleRectForObject(Vector2 objectPos, int textureWidth, int textureHeight,
        int textureOffsetX = 0, int textureOffsetY = 0)
    {
        var (xPx, yPx) = GridPosToPx(objectPos);
        xPx += textureOffsetX * GameConfig.ScaleMultiplier;
        yPx += textureOffsetY * GameConfig.ScaleMultiplier;
        return AdjustRectForCamera(xPx, yPx, textureWidth * GameConfig.ScaleMultiplier,
            textureHeight * GameConfig.ScaleMultiplier);
    }

    public static (int, int) GridPosToPx(Vector2 gridPos)
    {
        return ((int)(gridPos.X * GameConfig.CellDisplayedSizePx), (int)(gridPos.Y * GameConfig.CellDisplayedSizePx));
    }
}