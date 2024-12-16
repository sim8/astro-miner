using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class GradientOverlayRenderer(
    Dictionary<string, Texture2D> _textures,
    GameState gameState,
    ViewHelpers _viewHelpers)
{
    private static readonly int TextureSizePx = 256;

    private static (int x, int y) Right = (0, 2);
    private static (int x, int y) Down = (1, 2);
    private static (int x, int y) Left = (2, 2);
    private static (int x, int y) Up = (3, 2);
    private static (int x, int y) UpRight = (0, 1);
    private static (int x, int y) DownRight = (0, 0);
    private static (int x, int y) UpLeft = (1, 1);
    private static (int x, int y) DownLeft = (1, 0);
    private static (int x, int y) UpRightWide = (3, 0);
    private static (int x, int y) DownRightWide = (3, 1);
    private static (int x, int y) UpLeftWide = (2, 0);
    private static (int x, int y) DownLeftWide = (2, 1);
    private static (int x, int y) UpLeftToBottomRight = (4, 0);
    private static (int x, int y) UpRightToBottomLeft = (4, 1);
    private static (int x, int y) Solid = (4, 2);

    private static Rectangle GetSourceRect((int, int) offsetXY)
    {
        return new Rectangle(offsetXY.Item1 * TextureSizePx, offsetXY.Item2 * TextureSizePx, TextureSizePx,
            TextureSizePx);
    }

    public void RenderGradientOverlay(SpriteBatch spriteBatch, int col, int row)
    {
        var cellState = gameState.Grid.GetCellState(col, row);
        if (cellState.distanceToOutsideConnectedFloor > 1 ||
            cellState.distanceToOutsideConnectedFloor == CellState.DISTANCE_UNINITIALIZED) // TODO doesnt read well
        {
            var overlayOpacityMidPoint = 0.6f;
            var overlayColor = new Color(37, 73, 99);

            var overlaySourceRect = GetSourceRect(Up);
            var solidSourceRect = GetSourceRect(Solid);

            if (cellState.distanceToOutsideConnectedFloor == 2)
            {
                spriteBatch.Draw(_textures["gradient-set"], _viewHelpers.GetVisibleRectForGridCell(col, row),
                    overlaySourceRect, overlayColor * overlayOpacityMidPoint);
            }
            else if (cellState.distanceToOutsideConnectedFloor == 3)
            {
                spriteBatch.Draw(_textures["gradient-set"], _viewHelpers.GetVisibleRectForGridCell(col, row),
                    solidSourceRect, overlayColor * overlayOpacityMidPoint);
                spriteBatch.Draw(_textures["gradient-set"], _viewHelpers.GetVisibleRectForGridCell(col, row),
                    overlaySourceRect, overlayColor);
            }
            else
            {
                spriteBatch.Draw(_textures["gradient-set"], _viewHelpers.GetVisibleRectForGridCell(col, row),
                    solidSourceRect, overlayColor);
            }

            // var rect = _viewHelpers.GetVisibleRectForGridCell(col, row);
            // _userInterfaceRenderer.RenderString(spriteBatch, rect.X + 20, rect.Y + 20,
            //     cellState.distanceToOutsideConnectedFloor.ToString());
        }
    }
}