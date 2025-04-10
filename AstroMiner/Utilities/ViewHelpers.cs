using System;
using AstroMiner.Definitions;
using AstroMiner.ECS;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.Utilities;

public class ViewHelpers(GameState gameState, GraphicsDeviceManager graphics)
{
    // Should only be used at very end of calc pipeline
    private int ConvertToRenderedPxValue_CAUTION(double value)
    {
        // Round up to reduce visual artifacts
        return (int)Math.Ceiling(value);
    }

    // TODO maybe this should be clamping the visible screen rather than player pos? Could reuse + cache visible grid logic?
    private Vector2 GetCameraPos()
    {
        const int DistanceThreshold = 5;
        var (cols, rows) = gameState.ActiveWorldState.GetGridSize();
        var playerCenterPos = gameState.Ecs.ActiveControllableEntityCenterPosition;

        float cameraX;
        float cameraY;

        // For the X axis, if the grid's width is too small (cannot support a margin on both sides), center the camera.
        if (cols <= DistanceThreshold * 2)
            cameraX = cols / 2f;
        else
            // Otherwise, clamp the player's X between DistanceThreshold and (cols - DistanceThreshold).
            cameraX = Math.Clamp(playerCenterPos.X, DistanceThreshold, cols - DistanceThreshold);

        // For the Y axis, do the same: center the camera if too small, or clamp otherwise.
        if (rows <= DistanceThreshold * 2)
            cameraY = rows / 2f;
        else
            cameraY = Math.Clamp(playerCenterPos.Y, DistanceThreshold, rows - DistanceThreshold);

        return new Vector2(cameraX, cameraY);
    }

    private Rectangle AdjustRectForCamera(float x, float y, float width, float height, float parallaxLayer = 1)
    {
        var cameraPos = GetCameraPos();
        var (xPx, yPx) = GridPosToDisplayedPx(cameraPos);


        var rectCenterX = x + width / 2;
        var rectCenterY = y + height / 2;

        // Parallax calculation is applied based on distances between centers of rectangles for more predictable scaling
        var centerOffsetFromPlayerX = (rectCenterX - xPx) * parallaxLayer;
        var centerOffsetFromPlayerY = (rectCenterY - yPx) * parallaxLayer;

        var adjustedX = centerOffsetFromPlayerX - width / 2 + graphics.GraphicsDevice.Viewport.Width / 2f;
        var adjustedY = centerOffsetFromPlayerY - height / 2 + graphics.GraphicsDevice.Viewport.Height / 2f;

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

    public Rectangle GetVisibleRectForGridCell(float gridX, float gridY, float widthOnGrid = 1, float heightOnGrid = 1,
        float parallaxLayer = 1)
    {
        return AdjustRectForCamera(ConvertGridUnitsToVisiblePx(gridX, parallaxLayer),
            ConvertGridUnitsToVisiblePx(gridY, parallaxLayer),
            ConvertGridUnitsToVisiblePx(widthOnGrid, parallaxLayer),
            ConvertGridUnitsToVisiblePx(heightOnGrid, parallaxLayer), parallaxLayer);
    }

    public Rectangle GetVisibleRectForWallQuadrant(int gridX, int gridY, Corner corner)
    {
        // Quadrants are rendered at double height, with top half overlaying quadrant behind
        var isTopQuadrant = corner is Corner.TopLeft or Corner.TopRight;
        var quadrantX = corner is Corner.TopRight or Corner.BottomRight ? gridX + 0.5f : gridX;

        // All quadrants rendered - .5 y to account for space in texture
        var quadrantY = isTopQuadrant ? gridY - 0.5f : gridY;
        return GetVisibleRectForGridCell(quadrantX, quadrantY, 0.5f);
    }

    public Rectangle GetVisibleRectForFloorQuadrant(int gridX, int gridY, Corner corner)
    {
        var quadrantX = corner is Corner.TopRight or Corner.BottomRight ? gridX + 0.5f : gridX;
        var quadrantY = corner is Corner.TopLeft or Corner.TopRight ? gridY : gridY + 0.5f;

        return GetVisibleRectForGridCell(quadrantX, quadrantY, 0.5f, 0.5f);
    }

    private float ConvertGridUnitsToVisiblePx(float gridUnits, float parallaxLayer = 1f)
    {
        // TODO fix scale issues. Scale and parallax aren't working togethrr. Scale should happen after parallax
        // It'd be nice if scale scaled accordingly to parallax layer as well
        var scale = parallaxLayer < 1 ? 2 : gameState.Camera.ScaleMultiplier;
        return gridUnits * GameConfig.CellTextureSizePx * scale;
    }

    private float ConvertVisiblePxToGridUnits(float visiblePx)
    {
        return visiblePx / (GameConfig.CellTextureSizePx * gameState.Camera.ScaleMultiplier);
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

    public Rectangle GetVisibleRectForObject(Vector2 objectPos, float gridWidth, float gridHeight)
    {
        var (xPx, yPx) = GridPosToDisplayedPx(objectPos);
        var width = ConvertGridUnitsToVisiblePx(gridWidth);
        var height = ConvertGridUnitsToVisiblePx(gridHeight);
        return AdjustRectForCamera(xPx, yPx, width, height);
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

    public static Color GetEntityTintColor(Ecs ecs, int entityId)
    {
        var baseColor = GetEntityBaseTintColor(ecs, entityId);
        return baseColor * GetEntityOpacityForPortal(ecs, entityId);
    }

    private static float GetEntityOpacityForPortal(Ecs ecs, int entityId)
    {
        var movement = ecs.GetComponent<MovementComponent>(entityId);
        if (movement == null || movement.PortalStatus == PortalStatus.None) return 1f;

        var position = ecs.GetComponent<PositionComponent>(entityId);


        var (gridX, gridY) = ToGridPosition(position.CenterPosition);
        var config = WorldGrid.GetPortalConfig(position.World, (gridX, gridY));

        if (config == null) return 1f;

        var percentageOutOfPortal = config.Direction switch
        {
            Direction.Top => position.CenterPosition.Y % 1,
            Direction.Bottom => 1 - position.CenterPosition.Y % 1,
            Direction.Right => 1 - position.CenterPosition.X % 1,
            Direction.Left => position.CenterPosition.X % 1,
            _ => 1f
        };

        var percentageToCutOffOpacity = 0.5f;

        return Math.Clamp((percentageOutOfPortal - percentageToCutOffOpacity) / percentageToCutOffOpacity, 0f, 1f);
    }

    // Tint red if taking damage
    private static Color GetEntityBaseTintColor(Ecs ecs, int entityId)
    {
        var healthComponent = ecs.GetComponent<HealthComponent>(entityId);

        if (healthComponent == null) return Color.White;

        if (!healthComponent.IsAnimatingDamage)
        {
            if (healthComponent.TimeOnLavaMs > 0)
            {
                var gb = (int)(255 * (1f - healthComponent.LavaTimePercentToTakingDamage));
                return new Color(255, gb, gb, 255);
            }

            return Color.White;
        }

        var flashFrame =
            (int)(healthComponent.TotalDamageAnimationTimeMs / (GameConfig.DamageAnimationTimeMs / 8.0f));
        return flashFrame % 2 == 0 ? Color.Red : Color.White;
    }

    public (int startCol, int startRow, int endCol, int endRow) GetVisibleGrid(int padding = 0)
    {
        var cameraPos = GetCameraPos();
        var (viewportWidthPx, viewportHeightPx) = GetViewportSize();

        var viewportGridWidth = ConvertVisiblePxToGridUnits(viewportWidthPx);
        var viewportGridHeight = ConvertVisiblePxToGridUnits(viewportHeightPx);

        var startCol = (int)(cameraPos.X - viewportGridWidth / 2) - padding;
        var startRow = (int)(cameraPos.Y - viewportGridHeight / 2) - padding;

        var endCol = (int)Math.Ceiling(cameraPos.X + viewportGridWidth / 2) + padding;
        var endRow = (int)Math.Ceiling(cameraPos.Y + viewportGridHeight / 2) + padding;

        return (
            Math.Max(0, startCol),
            Math.Max(0, startRow),
            Math.Min(GameConfig.GridSize, endCol),
            Math.Min(GameConfig.GridSize, endRow)
        );
    }

    public static Vector2 AbsoluteXyPxToGridPos(int xPx, int yPx)
    {
        return new Vector2(xPx / (float)GameConfig.CellTextureSizePx, yPx / (float)GameConfig.CellTextureSizePx);
    }
}