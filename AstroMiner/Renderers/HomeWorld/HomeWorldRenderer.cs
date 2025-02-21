using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.HomeWorld;

public class HomeWorldRenderer : BaseWorldRenderer
{
    private readonly GameState _gameState;

    public HomeWorldRenderer(RendererShared shared) : base(shared)
    {
        _gameState = shared.GameState;
    }

    public override void RenderWorld(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Shared.Textures["oizus-bg"], Shared.ViewHelpers.GetVisibleRectForGridCell(0, 0, 20, 20), Color.White);
        // RenderDebugCollisionMap(spriteBatch);
    }

    private void RenderDebugCollisionMap(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < _gameState.HomeWorld.Grid.GetLength(0); row++)
            for (var col = 0; col < _gameState.HomeWorld.Grid.GetLength(1); col++)
                if (_gameState.HomeWorld.Grid[row, col] == WorldCellType.Filled)
                    spriteBatch.Draw(Shared.Textures["white"],
                        Shared.ViewHelpers.GetVisibleRectForGridCell(col, row),
                        Color.Red * 0.5f);
    }
}