using System;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

// TODO
// - Tidy up this file
// - Remove other version
// - Fix jumping out - instead of hardcoded padding, calculate from grid + texture
//   - Also only apply to top + left
// - Only define cloudTextureSizePx once
// - Factor out config
//   - Wire up for ship interior

public class ScrollingBackgroundRenderer(RendererShared shared)
{
    private const int LandTextureWidthPx = 1600;
    private const int LandTextureHeightPx = 1600;
    private const int CloudTextureSizePx = 128;

    private readonly float LandSpeed = 0.5f;
    private readonly float LandParallaxFactorX = 0.4f;
    private readonly float LandParallaxFactorY = 0.2f;

    // Cloud configuration
    private readonly float CloudsPerGridCell = 0.1f;
    private readonly int CloudSeed = 42;
    private readonly float CloudSpeed = 1f;
    private readonly float CloudPadding = 2.0f;
    private readonly float CloudParallaxFactorX = 0.6f;
    private readonly float CloudParallaxFactorY = 0.3f;

    private readonly float LandTextureGridWidth = LandTextureWidthPx / GameConfig.CellTextureSizePx * 0.5f;
    private readonly float LandTextureGridHeight = LandTextureHeightPx / GameConfig.CellTextureSizePx * 0.5f;
    private readonly float CloudTextureSizeGridUnits = CloudTextureSizePx / (float)GameConfig.CellTextureSizePx;

    private (float, float, float, float) GetGridRectForVisibleLand()
    {
        var cameraPos = shared.ViewHelpers.GetCameraPos();


        var offsetXForParallax = cameraPos.X * LandParallaxFactorX - cameraPos.X;
        var offsetYForParallax = cameraPos.Y * LandParallaxFactorY - cameraPos.Y;

        var movedGridDistance = (float)(shared.GameStateManager.GameTime.TotalGameTime.TotalMilliseconds / 1000f * LandSpeed);

        var (startX, startY, viewportGridWidth, viewportGridHeight) = shared.ViewHelpers.GetViewportGridRect();
        return (startX + offsetXForParallax + movedGridDistance, startY + offsetYForParallax, viewportGridWidth, viewportGridHeight);
    }

    private (float, float, float, float) GetGridRectForVisibleClouds()
    {
        var cameraPos = shared.ViewHelpers.GetCameraPos();


        var offsetXForParallax = cameraPos.X * CloudParallaxFactorX - cameraPos.X;
        var offsetYForParallax = cameraPos.Y * CloudParallaxFactorY - cameraPos.Y;

        var movedGridDistance = (float)(shared.GameStateManager.GameTime.TotalGameTime.TotalMilliseconds / 1000f * CloudSpeed);

        var (startX, startY, viewportGridWidth, viewportGridHeight) = shared.ViewHelpers.GetViewportGridRect();
        return (startX + offsetXForParallax + movedGridDistance, startY + offsetYForParallax, viewportGridWidth, viewportGridHeight);
    }

    private float GetPixelsPerGridUnit(float gridRectWidth)
    {
        var (screenWidthPx, screenHeightPx) = shared.ViewHelpers.GetViewportSize();
        return screenWidthPx / gridRectWidth;
    }


    public void RenderBackground(SpriteBatch spriteBatch)
    {
        // Render land background
        RenderLandBackground(spriteBatch);

        // Render clouds on top
        RenderClouds(spriteBatch);
    }

    private void RenderLandBackground(SpriteBatch spriteBatch)
    {
        var (gridRectX, gridY, gridRectWidth, gridRectHeight) = GetGridRectForVisibleLand();

        // Calculate pixels per grid unit based on viewport size
        var pixelsPerGridUnit = GetPixelsPerGridUnit(gridRectWidth);

        // Calculate which background tiles are visible
        // Add padding to ensure we cover the entire screen even during sub-tile movements
        var padding = 1.0f;
        var tilesStartX = (float)Math.Floor((gridRectX - padding) / LandTextureGridWidth) * LandTextureGridWidth;
        var tilesStartY = (float)Math.Floor((gridY - padding) / LandTextureGridHeight) * LandTextureGridHeight;
        var tilesEndX = tilesStartX + gridRectWidth + padding * 2 + LandTextureGridWidth;
        var tilesEndY = tilesStartY + gridRectHeight + padding * 2 + LandTextureGridHeight;

        // Iterate over all visible background tiles
        for (var tileX = tilesStartX; tileX < tilesEndX; tileX += LandTextureGridWidth)
        {
            for (var tileY = tilesStartY; tileY < tilesEndY; tileY += LandTextureGridHeight)
            {
                // Calculate tile position relative to viewport start
                var relativePosX = (tileX - gridRectX) * pixelsPerGridUnit;
                var relativePosY = (tileY - gridY) * pixelsPerGridUnit;

                // Calculate tile size in pixels
                var tileSizeX = LandTextureGridWidth * pixelsPerGridUnit;
                var tileSizeY = LandTextureGridHeight * pixelsPerGridUnit;

                // Create the rectangle for this tile
                var tileRect = new Rectangle(
                    shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(relativePosX),
                    shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(relativePosY),
                    shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(tileSizeX),
                    shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(tileSizeY)
                );

                // Draw the tile
                spriteBatch.Draw(
                    shared.Textures[Tx.MountainsNiceTiled],
                    tileRect,
                    Color.White
                );
            }
        }
    }

    private void RenderClouds(SpriteBatch spriteBatch)
    {
        var (gridRectX, gridRectY, gridRectWidth, gridRectHeight) = GetGridRectForVisibleClouds();

        // Calculate pixels per grid unit based on viewport size
        var pixelsPerGridUnit = GetPixelsPerGridUnit(gridRectWidth);

        // Get cloud placements for the visible area
        var cloudPlacements = CloudGenerator.GetCloudPlacements(
            gridRectX, gridRectY, gridRectWidth, gridRectHeight,
            CloudsPerGridCell, CloudSeed, CloudPadding);



        // Render each cloud
        foreach (var cloud in cloudPlacements)
        {
            // Calculate cloud position relative to viewport start
            var relativePosX = (cloud.X - gridRectX) * pixelsPerGridUnit;
            var relativePosY = (cloud.Y - gridRectY) * pixelsPerGridUnit;

            // Calculate cloud size in pixels
            var cloudSizeX = CloudTextureSizeGridUnits * pixelsPerGridUnit;
            var cloudSizeY = CloudTextureSizeGridUnits * pixelsPerGridUnit;

            // Create the rectangle for this cloud
            var cloudRect = new Rectangle(
                shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(relativePosX),
                shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(relativePosY),
                shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(cloudSizeX),
                shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(cloudSizeY)
            );

            spriteBatch.Draw(
                shared.Textures["cloud-background"],
                cloudRect,
                Color.White
            );
        }
    }
}