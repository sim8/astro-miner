using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.World;

public class WorldRenderer(RendererShared shared)
{
    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        RenderScene(spriteBatch);
        spriteBatch.End();
    }

    public void RenderScene(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < shared.GameState.World.Grid.GetLength(0); row++)
        for (var col = 0; col < shared.GameState.World.Grid.GetLength(1); col++)
            if (shared.GameState.World.Grid[row, col] == WorldCellType.Filled)
                spriteBatch.Draw(shared.Textures["white"],
                    shared.ViewHelpers.GetVisibleRectForGridCell(col, row),
                    Color.White);
    }
}