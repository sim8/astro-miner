using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public static class DualTilesets
{
    private const int QuadrantTextureSizePx = GameConfig.CellTextureSizePx / 2;

    private static readonly Dictionary<int, (int, int)> CornerKeyToTextureOffset = new()
    {
        // Row 1
        { CornerKeys.UpRightWide, (0, 0) }, // (true, true, false, true)
        { CornerKeys.Left, (1, 0) }, // (true, false, true, false)
        { CornerKeys.UpRight, (2, 0) }, // (false, true, false, false)
        { CornerKeys.Up, (3, 0) }, // (true, true, false, false)

        // Row 2
        { CornerKeys.UpRightToBottomLeft, (0, 1) }, // (false, true, true, false)
        { CornerKeys.UpLeft, (1, 1) }, // (true, false, false, false)
        { CornerKeys.Empty, (2, 1) }, // (false, false, false, false)
        { CornerKeys.DownRight, (3, 1) }, // (false, false, false, true)

        // Row 3
        { CornerKeys.DownLeftWide, (0, 2) }, // (true, false, true, true) 
        { CornerKeys.Down, (1, 2) }, // (false, false, true, true)
        { CornerKeys.DownLeft, (2, 2) }, // (false, false, true, false)
        { CornerKeys.Right, (3, 2) },
        // Row 4
        { CornerKeys.Solid, (0, 3) }, // all corners
        { CornerKeys.UpLeftWide, (1, 3) }, // top-left, top-right, bottom-left
        { CornerKeys.UpLeftToBottomRight, (2, 3) }, // (true, false, false, true)
        { CornerKeys.DownRightWide, (3, 3) }
    };

    private static readonly HashSet<CellType> TilesetCellTypes =
        [CellType.Rock, CellType.SolidRock, CellType.Ruby, CellType.Diamond];

    // A given corner is the center of a 2x2 set of tiles - use this to find the top left of each set
    private static readonly Dictionary<Corner, (int, int)> GetTopLeftOffsetFor2X2 = new()
    {
        { Corner.TopLeft, (-1, -1) },
        { Corner.TopRight, (0, -1) },
        { Corner.BottomLeft, (-1, 0) },
        { Corner.BottomRight, (0, 0) }
    };

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

        return CornerKeys.CreateKey(isTopLeftTileset, isTopRightTileset, isBottomLeftTileset,
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