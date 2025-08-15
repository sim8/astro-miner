using System;
using AstroMiner.Definitions;
using AstroMiner.ECS;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;
using static AstroMiner.Definitions.GameConfig;

namespace AstroMiner.Utilities;

public class ViewHelpers(BaseGame game, GraphicsDeviceManager graphics)
{
    // Should only be used at very end of calc pipeline
    private int ConvertToRenderedPxValue_CAUTION(double value)
    {
        // Round up to reduce visual artifacts
        return (int)Math.Ceiling(value);
    }

    private Vector2 GetCameraPos()
    {
        var playerCenterPos = game.StateManager.Ecs.ActiveControllableEntityCenterPosition;
        var cameraPosWithPortal = OverrideCameraPosIfUsingPortal(playerCenterPos);
        return ClampCameraPosForGridBounds(cameraPosWithPortal);
    }

    private Vector2 OverrideCameraPosIfUsingPortal(Vector2 cameraPos)
    {
        if (game.Model.Ecs.ActiveControllableEntityId == null) return cameraPos;

        // TODO

        return cameraPos;
    }

    /**
     * Clamp camera pos so no off-grid area is shown. If grid is smaller
     * than viewport (x or y) center instead
     */
    private Vector2 ClampCameraPosForGridBounds(Vector2 cameraPos)
    {
        return cameraPos;
        var (viewportGridWidth, viewportGridHeight) = GetViewportGridSize();

        var widthThreshold = viewportGridWidth / 2;
        var heightThreshold = viewportGridHeight / 2;

        var (cols, rows) = game.StateManager.ActiveWorldState.GetGridSize();

        float cameraX;
        float cameraY;

        // Remove clamping for Oizus left (pier) and top (launch sequence)
        var leftThreshold = game.Model.ActiveWorld == World.Home ? 0 : widthThreshold;
        var topThreshold = game.Model.ActiveWorld == World.Home ? -1000 : heightThreshold;


        if (cols <= viewportGridWidth)
            cameraX = cols / 2f;
        else
            cameraX = Math.Clamp(cameraPos.X, leftThreshold, cols - widthThreshold);

        if (rows <= viewportGridHeight)
            cameraY = rows / 2f;
        else
            cameraY = Math.Clamp(cameraPos.Y, topThreshold, rows - heightThreshold);

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

    private (float, float) GetViewportGridSize()
    {
        var (viewportWidthPx, viewportHeightPx) = GetViewportSize();

        var viewportGridWidth = ConvertVisiblePxToGridUnits(viewportWidthPx);
        var viewportGridHeight = ConvertVisiblePxToGridUnits(viewportHeightPx);
        return (viewportGridWidth, viewportGridHeight);
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
        return gridUnits * CellTextureSizePx * game.StateManager.Camera.ScaleMultiplier;
    }

    private float ConvertVisiblePxToGridUnits(float visiblePx)
    {
        return visiblePx / (CellTextureSizePx * game.StateManager.Camera.ScaleMultiplier);
    }

    private float ConvertTexturePxToVisiblePx(int numToScale)
    {
        return numToScale * game.StateManager.Camera.ScaleMultiplier;
    }

    public static float ConvertTexturePxToGridUnits(int texturePx)
    {
        return (float)texturePx / CellTextureSizePx;
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
        return ((int)(gridPos.X * CellTextureSizePx), (int)(gridPos.Y * CellTextureSizePx));
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
        return x >= 0 && x < GridSize && y >= 0 && y < GridSize;
    }

    public static Color GetEntityTintColor(Ecs ecs, int entityId)
    {
        var baseColor = GetEntityBaseTintColor(ecs, entityId);

        if (ecs.InteractionSystem.InteractableEntityId == entityId) baseColor = Color.Yellow;

        return baseColor * GetEntityOpacityForPortal(ecs, entityId);
    }

    private static float GetEntityOpacityForPortal(Ecs ecs, int entityId)
    {
        var movement = ecs.GetComponent<MovementComponent>(entityId);
        if (movement == null || movement.PortalStatus == PortalStatus.None) return 1f;

        var position = ecs.GetComponent<PositionComponent>(entityId);


        var (gridX, gridY) = ToGridPosition(position.CenterPosition);
        var config = StaticWorlds.GetPortalConfig(position.World, (gridX, gridY), true);

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
            (int)(healthComponent.TotalDamageAnimationTimeMs / (DamageAnimationTimeMs / 8.0f));
        return flashFrame % 2 == 0 ? Color.Red : Color.White;
    }

    public (int startCol, int startRow, int endCol, int endRow) GetVisibleGrid(int padding = 0)
    {
        var cameraPos = GetCameraPos();
        var (viewportGridWidth, viewportGridHeight) = GetViewportGridSize();

        var startCol = (int)(cameraPos.X - viewportGridWidth / 2) - padding;
        var startRow = (int)(cameraPos.Y - viewportGridHeight / 2) - padding;

        var endCol = (int)Math.Ceiling(cameraPos.X + viewportGridWidth / 2) + padding;
        var endRow = (int)Math.Ceiling(cameraPos.Y + viewportGridHeight / 2) + padding;

        return (
            Math.Max(0, startCol),
            Math.Max(0, startRow),
            Math.Min(GridSize, endCol),
            Math.Min(GridSize, endRow)
        );
    }

    public static Vector2 AbsoluteXyPxToGridPos(int xPx, int yPx)
    {
        return new Vector2(xPx / (float)CellTextureSizePx, yPx / (float)CellTextureSizePx);
    }
}