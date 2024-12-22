using System;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class ViewHelpers(GameState gameState, GraphicsDeviceManager graphics)
{
    // Should only be used at very end of calc pipeline
    private int ConvertToRenderedPxValue_CAUTION(double value)
    {
        // Round up to reduce visual artifacts
        return (int)Math.Ceiling(value);
    }

    private Rectangle AdjustRectForCamera(float x, float y, float width, float height)
    {
        var (xPx, yPx) = GridPosToDisplayedPx(gameState.ActiveControllableEntity.CenterPosition);
        var adjustedX = x - xPx + graphics.GraphicsDevice.Viewport.Width / 2f;
        var adjustedY = y - yPx + graphics.GraphicsDevice.Viewport.Height / 2f;
        return new Rectangle(
            ConvertToRenderedPxValue_CAUTION(adjustedX),
            ConvertToRenderedPxValue_CAUTION(adjustedY),
            ConvertToRenderedPxValue_CAUTION(width),
            ConvertToRenderedPxValue_CAUTION(height)
        );
    }

    public (int, int) GetViewportSize()
    {
        return (graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);
    }

    public Rectangle GetVisibleRectForGridCell(float gridX, float gridY, float widthOnGrid = 1, float heightOnGrid = 1)
    {
        return AdjustRectForCamera(ConvertGridUnitsToVisiblePx(gridX), ConvertGridUnitsToVisiblePx(gridY),
            ConvertGridUnitsToVisiblePx(widthOnGrid),
            ConvertGridUnitsToVisiblePx(heightOnGrid));
    }

    public Rectangle GetVisibleRectForGridQuadrant(int gridX, int gridY, Corner corner)
    {
        // Quadrants are rendered at double height, with top half overlaying quadrant behind
        var isTopQuadrant = corner is Corner.TopLeft or Corner.TopRight;
        var quadrantX = corner is Corner.TopRight or Corner.BottomRight ? gridX + 0.5f : gridX;

        // All quadrants rendered - .5 y to account for space in texture
        var quadrantY = isTopQuadrant ? gridY - 0.5f : gridY;
        return GetVisibleRectForGridCell(quadrantX, quadrantY, 0.5f);
    }

    private float ConvertGridUnitsToVisiblePx(float gridUnits)
    {
        return gridUnits * GameConfig.CellTextureSizePx * gameState.Camera.ScaleMultiplier;
    }

    private float ConvertTexturePxToVisiblePx(int numToScale)
    {
        return numToScale * gameState.Camera.ScaleMultiplier;
    }

    public Rectangle GetVisibleRectForObject(Vector2 objectPos, int textureWidth, int textureHeight,
        int textureOffsetX = 0, int textureOffsetY = 0)
    {
        var (xPx, yPx) = GridPosToDisplayedPx(objectPos);
        xPx += ConvertTexturePxToVisiblePx(textureOffsetX);
        yPx += ConvertTexturePxToVisiblePx(textureOffsetY);
        return AdjustRectForCamera(xPx, yPx, ConvertTexturePxToVisiblePx(textureWidth),
            ConvertTexturePxToVisiblePx(textureHeight));
    }

    private (float, float) GridPosToDisplayedPx(Vector2 gridPos)
    {
        return (ConvertGridUnitsToVisiblePx(gridPos.X), ConvertGridUnitsToVisiblePx(gridPos.Y));
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