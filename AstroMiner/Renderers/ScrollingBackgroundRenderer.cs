using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class ScrollingBackgroundRenderer(RendererShared shared)
{


    private (float, float, float, float) GetGridRectForVisibleLand(ScrollingBackgroundConfig config)
    {
        var cameraPos = shared.ViewHelpers.GetCameraPos();


        var offsetXForParallax = cameraPos.X * config.LandParallaxFactorX - cameraPos.X;
        var offsetYForParallax = cameraPos.Y * config.LandParallaxFactorY - cameraPos.Y;

        var movedGridDistance = (float)(shared.GameStateManager.GameTime.TotalGameTime.TotalMilliseconds / 1000f * config.LandSpeed);

        var (startX, startY, viewportGridWidth, viewportGridHeight) = shared.ViewHelpers.GetViewportGridRect();
        return (startX + offsetXForParallax + movedGridDistance, startY + offsetYForParallax, viewportGridWidth, viewportGridHeight);
    }

    private float GetPixelsPerGridUnit(float gridRectWidth)
    {
        var (screenWidthPx, screenHeightPx) = shared.ViewHelpers.GetViewportSize();
        return screenWidthPx / gridRectWidth;
    }


    public void RenderBackground(SpriteBatch spriteBatch, ScrollingBackgroundConfig config)
    {
        // Render land background
        RenderLandBackground(spriteBatch, config);
    }

    private void RenderLandBackground(SpriteBatch spriteBatch, ScrollingBackgroundConfig config)
    {
        var LandTextureGridWidth = config.LandTextureWidthPx / GameConfig.CellTextureSizePx;
        var LandTextureGridHeight = config.LandTextureHeightPx / GameConfig.CellTextureSizePx;

        var (gridRectX, gridY, gridRectWidth, gridRectHeight) = GetGridRectForVisibleLand(config);

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
                    shared.Textures[config.LandTextureName],
                    tileRect,
                    Color.White
                );
            }
        }
    }

}