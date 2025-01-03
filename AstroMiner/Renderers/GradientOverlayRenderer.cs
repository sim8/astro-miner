using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class GradientOverlayRenderer(RendererShared shared)
{
    public static readonly Color OverlayColor = new(37, 73, 99);

    public void RenderGradientOverlay(SpriteBatch spriteBatch, int col, int row,
        int showGradientsAtDistance = 4, int gradientSize = 160)
    {
        var cellState = shared.GameState.Grid.GetCellState(col, row);

        if (cellState.Type == CellType.Empty) return;

        if (cellState.DistanceToExploredFloor >= showGradientsAtDistance ||
            cellState.DistanceToExploredFloor ==
            CellState.UninitializedOrAboveMax)
        {
            var pos = new Vector2(col + 0.5f, row + 0.5f);
            shared.RenderRadialLightSource(spriteBatch, pos, OverlayColor, gradientSize);
        }
    }
}