using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class FogOfWarRenderer(RendererShared shared)
{
    public static readonly Color FogColor = new(37, 73, 99);
    public static readonly int FogGradientGridRadius = 2; // Roughly

    public void RenderFogOfWar(SpriteBatch spriteBatch, int col, int row, int gradientSize = 160)
    {
        var cellState = shared.GameState.Grid.GetCellState(col, row);

        if (!cellState.isEmpty && cellState.FogOpacity > 0f)
        {
            var pos = new Vector2(col + 0.5f, row + 0.5f);
            var fogColorWithAlpha = FogColor * cellState.FogOpacity;
            shared.RenderRadialLightSource(spriteBatch, pos, fogColorWithAlpha, gradientSize);
        }
    }
}