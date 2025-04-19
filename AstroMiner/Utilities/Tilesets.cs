using System.Collections.Generic;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.Utilities;

/// <summary>
///     Based heavily on Dual Grid System https://x.com/OskSta/status/1448248658865049605
///     High level steps of rendering:
///     1. Iterate each cell (back to front) and each corner of the cell (back to front)
///     2. Find the tile to render based on corner's 3 neighbors
///     3. Render single floor + wall quadrant (corner of cell)
///     - Only quadrant rendered as opposed to whole tile, as neighboring cell
///     might be different type (but shares based rock design)
///     - Each wall quadrant rendered two quadrants high, leaving room for overlaying
///     texture at the top
/// </summary>
public static class Tilesets
{
    private const int QuadrantTextureSizePx = GameConfig.CellTextureSizePx / 2;

    private const int TextureGridWidth = 4;
    private const int WallTextureGridHeight = 8;

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

    private static readonly Dictionary<WallType, int> WallTypeTextureIndex = new()
    {
        { WallType.Rock, 0 },
        { WallType.LooseRock, 0 },
        { WallType.SolidRock, 1 },
        { WallType.Ruby, 2 },
        { WallType.Diamond, 3 },
        { WallType.Gold, 4 },
        { WallType.Nickel, 5 },
        { WallType.ExplosiveRock, 6 }
    };


    // A given corner is the center of a 2x2 set of tiles - use this to find the top left of each set
    private static readonly Dictionary<Corner, (int, int)> GetTopLeftOffsetFor2X2 = new()
    {
        { Corner.TopLeft, (-1, -1) },
        { Corner.TopRight, (0, -1) },
        { Corner.BottomLeft, (-1, 0) },
        { Corner.BottomRight, (0, 0) }
    };

    public static bool CellIsTilesetType(AstroMinerGame game, int x, int y)
    {
        var wallType = game.State.AsteroidWorld.Grid.GetWallType(x, y);
        return WallTypeTextureIndex.ContainsKey(wallType);
    }

    // Find the tile to render based on corner's 3 neighbors
    private static int GetWallQuadrantTileKey(AstroMinerGame game, int x, int y, Corner corner)
    {
        var (topLeftXOffset, topLeftYOffset) = GetTopLeftOffsetFor2X2[corner];

        var twoByTwoX = x + topLeftXOffset;
        var twoByTwoY = y + topLeftYOffset;

        var isTopLeftTileset = CellIsTilesetType(game, twoByTwoX, twoByTwoY);
        var isTopRightTileset = CellIsTilesetType(game, twoByTwoX + 1, twoByTwoY);
        var isBottomLeftTileset = CellIsTilesetType(game, twoByTwoX, twoByTwoY + 1);
        var isBottomRightTileset = CellIsTilesetType(game, twoByTwoX + 1, twoByTwoY + 1);

        return RampKeys.CreateKey(isTopLeftTileset, isTopRightTileset, isBottomLeftTileset,
            isBottomRightTileset);
    }

    // Find the tile to render based on corner's 3 neighbors + return texture index within Tileset.png
    private static (int, int) GetFloorQuadrantTileKeyAndTextureIndex(AstroMinerGame game, int x, int y, Corner corner)
    {
        var (topLeftXOffset, topLeftYOffset) = GetTopLeftOffsetFor2X2[corner];

        var twoByTwoX = x + topLeftXOffset;
        var twoByTwoY = y + topLeftYOffset;

        var topLeft = game.State.AsteroidWorld.Grid.GetFloorType(twoByTwoX, twoByTwoY);
        var topRight = game.State.AsteroidWorld.Grid.GetFloorType(twoByTwoX + 1, twoByTwoY);
        var bottomLeft = game.State.AsteroidWorld.Grid.GetFloorType(twoByTwoX, twoByTwoY + 1);
        var bottomRight = game.State.AsteroidWorld.Grid.GetFloorType(twoByTwoX + 1, twoByTwoY + 1);

        // Dual grid system doesn't normally allow for > 2 textures adjoining. Deriving the texture to use from the neighbors.
        // When adding more floor types will likely need a priority order for textures + try and ensure they don't adjoin
        var textureOffset = topLeft == FloorType.Lava || topRight == FloorType.Lava || bottomLeft == FloorType.Lava ||
                            bottomRight == FloorType.Lava
            ? 1
            : 0;

        return (RampKeys.CreateKey( // Needs to take cracks into account. Floor-like?
            !FloorTypes.IsFloorLikeTileset(topLeft),
            !FloorTypes.IsFloorLikeTileset(topRight),
            !FloorTypes.IsFloorLikeTileset(bottomLeft),
            !FloorTypes.IsFloorLikeTileset(bottomRight)
        ), textureOffset);
    }

    // High level steps
    // - get tile key (floor == empty on main tileset)
    // - choose texture based on if any neighbors are lava

    // Find px offset within main texture for a given quadrant
    // TODO save this to CellState
    private static (int, int) GetWallQuadrantTextureOffset(AstroMinerGame game, int col, int row, Corner corner)
    {
        // Walls tileset has one quadrant's space above each actual quadrant for overlaying texture.
        // Each quadrant is rendered at double height, overlaying the one behind it
        // // TODO - change/centralize this logic? Will need doing for floor tilesets

        // For the cell quadrant, work out which tile to use
        var tileKey = GetWallQuadrantTileKey(game, col, row, corner);

        // Get the grid x,y of that tile within the default dual tileset
        var (textureGridX, textureGridY) = RampKeyToTextureOffset[tileKey];

        // Get grid index of cellType tileset within main texture
        var wallType = game.State.AsteroidWorld.Grid.GetWallType(col, row);

        var textureTilesetX = WallTypeTextureIndex[wallType] * TextureGridWidth;

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

    private static (int, int) GetFloorQuadrantTextureOffset(AstroMinerGame game, int col, int row, Corner corner)
    {
        // Walls tileset has one quadrant's space above each actual quadrant for overlaying texture.
        // Each quadrant is rendered at double height, overlaying the one behind it
        // // TODO - change/centralize this logic? Will need doing for floor tilesets

        // For the cell quadrant, work out which tile to use
        var (tileKey, textureIndex) = GetFloorQuadrantTileKeyAndTextureIndex(game, col, row, corner);

        // Get the grid x,y of that tile within the default dual tileset
        var (textureGridX, textureGridY) = RampKeyToTextureOffset[tileKey];

        var textureTilesetX = textureIndex * TextureGridWidth;

        // Convert those to pixel x,y within the actual texture
        var tileTexturePxX = (textureTilesetX + textureGridX) * GameConfig.CellTextureSizePx;
        var tileTexturePxY = (WallTextureGridHeight + textureGridY) * GameConfig.CellTextureSizePx;

        var quadrantX = tileTexturePxX +
                        (corner is Corner.TopLeft or Corner.BottomLeft ? QuadrantTextureSizePx : 0);
        var quadrantY = tileTexturePxY +
                        (corner is Corner.TopLeft or Corner.TopRight ? QuadrantTextureSizePx : 0);

        return (quadrantX, quadrantY);
    }

    public static Rectangle GetWallQuadrantSourceRect(AstroMinerGame game, int col, int row, Corner corner)
    {
        var (x, y) = GetWallQuadrantTextureOffset(game, col, row, corner);

        // Each tile quadrant has one quadrant above it in texture for any overlaying visuals. Render at double height
        return new Rectangle(x, y, QuadrantTextureSizePx, QuadrantTextureSizePx * 2);
    }

    public static Rectangle GetFloorQuadrantSourceRect(AstroMinerGame game, int col, int row, Corner corner)
    {
        var (x, y) = GetFloorQuadrantTextureOffset(game, col, row, corner);
        return new Rectangle(x, y, QuadrantTextureSizePx, QuadrantTextureSizePx);
    }
}