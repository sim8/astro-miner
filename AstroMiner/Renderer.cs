using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class Renderer
{
    private readonly MinerRenderer _minerRenderer;
    private readonly MiningState _miningState;
    private readonly Dictionary<string, Texture2D> _textures;
    private readonly ViewHelpers _viewHelpers;

    public Renderer(
        GraphicsDeviceManager graphics,
        Dictionary<string, Texture2D> textures,
        MiningState miningState)
    {
        _miningState = miningState;
        _textures = textures;
        _viewHelpers = new ViewHelpers(miningState, graphics);
        _minerRenderer = new MinerRenderer(textures, _miningState, _viewHelpers);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < MiningState.GridSize; row++)
        for (var col = 0; col < MiningState.GridSize; col++)
        {
            var cellState = _miningState.GetCellState(col, row);
            if (Tilesets.TilesetTextureNames.TryGetValue(cellState, out var name))
            {
                var offset = Tilesets.GetTileCoords(_miningState, col, row);
                var tilesetSourceRect = new Rectangle(offset.Item1 * GameConfig.CellTextureSizePx,
                    offset.Item2 * GameConfig.CellTextureSizePx,
                    GameConfig.CellTextureSizePx, GameConfig.CellTextureSizePx);
                spriteBatch.Draw(_textures[name], _viewHelpers.GetVisibleRectForGridCell(col, row),
                    tilesetSourceRect, Color.White);
                if (offset == (1, 2)) // Top piece
                {
                    var overlayOffset = (5, 2);
                    var overlaySourceRect = new Rectangle(overlayOffset.Item1 * GameConfig.CellTextureSizePx,
                        overlayOffset.Item2 * GameConfig.CellTextureSizePx,
                        GameConfig.CellTextureSizePx, GameConfig.CellTextureSizePx);
                    var overlayOpacity = HasFloorWithinTwoTiles(col, row) ? 0.8f : 1;
                    spriteBatch.Draw(_textures[name], _viewHelpers.GetVisibleRectForGridCell(col, row),
                        overlaySourceRect, Color.White * overlayOpacity);
                }
            }
            else if (_miningState.GetCellState(col, row) == CellState.Floor)
            {
                var tilesetSourceRect = new Rectangle(3 * GameConfig.CellTextureSizePx,
                    GameConfig.CellTextureSizePx,
                    GameConfig.CellTextureSizePx, GameConfig.CellTextureSizePx);
                spriteBatch.Draw(_textures["rock-tileset"], _viewHelpers.GetVisibleRectForGridCell(col, row),
                    tilesetSourceRect,
                    Color.White);
            }
        }

        _minerRenderer.RenderMiner(spriteBatch);
    }

    private bool HasFloorWithinTwoTiles(int col, int row)
    {
        for (var x = col - 1; x <= col + 1; x++)
            if ((_miningState.IsValidGridPosition(x, row - 2) &&
                 _miningState.GetCellState(x, row - 2) == CellState.Floor) ||
                (_miningState.IsValidGridPosition(x, row + 2) &&
                 _miningState.GetCellState(x, row + 2) == CellState.Floor))
                return true;
        for (var y = row - 1; y <= row + 1; y++)
            if ((_miningState.IsValidGridPosition(col + 2, y) &&
                 _miningState.GetCellState(col + 2, y) == CellState.Floor) ||
                (_miningState.IsValidGridPosition(col - 2, y) &&
                 _miningState.GetCellState(col - 2, y) == CellState.Floor))
                return true;
        return false;
    }
}