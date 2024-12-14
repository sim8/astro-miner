using System;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class ViewHelpers(GameState gameState, GraphicsDeviceManager graphics)
{
    private Rectangle AdjustRectForCamera(int x, int y, int width, int height)
    {
        var (xPx, yPx) = GridPosToDisplayedPx(gameState.ActiveControllableEntity.CenterPosition);
        return new Rectangle(
            x - xPx + graphics.GraphicsDevice.Viewport.Width / 2,
            y - yPx + graphics.GraphicsDevice.Viewport.Height / 2, width,
            height);
    }

    public (int, int) GetViewportSize()
    {
        return (graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);
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
        var (xPx, yPx) = GridPosToDisplayedPx(objectPos);
        xPx += textureOffsetX * GameConfig.ScaleMultiplier;
        yPx += textureOffsetY * GameConfig.ScaleMultiplier;
        return AdjustRectForCamera(xPx, yPx, textureWidth * GameConfig.ScaleMultiplier,
            textureHeight * GameConfig.ScaleMultiplier);
    }

    private static (int, int) GridPosToDisplayedPx(Vector2 gridPos)
    {
        return ((int)(gridPos.X * GameConfig.CellDisplayedSizePx), (int)(gridPos.Y * GameConfig.CellDisplayedSizePx));
    }

    public static (int, int) GridPosToTexturePx(Vector2 gridPos)
    {
        return ((int)(gridPos.X * GameConfig.CellTextureSizePx), (int)(gridPos.Y * GameConfig.CellTextureSizePx));
    }

    public static int ToXorYCoordinate(float fl)
    {
        return (int)Math.Floor(fl);
    }

    public static (int x, int y) ToGridPosition(Vector2 vector)
    {
        return (ToXorYCoordinate(vector.X), ToXorYCoordinate(vector.Y));
    }

    public static bool IsValidGridPosition(int x, int y)
    {
        return x >= 0 && x < GameConfig.GridSize && y >= 0 && y < GameConfig.GridSize;
    }
}