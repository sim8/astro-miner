using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class GradientOverlayRenderer(RendererShared shared)
{
    private static readonly int TextureSizePx = 256;
    public static readonly Color OverlayColor = new(37, 73, 99);

    private static readonly HashSet<int> TopEdgeKeys =
    [
        CornerKeys.DownLeft, CornerKeys.DownLeftWide, CornerKeys.Down, CornerKeys.DownRight,
        CornerKeys.DownRightWide
    ];

    private readonly Dictionary<int, (int x, int y)> _gradientKeysToOffsets = new()
    {
        { CornerKeys.Right, (0, 2) },
        { CornerKeys.Down, (1, 2) },
        { CornerKeys.Left, (2, 2) },
        { CornerKeys.Up, (3, 2) },
        { CornerKeys.UpRight, (0, 1) },
        { CornerKeys.DownRight, (0, 0) },
        { CornerKeys.UpLeft, (1, 1) },
        { CornerKeys.DownLeft, (1, 0) },
        { CornerKeys.UpRightWide, (3, 0) },
        { CornerKeys.DownRightWide, (3, 1) },
        { CornerKeys.UpLeftWide, (2, 0) },
        { CornerKeys.DownLeftWide, (2, 1) },
        { CornerKeys.UpLeftToBottomRight, (4, 0) },
        { CornerKeys.UpRightToBottomLeft, (4, 2) },
        { CornerKeys.Empty, (4, 2) },
        { CornerKeys.Solid, (7, 2) }
    };

    private Rectangle GetSourceRect(int gradientKey)
    {
        var offsetXY = _gradientKeysToOffsets[gradientKey];
        return new Rectangle(offsetXY.Item1 * TextureSizePx, offsetXY.Item2 * TextureSizePx, TextureSizePx,
            TextureSizePx);
    }

    // Offset gradient overlay to appear higher up
    // Top edge gradients are made smaller to not overlay ground (unwanted shadow)
    private Rectangle GetVisibleRectForGradientOverlay(float col, float row, bool isTopEdgeOfOverlay = false)
    {
        var gridY = isTopEdgeOfOverlay ? row : row - 0.5f;
        var height = isTopEdgeOfOverlay ? 0.5f : 1f;
        return shared.ViewHelpers.GetVisibleRectForGridCell(col, gridY, 1f, height);
    }

    // Two tiers of gradients. Values have to be consecutive and are constrained by MaxUnexploredCellsVisible
    public void RenderGradientOverlay(SpriteBatch spriteBatch, int col, int row,
        int innerGradientDepth = 2, int outerGradientDepth = 3)
    {
        var cellState = shared.GameState.Grid.GetCellState(col, row);
        if (cellState.distanceToOutsideConnectedFloor >= innerGradientDepth ||
            cellState.distanceToOutsideConnectedFloor ==
            CellState.DISTANCE_UNINITIALIZED_OR_ABOVE_MAX) // Cell above max, always render overlay
        {
            var overlayOpacityMidPoint = 0.6f;

            var overlaySourceRect = GetSourceRect(cellState.gradientKey);
            var solidSourceRect = GetSourceRect(CornerKeys.InitialKey);

            if (cellState.distanceToOutsideConnectedFloor == innerGradientDepth)
            {
                if (cellState.gradientKey > 0)
                {
                    var isTopEdgeOfOverlay = TopEdgeKeys.Contains(cellState.gradientKey);
                    spriteBatch.Draw(shared.Textures["gradient-set"],
                        GetVisibleRectForGradientOverlay(col, row, isTopEdgeOfOverlay),
                        overlaySourceRect, OverlayColor * overlayOpacityMidPoint);
                    // GetVisibleRectForGradientOverlay(col, row, isTopEdgeOfOverlay),
                    // overlaySourceRect, Color.Red);
                }
            }
            else if (cellState.distanceToOutsideConnectedFloor == outerGradientDepth)
            {
                spriteBatch.Draw(shared.Textures["gradient-set"],
                    GetVisibleRectForGradientOverlay(col, row),
                    solidSourceRect, OverlayColor * overlayOpacityMidPoint);
                if (cellState.gradientKey > 0)
                    spriteBatch.Draw(shared.Textures["gradient-set"],
                        GetVisibleRectForGradientOverlay(col, row),
                        overlaySourceRect, OverlayColor);
            }
            else
            {
                spriteBatch.Draw(shared.Textures["gradient-set"],
                    GetVisibleRectForGradientOverlay(col, row),
                    solidSourceRect, OverlayColor);
            }
        }

        // RenderGradientDebug(spriteBatch, col, row);
    }

    public void RenderGradientDebug(SpriteBatch spriteBatch, int col, int row)
    {
        var cellState = shared.GameState.Grid.GetCellState(col, row);
        var rect = shared.ViewHelpers.GetVisibleRectForGridCell(col, row);
        if (cellState.distanceToOutsideConnectedFloor > 1)
            shared.RenderString(spriteBatch, rect.X + 20, rect.Y,
                cellState.distanceToOutsideConnectedFloor.ToString());

        shared.RenderString(spriteBatch, rect.X, rect.Y + 25,
            "X" + col + " Y" + row, 1);
    }
}