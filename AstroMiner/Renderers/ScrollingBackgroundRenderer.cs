using System;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class ScrollingBackgroundRenderer(RendererShared shared)
{


    private (float, float, float, float) GetGridRectForVisible(ScrollingBackgroundLayer layer)
    {
        var cameraPos = shared.ViewHelpers.GetCameraPos();


        var offsetXForParallax = cameraPos.X * layer.ParallaxFactorX - cameraPos.X;
        var offsetYForParallax = cameraPos.Y * layer.ParallaxFactorY - cameraPos.Y;

        var movedGridDistance = (float)(shared.GameStateManager.GameTime.TotalGameTime.TotalMilliseconds / 1000f * layer.Speed);

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
        // Render each layer in order
        foreach (var layer in config.Layers)
        {
            RenderBackground(spriteBatch, layer);
        }
    }

    private void RenderBackground(SpriteBatch spriteBatch, ScrollingBackgroundLayer layer)
    {
        var TextureGridWidth = layer.TextureWidthPx / GameConfig.CellTextureSizePx;
        var TextureGridHeight = layer.TextureHeightPx / GameConfig.CellTextureSizePx;

        var (gridRectX, gridY, gridRectWidth, gridRectHeight) = GetGridRectForVisible(layer);

        // Calculate pixels per grid unit based on viewport size
        var pixelsPerGridUnit = GetPixelsPerGridUnit(gridRectWidth);

        // Calculate which background tiles are visible
        // Add padding to ensure we cover the entire screen even during sub-tile movements
        var padding = 1.0f;
        var tilesStartX = (float)Math.Floor((gridRectX - padding) / TextureGridWidth) * TextureGridWidth;
        var tilesStartY = (float)Math.Floor((gridY - padding) / TextureGridHeight) * TextureGridHeight;
        var tilesEndX = tilesStartX + gridRectWidth + padding * 2 + TextureGridWidth;
        var tilesEndY = tilesStartY + gridRectHeight + padding * 2 + TextureGridHeight;

        // Iterate over all visible background tiles
        for (var tileX = tilesStartX; tileX < tilesEndX; tileX += TextureGridWidth)
        {
            for (var tileY = tilesStartY; tileY < tilesEndY; tileY += TextureGridHeight)
            {
                // Calculate tile position relative to viewport start
                var relativePosX = (tileX - gridRectX) * pixelsPerGridUnit;
                var relativePosY = (tileY - gridY) * pixelsPerGridUnit;

                // Calculate tile size in pixels
                var tileSizeX = TextureGridWidth * pixelsPerGridUnit;
                var tileSizeY = TextureGridHeight * pixelsPerGridUnit;

                // Create the rectangle for this tile
                var tileRect = new Rectangle(
                    shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(relativePosX),
                    shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(relativePosY),
                    shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(tileSizeX),
                    shared.ViewHelpers.ConvertToRenderedPxValue_CAUTION(tileSizeY)
                );

                // Draw the tile
                spriteBatch.Draw(
                    shared.Textures[layer.TextureName],
                    tileRect,
                    Color.White
                );
            }
        }
    }

}