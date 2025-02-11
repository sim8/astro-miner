using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.HomeWorld;

public class HomeWorldRenderer
{
    private readonly RendererShared _shared;

    public HomeWorldRenderer(RendererShared shared)
    {
        _shared = shared;
    }

    public void RenderWorld(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < _shared.GameState.HomeWorld.Grid.GetLength(0); row++)
            for (var col = 0; col < _shared.GameState.HomeWorld.Grid.GetLength(1); col++)
                if (_shared.GameState.HomeWorld.Grid[row, col] == WorldCellType.Filled)
                    spriteBatch.Draw(_shared.Textures["white"],
                        _shared.ViewHelpers.GetVisibleRectForGridCell(col, row),
                        Color.White);
    }
}