using System;
using AstroMiner.Definitions;
using AstroMiner.Renderers.AsteroidWorld;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.StaticWorld;

public class StaticWorldRenderer(RendererShared shared) : BaseWorldRenderer(shared)
{
    private readonly GameStateManager _gameStateManager = shared.GameStateManager;

    public override void RenderWorld(SpriteBatch spriteBatch)
    {
        if (StaticWorlds.StaticWorldConfigs.TryGetValue(shared.Game.Model.ActiveWorld, out var config))
        {
            var destRect = Shared.ViewHelpers.GetVisibleRectForGridCell(0, 0, config.GridWidth, config.GridHeight);
            var sourceRect = new Rectangle(0, config.TextureGridYOffset * 32, config.GridWidth * 32,
                config.GridHeight * 32);
            spriteBatch.Draw(Shared.Textures[config.TexureName],
                destRect,
                sourceRect,
                Color.White);

            // Fill outer gaps here
            FillBackgroundGaps(spriteBatch, destRect);
        }

        if (shared.Game.Debug.showGridDebug) RenderGridDebugOverlay(spriteBatch);
    }

    public override void RenderWorldOverlay(SpriteBatch spriteBatch)
    {
        if (shared.Game.Model.ActiveWorld == World.ShipUpstairs || shared.Game.Model.ActiveWorld == World.ShipDownstairs)
        {
            if (StaticWorlds.StaticWorldConfigs.TryGetValue(shared.Game.Model.ActiveWorld, out var config))
            {
                var destRect = Shared.ViewHelpers.GetVisibleRectForGridCell(0, 0, config.GridWidth, config.GridHeight);
                var sourceRect = new Rectangle(0, config.TextureGridYOffset * 32, config.GridWidth * 32,
                    config.GridHeight * 32);
                spriteBatch.Draw(Shared.Textures[Tx.ShipShadowMap],
                    destRect,
                    sourceRect,
                    FogOfWarRenderer.FogColor * 0.4f);
            }
        }
    }

    private void RenderGridDebugOverlay(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < _gameStateManager.StaticWorld.Grid.GetLength(0); row++)
            for (var col = 0; col < _gameStateManager.StaticWorld.Grid.GetLength(1); col++)
            {
                var cellRect = Shared.ViewHelpers.GetVisibleRectForGridCell(col, row);
                if (_gameStateManager.StaticWorld.Grid[row, col] == WorldCellType.Filled)
                    spriteBatch.Draw(Shared.Textures[Tx.White], cellRect, Color.Red * 0.5f);
                if (_gameStateManager.StaticWorld.Grid[row, col] == WorldCellType.Portal)
                    spriteBatch.Draw(Shared.Textures[Tx.White], cellRect, Color.Green * 0.5f);

                var leftBorderRect = cellRect;
                leftBorderRect.Width = 1;
                spriteBatch.Draw(Shared.Textures[Tx.White], leftBorderRect, Color.Black);

                var topBorderRect = cellRect;
                topBorderRect.Height = 1;
                spriteBatch.Draw(Shared.Textures[Tx.White], topBorderRect, Color.Black);


                var coordinatesStr = col + " " + row;
                shared.RenderString(spriteBatch, cellRect.X, cellRect.Y, coordinatesStr, 2);
            }
    }

    private void FillBackgroundGaps(SpriteBatch spriteBatch, Rectangle mainBackgroundRect)
    {
        var (viewportWidth, viewportHeight) = Shared.ViewHelpers.GetViewportSize();
        var fillColor = Colors.VeryDarkBlue;

        // Top gap
        if (mainBackgroundRect.Top > 0)
        {
            var topGapRect = new Rectangle(0, 0, viewportWidth, mainBackgroundRect.Top);
            spriteBatch.Draw(Shared.Textures[Tx.White], topGapRect, fillColor);
        }

        // Bottom gap
        if (mainBackgroundRect.Bottom < viewportHeight)
        {
            var bottomGapRect = new Rectangle(0, mainBackgroundRect.Bottom, viewportWidth, viewportHeight - mainBackgroundRect.Bottom);
            spriteBatch.Draw(Shared.Textures[Tx.White], bottomGapRect, fillColor);
        }

        // Left gap
        if (mainBackgroundRect.Left > 0)
        {
            var leftGapRect = new Rectangle(0, Math.Max(0, mainBackgroundRect.Top), mainBackgroundRect.Left,
                Math.Min(viewportHeight, mainBackgroundRect.Bottom) - Math.Max(0, mainBackgroundRect.Top));
            spriteBatch.Draw(Shared.Textures[Tx.White], leftGapRect, fillColor);
        }

        // Right gap  
        if (mainBackgroundRect.Right < viewportWidth)
        {
            var rightGapRect = new Rectangle(mainBackgroundRect.Right, Math.Max(0, mainBackgroundRect.Top),
                viewportWidth - mainBackgroundRect.Right,
                Math.Min(viewportHeight, mainBackgroundRect.Bottom) - Math.Max(0, mainBackgroundRect.Top));
            spriteBatch.Draw(Shared.Textures[Tx.White], rightGapRect, fillColor);
        }
    }
}