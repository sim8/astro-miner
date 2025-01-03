using System.Collections.Generic;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.Utilities;

/// <summary>
///     Based heavily on Dual Grid System https://x.com/OskSta/status/1448248658865049605
///     High level steps of rendering:
///     1. Iterate each cell (back to front) and each corner of the cell (back to front)
///     2. Find the tile to render based on corner's 3 neighbors
///     3. Render single quadrant (corner of cell)
///     - Only quadrant rendered as opposed to whole tile, as neighboring cell
///     might be different type (but shares based rock design)
///     - Each quadrant rendered two quadrants high, leaving room for overlaying
///     texture at the top
/// </summary>
public static class Tilesets
{
    private const int QuadrantTextureSizePx = GameConfig.CellTextureSizePx / 2;

    private const int TextureGridWidth = 4;

    // Define coordinates for each
    private static readonly Dictionary<int, (int, int)> RampKeyToTextureOffset = new()
    {
        // Row 1
        { RampKeys.UpRightWide, (0, 0) },
        { RampKeys.Left, (1, 0) },
        { RampKeys.UpRight, (2, 0) },
        { RampKeys.Up, (3, 0) },

        // Row 2
        { RampKeys.UpRightToBottomLeft, (0, 1) },
        { RampKeys.UpLeft, (1, 1) },
        { RampKeys.Empty, (2, 1) },
        { RampKeys.DownRight, (3, 1) },

        // Row 3
        { RampKeys.DownLeftWide, (0, 2) },
        { RampKeys.Down, (1, 2) },
        { RampKeys.DownLeft, (2, 2) },
        { RampKeys.Right, (3, 2) },

        // Row 4
        { RampKeys.Solid, (0, 3) },
        { RampKeys.UpLeftWide, (1, 3) },
        { RampKeys.UpLeftToBottomRight, (2, 3) },
        { RampKeys.DownRightWide, (3, 3) }
    };

    private static readonly Dictionary<CellType, int> WallTypeTextureIndex = new()
    {
        { CellType.Rock, 0 },
        { CellType.SolidRock, 1 },
        { CellType.Ruby, 2 },
        { CellType.Diamond, 3 },
        { CellType.Gold, 4 },
        { CellType.Nickel, 5 },
        { CellType.ExplosiveRock, 6 }
    };


    // A given corner is the center of a 2x2 set of tiles - use this to find the top left of each set
    private static readonly Dictionary<Corner, (int, int)> GetTopLeftOffsetFor2X2 = new()
    {
        { Corner.TopLeft, (-1, -1) },
        { Corner.TopRight, (0, -1) },
        { Corner.BottomLeft, (-1, 0) },
        { Corner.BottomRight, (0, 0) }
    };

    public static bool CellIsTilesetType(GameState gameState, int x, int y)
    {
        return WallTypeTextureIndex.ContainsKey(gameState.Grid.GetCellType(x, y));
    }

    // Find the tile to render based on corner's 3 neighbors
    private static int GetCellQuadrantTileKey(GameState gameState, int x, int y, Corner corner)
    {
        var (topLeftXOffset, topLeftYOffset) = GetTopLeftOffsetFor2X2[corner];

        var twoByTwoX = x + topLeftXOffset;
        var twoByTwoY = y + topLeftYOffset;

        var isTopLeftTileset = CellIsTilesetType(gameState, twoByTwoX, twoByTwoY);
        var isTopRightTileset = CellIsTilesetType(gameState, twoByTwoX + 1, twoByTwoY);
        var isBottomLeftTileset = CellIsTilesetType(gameState, twoByTwoX, twoByTwoY + 1);
        var isBottomRightTileset = CellIsTilesetType(gameState, twoByTwoX + 1, twoByTwoY + 1);

        return RampKeys.CreateKey(isTopLeftTileset, isTopRightTileset, isBottomLeftTileset,
            isBottomRightTileset);
    }

    // Find px offset within main texture for a given quadrant
    // TODO save this to CellState
    private static (int, int) GetCellQuadrantTextureOffset(GameState gameState, int col, int row, Corner corner)
    {
        // Walls tileset has one quadrant's space above each actual quadrant for overlaying texture.
        // Each quadrant is rendered at double height, overlaying the one behind it
        // // TODO - change/centralize this logic? Will need doing for floor tilesets

        // For the cell quadrant, work out which tile to use
        var tileKey = GetCellQuadrantTileKey(gameState, col, row, corner);

        // Get the grid x,y of that tile within the default dual tileset
        var (textureGridX, textureGridY) = RampKeyToTextureOffset[tileKey];

        // Get grid index of cellType tileset within main texture
        var cellType = gameState.Grid.GetCellType(col, row);
        var textureTilesetX = WallTypeTextureIndex[cellType] * TextureGridWidth;

        // Convert those to pixel x,y within the actual texture
        // NOTE y pos accounts for texture overlay space. Logic will need to change for floor tilesets
        var tileTexturePxX = (textureTilesetX + textureGridX) * GameConfig.CellTextureSizePx;
        var tileTexturePxY = textureGridY * QuadrantTextureSizePx * 4; // Each tile in texture is 4 quadrants high

        var quadrantX = tileTexturePxX +
                        (corner is Corner.TopLeft or Corner.BottomLeft ? QuadrantTextureSizePx : 0);
        var quadrantY = tileTexturePxY +
                        (corner is Corner.TopLeft or Corner.TopRight ? QuadrantTextureSizePx * 2 : 0);


        return (quadrantX, quadrantY);
    }

    public static Rectangle GetCellQuadrantSourceRect(GameState gameState, int col, int row, Corner corner)
    {
        var (x, y) = GetCellQuadrantTextureOffset(gameState, col, row, corner);

        // Each tile quadrant has one quadrant above it in texture for any overlaying visuals. Render at double height
        // TODO - change/centralize this logic? Will need doing for floor tilesets
        return new Rectangle(x, y, QuadrantTextureSizePx, GameConfig.CellTextureSizePx);
    }
}