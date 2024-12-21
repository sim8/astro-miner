using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public static class DualTilesets
{
    private const int QuadrantTextureSizePx = GameConfig.CellTextureSizePx / 2;
    private static readonly Dictionary<int, (int, int)> CornerKeyToTextureOffset;
    private static readonly HashSet<CellType> TilesetCellTypes;

    // A given corner is the center of a 2x2 set of tiles - use this to find the top left of each set
    private static readonly Dictionary<Corner, (int, int)> GetTopLeftOffsetFor2X2 = new()
    {
        { Corner.TopLeft, (-1, -1) },
        { Corner.TopRight, (0, -1) },
        { Corner.BottomLeft, (-1, 0) },
        { Corner.BottomRight, (0, 0) }
    };

    static DualTilesets()
    {
        CornerKeyToTextureOffset = new Dictionary<int, (int, int)>
        {
            // Row 1
            {
                CornerKeyHelpers.CreateKey(true, true, false, true), (0, 0)
            },
            {
                CornerKeyHelpers.CreateKey(true, false, true, false), (1, 0)
            },
            {
                CornerKeyHelpers.CreateKey(false, true, false, false), (2, 0)
            },
            {
                CornerKeyHelpers.CreateKey(true, true, false, false), (3, 0)
            },
            // Row 2
            {
                CornerKeyHelpers.CreateKey(false, true, true, false), (0, 1)
            },
            {
                CornerKeyHelpers.CreateKey(true, false, false, false), (1, 1)
            },
            {
                CornerKeyHelpers.CreateKey(false, false, false, false), (2, 1)
            },
            {
                CornerKeyHelpers.CreateKey(false, false, false, true), (3, 1)
            },
            // Row 3
            {
                CornerKeyHelpers.CreateKey(true, false, true, true), (0, 2)
            },
            {
                CornerKeyHelpers.CreateKey(false, false, true, true), (1, 2)
            },
            {
                CornerKeyHelpers.CreateKey(false, false, true, false), (2, 2)
            },
            {
                CornerKeyHelpers.CreateKey(false, true, false, true), (3, 2)
            },
            // Row 4
            {
                CornerKeyHelpers.CreateKey(true, true, true, true), (0, 3)
            },
            {
                CornerKeyHelpers.CreateKey(true, true, true, false), (1, 3)
            },
            {
                CornerKeyHelpers.CreateKey(true, false, false, true), (2, 3)
            },
            {
                CornerKeyHelpers.CreateKey(false, true, true, true), (3, 3)
            }
        };

        TilesetCellTypes = [CellType.Rock, CellType.SolidRock, CellType.Ruby, CellType.Diamond];
    }

    private static bool CellIsTilesetType(GameState gameState, int x, int y)
    {
        return TilesetCellTypes.Contains(gameState.Grid.GetCellType(x, y));
    }

    private static int GetCellQuadrantTileKey(GameState gameState, int x, int y, Corner corner)
    {
        var (topLeftXOffset, topLeftYOffset) = GetTopLeftOffsetFor2X2[corner];

        var twoByTwoX = x + topLeftXOffset;
        var twoByTwoY = y + topLeftYOffset;

        var isTopLeftTileset = CellIsTilesetType(gameState, twoByTwoX, twoByTwoY);
        var isTopRightTileset = CellIsTilesetType(gameState, twoByTwoX + 1, twoByTwoY);
        var isBottomLeftTileset = CellIsTilesetType(gameState, twoByTwoX, twoByTwoY + 1);
        var isBottomRightTileset = CellIsTilesetType(gameState, twoByTwoX + 1, twoByTwoY + 1);

        return CornerKeyHelpers.CreateKey(isTopLeftTileset, isTopRightTileset, isBottomLeftTileset,
            isBottomRightTileset);
    }

    // TODO save this to CellState
    private static (int, int) GetCellQuadrantTextureOffset(GameState gameState, int col, int row, Corner corner)
    {
        // Walls tileset has one quadrant room above each tile for overlaying texture.
        // Each quadrant is rendered at double height, overlaying the one behind it
        // // TODO - change/centralize this logic? Will need doing for floor tilesets

        // For the cell quadrant, work out which tile to use
        var tileKey = GetCellQuadrantTileKey(gameState, col, row, corner);

        // Get the grid x,y of that tile within the default dual tileset
        var (textureGridX, textureGridY) = CornerKeyToTextureOffset[tileKey];

        // Convert those to pixel x,y within the actual texture
        // NOTE y pos accounts for texture overlay space. Logic will need to change for floor tilesets
        var tileTexturePxX = textureGridX * GameConfig.CellTextureSizePx;
        var tileTexturePxY = textureGridY * (GameConfig.CellTextureSizePx + QuadrantTextureSizePx);

        var quadrantX = tileTexturePxX +
                        (corner is Corner.TopLeft or Corner.BottomLeft ? QuadrantTextureSizePx : 0);
        var quadrantY = tileTexturePxY +
                        (corner is Corner.TopLeft or Corner.TopRight ? QuadrantTextureSizePx : 0);


        return (quadrantX, quadrantY);
    }

    public static Rectangle GetCellQuadrantSourceRect(GameState gameState, int col, int row, Corner corner)
    {
        var (x, y) = GetCellQuadrantTextureOffset(gameState, col, row, corner);

        // Walls tileset has one quadrant room above each tile for overlaying texture.
        // Each quadrant is rendered at double height, overlaying the one behind it
        // TODO - change/centralize this logic? Will need doing for floor tilesets
        return new Rectangle(x, y, QuadrantTextureSizePx, GameConfig.CellTextureSizePx);
    }
}