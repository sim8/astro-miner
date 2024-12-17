using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class GradientOverlayRenderer(RendererShared shared)
{
    private static readonly int TextureSizePx = 256;
    public static readonly Color OverlayColor = new(37, 73, 99);

    private readonly Dictionary<int, (int x, int y)> _gradientKeysToOffsets = new()
    {
        {
            // right
            GradientKeyHelpers.CreateKey(Corner.TopRight, Corner.BottomRight), (0, 2)
        },
        {
            // down
            GradientKeyHelpers.CreateKey(Corner.BottomLeft, Corner.BottomRight), (1, 2)
        },
        {
            // left
            GradientKeyHelpers.CreateKey(Corner.TopLeft, Corner.BottomLeft), (2, 2)
        },
        {
            // up
            GradientKeyHelpers.CreateKey(Corner.TopRight, Corner.TopLeft), (3, 2)
        },
        {
            // up right
            GradientKeyHelpers.CreateKey(Corner.TopRight), (0, 1)
        },
        {
            // down right
            GradientKeyHelpers.CreateKey(Corner.BottomRight), (0, 0)
        },
        {
            // up left
            GradientKeyHelpers.CreateKey(Corner.TopLeft), (1, 1)
        },
        {
            // down left
            GradientKeyHelpers.CreateKey(Corner.BottomLeft), (1, 0)
        },
        {
            // up right wide
            GradientKeyHelpers.CreateKey(Corner.TopRight, Corner.BottomRight, Corner.TopLeft), (3, 0)
        },
        {
            // down right wide
            GradientKeyHelpers.CreateKey(Corner.TopRight, Corner.BottomRight, Corner.BottomLeft), (3, 1)
        },
        {
            // up left wide
            GradientKeyHelpers.CreateKey(Corner.TopRight, Corner.BottomLeft, Corner.TopLeft), (2, 0)
        },
        {
            // down left wide
            GradientKeyHelpers.CreateKey(Corner.TopLeft, Corner.BottomLeft, Corner.BottomRight), (2, 1)
        },
        {
            // up left to bottom right
            GradientKeyHelpers.CreateKey(Corner.TopLeft, Corner.BottomRight), (4, 0)
        },
        {
            // up right to bottom left
            GradientKeyHelpers.CreateKey(Corner.TopRight, Corner.BottomLeft), (4, 2)
        },
        {
            // solid
            GradientKeyHelpers.CreateKey(), (4, 2)
        },
        {
            // TODO REMOVE. Shouldnt happen in game
            GradientKeyHelpers.CreateKey(Corner.TopRight, Corner.BottomLeft, Corner.BottomRight, Corner.TopLeft),
            (7, 2)
        }
    };

    private Rectangle GetSourceRect(int gradientKey)
    {
        var offsetXY = _gradientKeysToOffsets[gradientKey];
        return new Rectangle(offsetXY.Item1 * TextureSizePx, offsetXY.Item2 * TextureSizePx, TextureSizePx,
            TextureSizePx);
    }

    // Two tiers of gradients. Values have to be consecutive and are constrained by MaxUnexploredCellsVisible
    public void RenderGradientOverlay(SpriteBatch spriteBatch, int col, int row, UserInterfaceRenderer removeMe,
        int innerGradientDepth = 2, int outerGradientDepth = 3)
    {
        var cellState = shared.GameState.Grid.GetCellState(col, row);
        if (cellState.distanceToOutsideConnectedFloor >= innerGradientDepth ||
            cellState.distanceToOutsideConnectedFloor ==
            CellState.DISTANCE_UNINITIALIZED_OR_ABOVE_MAX) // Cell above max, always render overlay
        {
            var overlayOpacityMidPoint = 0.6f;

            var overlaySourceRect = GetSourceRect(cellState.gradientKey);
            var solidSourceRect = GetSourceRect(GradientKeyHelpers.InitialKey);

            if (cellState.distanceToOutsideConnectedFloor == innerGradientDepth)
            {
                if (cellState.gradientKey > 0)
                    spriteBatch.Draw(shared.Textures["gradient-set"],
                        shared.ViewHelpers.GetVisibleRectForGridCell(col, row),
                        overlaySourceRect, OverlayColor * overlayOpacityMidPoint);
            }
            else if (cellState.distanceToOutsideConnectedFloor == outerGradientDepth)
            {
                spriteBatch.Draw(shared.Textures["gradient-set"],
                    shared.ViewHelpers.GetVisibleRectForGridCell(col, row),
                    solidSourceRect, OverlayColor * overlayOpacityMidPoint);
                if (cellState.gradientKey > 0)
                    spriteBatch.Draw(shared.Textures["gradient-set"],
                        shared.ViewHelpers.GetVisibleRectForGridCell(col, row),
                        overlaySourceRect, OverlayColor);
            }
            else
            {
                spriteBatch.Draw(shared.Textures["gradient-set"],
                    shared.ViewHelpers.GetVisibleRectForGridCell(col, row),
                    solidSourceRect, OverlayColor);
            }
        }

        // RenderGradientDebug(spriteBatch, col, row, removeMe);
    }

    public void RenderGradientDebug(SpriteBatch spriteBatch, int col, int row, UserInterfaceRenderer removeMe)
    {
        var cellState = shared.GameState.Grid.GetCellState(col, row);
        if (cellState.distanceToOutsideConnectedFloor > 1)

        {
            var rect = shared.ViewHelpers.GetVisibleRectForGridCell(col, row);
            removeMe.RenderString(spriteBatch, rect.X + 20, rect.Y,
                cellState.distanceToOutsideConnectedFloor.ToString());
            removeMe.RenderString(spriteBatch, rect.X, rect.Y + 25,
                "X" + col + " Y" + row, 1);
        }
    }
}