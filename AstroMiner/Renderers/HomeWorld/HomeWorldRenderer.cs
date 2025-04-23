using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.HomeWorld;

public class HomeWorldRenderer(RendererShared shared) : BaseWorldRenderer(shared)
{
    private readonly GameStateManager _gameStateManager = shared.GameStateManager;

    public override void RenderWorld(SpriteBatch spriteBatch)
    {
        var rect = Shared.ViewHelpers.GetVisibleRectForGridCell(0, 0, Coordinates.Grid.OizusWidth,
            Coordinates.Grid.OizusHeight);
        spriteBatch.Draw(Shared.Textures["oizus-bg"], rect, Color.White);

        if (shared.Game.Debug.showGridDebug) RenderGridDebugOverlay(spriteBatch);
    }

    private void RenderGridDebugOverlay(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < _gameStateManager.HomeWorld.Grid.GetLength(0); row++)
        for (var col = 0; col < _gameStateManager.HomeWorld.Grid.GetLength(1); col++)
        {
            var cellRect = Shared.ViewHelpers.GetVisibleRectForGridCell(col, row);
            if (_gameStateManager.HomeWorld.Grid[row, col] == WorldCellType.Filled)
                spriteBatch.Draw(Shared.Textures["white"], cellRect, Color.Red * 0.5f);
            if (_gameStateManager.HomeWorld.Grid[row, col] == WorldCellType.Portal)
                spriteBatch.Draw(Shared.Textures["white"], cellRect, Color.Green * 0.5f);

            var coordinatesStr = col + " " + row;
            shared.RenderString(spriteBatch, cellRect.X, cellRect.Y, coordinatesStr, 2);
        }
    }
}