using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class FogOfWarRenderer(RendererShared shared)
{
    public static readonly Color FogColor = new(37, 73, 99);
    public static readonly int FogGradientGridRadius = 2; // Roughly

    public void RenderFogOfWar(SpriteBatch spriteBatch, int col, int row)
    {
        var cellState = shared.GameState.Asteroid.Grid.GetCellState(col, row);

        if (!cellState.isEmpty && cellState.FogOpacity > 0f)
            RenderGradientOverlay(spriteBatch, col, row, 160, cellState.FogOpacity);
    }

    public void RenderGradientOverlay(SpriteBatch spriteBatch, int col, int row,
        int gradientSize, float opacity = 1f)
    {
        var pos = new Vector2(col + 0.5f, row + 0.5f);
        shared.RenderRadialLightSource(spriteBatch, pos, FogColor * opacity, gradientSize);
    }
}