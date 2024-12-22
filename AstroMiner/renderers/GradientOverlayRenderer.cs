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
        RampKeys.DownLeft, RampKeys.Down, RampKeys.DownRight
    ];

    private readonly (int, int) _downLeftWideWithOffset = (5, 1);

    private readonly (int, int) _downRightWideWithOffset = (5, 0);

    private readonly Dictionary<int, (int x, int y)> _gradientKeysToOffsets = new()
    {
        { RampKeys.Right, (0, 2) },
        { RampKeys.Down, (1, 2) },
        { RampKeys.Left, (2, 2) },
        { RampKeys.Up, (3, 2) },
        { RampKeys.UpRight, (0, 1) },
        { RampKeys.DownRight, (0, 0) },
        { RampKeys.UpLeft, (1, 1) },
        { RampKeys.DownLeft, (1, 0) },
        { RampKeys.UpRightWide, (3, 0) },
        { RampKeys.DownRightWide, (3, 1) },
        { RampKeys.UpLeftWide, (2, 0) },
        { RampKeys.DownLeftWide, (2, 1) },
        { RampKeys.UpLeftToBottomRight, (4, 0) },
        { RampKeys.UpRightToBottomLeft, (4, 2) },
        { RampKeys.Empty, (5, 2) },
        { RampKeys.Solid, (4, 2) }
    };

    private Rectangle GetSourceRect(int gradientKey, bool isInnerGradient)
    {
        var textureOffset = _gradientKeysToOffsets[gradientKey];

        // Top edge gradients are rendered at reduced height to avoid overlaying ground.
        // For smooth gradient, swap out downWideLeft/Right for versions which smoothly join to shorter "down" gradient
        if (isInnerGradient && gradientKey == RampKeys.DownLeftWide) textureOffset = _downLeftWideWithOffset;
        if (isInnerGradient && gradientKey == RampKeys.DownRightWide) textureOffset = _downRightWideWithOffset;

        return new Rectangle(textureOffset.Item1 * TextureSizePx, textureOffset.Item2 * TextureSizePx, TextureSizePx,
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

            var gradientKey = cellState.gradientKey;

            var isInnerGradient = cellState.distanceToOutsideConnectedFloor == innerGradientDepth;

            var overlaySourceRect = GetSourceRect(gradientKey, isInnerGradient);
            var solidSourceRect = GetSourceRect(RampKeys.Solid, isInnerGradient);

            if (isInnerGradient)
            {
                if (gradientKey > 0)
                {
                    var isTopEdgeOfOverlay = TopEdgeKeys.Contains(gradientKey);
                    spriteBatch.Draw(shared.Textures["gradient-set"],
                        GetVisibleRectForGradientOverlay(col, row, isTopEdgeOfOverlay),
                        overlaySourceRect, OverlayColor * overlayOpacityMidPoint);
                }
            }
            else if (cellState.distanceToOutsideConnectedFloor == outerGradientDepth)
            {
                spriteBatch.Draw(shared.Textures["gradient-set"],
                    GetVisibleRectForGradientOverlay(col, row),
                    solidSourceRect, OverlayColor * overlayOpacityMidPoint);
                if (gradientKey > 0)
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