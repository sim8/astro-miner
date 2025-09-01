using System;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class ScrollingBackgroundRenderer(RendererShared shared)
{
    private const int LandTextureWidthPx = 1600;
    private const int LandTextureHeightPx = 1600;

    private readonly float LandTextureGridWidth = LandTextureWidthPx / GameConfig.CellTextureSizePx * 0.5f;
    private readonly float LandTextureGridHeight = LandTextureHeightPx / GameConfig.CellTextureSizePx * 0.5f;


    public void RenderBackground(SpriteBatch spriteBatch)
    {
        var (startX, startY, viewportGridWidth, viewportGridHeight) = shared.ViewHelpers.GetViewportGridRect();

        var (screenWidthPx, screenHeightPx) = shared.ViewHelpers.GetViewportSize();

        // Calculate pixels per grid unit based on viewport size
        var pixelsPerGridUnit = screenWidthPx / viewportGridWidth;

        // Calculate which background tiles are visible
        // Add padding to ensure we cover the entire screen even during sub-tile movements
        var padding = 1.0f;
        var tilesStartX = (float)Math.Floor((startX - padding) / LandTextureGridWidth) * LandTextureGridWidth;
        var tilesStartY = (float)Math.Floor((startY - padding) / LandTextureGridHeight) * LandTextureGridHeight;
        var tilesEndX = tilesStartX + viewportGridWidth + padding * 2 + LandTextureGridWidth;
        var tilesEndY = tilesStartY + viewportGridHeight + padding * 2 + LandTextureGridHeight;

        // Iterate over all visible background tiles
        for (var tileX = tilesStartX; tileX < tilesEndX; tileX += LandTextureGridWidth)
        {
            for (var tileY = tilesStartY; tileY < tilesEndY; tileY += LandTextureGridHeight)
            {
                // Calculate tile position relative to viewport start
                var relativePosX = (tileX - startX) * pixelsPerGridUnit;
                var relativePosY = (tileY - startY) * pixelsPerGridUnit;

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
}