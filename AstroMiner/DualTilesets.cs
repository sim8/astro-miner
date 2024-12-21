using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public static class DualTilesets
{
    private const int TilesetTextureGridWidth = 4;
    private const int TilesetTextureGridHeight = 4;
    private static readonly (int, int)[] Offsets;
    public static readonly Dictionary<CellType, int> OffsetsWithinMainTexture;
    public static readonly Dictionary<Corner, (int, int)> neighboursToCheck;
    public static readonly Dictionary<int, (int, int)> CornerKeyToTextureOffset;
    public static readonly HashSet<CellType> TilesetCellTypes;
    private static readonly int _quadrantTextureSizePx = GameConfig.CellTextureSizePx / 2;

    // A given corner is the center of a 2x2 set of tiles - use this to find the top left of each set
    private static readonly Dictionary<Corner, (int, int)> GetTopLeftOffsetFor2x2 = new()
    {
        { Corner.TopLeft, (-1, -1) },
        { Corner.TopRight, (0, -1) },
        { Corner.BottomLeft, (-1, 0) },
        { Corner.BottomRight, (0, 0) }
    };

    static DualTilesets()
    {
        OffsetsWithinMainTexture = new Dictionary<CellType, int>
        {
            { CellType.Rock, 0 },
            { CellType.SolidRock, 1 },
            { CellType.Ruby, 2 },
            { CellType.Diamond, 3 }
        };

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

        TilesetCellTypes = new HashSet<CellType>
        {
            CellType.Rock, CellType.SolidRock, CellType.Ruby, CellType.Diamond
        };
    }

    private static bool CellIsTilesetType(GameState gameState, int x, int y)
    {
        return TilesetCellTypes.Contains(gameState.Grid.GetCellType(x, y));
    }

    private static int GetCellQuadrantTileKey(GameState gameState, int x, int y, Corner corner)
    {
        var (topLeftXOffset, topLeftYOffset) = GetTopLeftOffsetFor2x2[corner];

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
        // // TODO - change/centralize this logic? Will need doing for floor tilesets
        var topTileQuadrantsTextureHeight = GameConfig.CellTextureSizePx;

        // For the cell quadrant, work out which tile to use
        var tileKey = GetCellQuadrantTileKey(gameState, col, row, corner);

        // Get the grid x,y of that tile within the default dual tileset
        var (textureGridX, textureGridY) = CornerKeyToTextureOffset[tileKey];

        // Convert those to pixel x,y within the actual texture
        // NOTE y pos accounts for texture overlay space. Logic will need to change for floor tilesets
        var tileTexturePxX = textureGridX * GameConfig.CellTextureSizePx;
        var tileTexturePxY = textureGridY * (GameConfig.CellTextureSizePx + _quadrantTextureSizePx);

        var quadrantX = tileTexturePxX +
                        (corner is Corner.TopLeft or Corner.BottomLeft ? _quadrantTextureSizePx : 0);
        var quadrantY = tileTexturePxY +
                        (corner is Corner.TopLeft or Corner.TopRight ? topTileQuadrantsTextureHeight : 0);


        return (quadrantX, quadrantY);
    }

    public static Rectangle GetCellQuadrantSourceRect(GameState gameState, int col, int row, Corner corner)
    {
        var (x, y) = GetCellQuadrantTextureOffset(gameState, col, row, corner);

        // Walls tileset has one quadrant room above each tile for overlaying texture.
        // Check bottom instead of top as corners are opposite between tiles + cells!
        // TODO - change/centralize this logic? Will need doing for floor tilesets
        var height = corner is Corner.BottomLeft or Corner.BottomRight
            ? GameConfig.CellTextureSizePx
            : _quadrantTextureSizePx;

        return new Rectangle(x, y, _quadrantTextureSizePx, height);
    }
}