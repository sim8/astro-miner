using AstroMiner.Definitions;
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
        }

        if (shared.Game.Debug.showGridDebug) RenderGridDebugOverlay(spriteBatch);
    }

    private void RenderGridDebugOverlay(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < _gameStateManager.StaticWorld.Grid.GetLength(0); row++)
        for (var col = 0; col < _gameStateManager.StaticWorld.Grid.GetLength(1); col++)
        {
            var cellRect = Shared.ViewHelpers.GetVisibleRectForGridCell(col, row);
            if (_gameStateManager.StaticWorld.Grid[row, col] == WorldCellType.Filled)
                spriteBatch.Draw(Shared.Textures["white"], cellRect, Color.Red * 0.5f);
            if (_gameStateManager.StaticWorld.Grid[row, col] == WorldCellType.Portal)
                spriteBatch.Draw(Shared.Textures["white"], cellRect, Color.Green * 0.5f);

            var leftBorderRect = cellRect;
            leftBorderRect.Width = 1;
            spriteBatch.Draw(Shared.Textures["white"], leftBorderRect, Color.Black);

            var topBorderRect = cellRect;
            topBorderRect.Height = 1;
            spriteBatch.Draw(Shared.Textures["white"], topBorderRect, Color.Black);


            var coordinatesStr = col + " " + row;
            shared.RenderString(spriteBatch, cellRect.X, cellRect.Y, coordinatesStr, 2);
        }
    }
}