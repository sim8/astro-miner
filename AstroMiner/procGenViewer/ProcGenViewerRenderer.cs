using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class ProcGenViewerRenderer
{
    private const int CellSize = 10;
    private readonly GameState _gameState;


    private readonly Dictionary<string, Texture2D> _textures;

    public ProcGenViewerRenderer(
        Dictionary<string, Texture2D> textures,
        GameState gameState)
    {
        _gameState = gameState;
        _textures = textures;
    }

    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        RenderScene(spriteBatch);
        spriteBatch.End();
    }

    public Rectangle GetGridCellRect(int col, int row)
    {
        return new Rectangle(col * CellSize, row * CellSize, CellSize, CellSize);
    }

    private void RenderScene(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < GameConfig.GridSize; row++)
        for (var col = 0; col < GameConfig.GridSize; col++)
        {
            var cellState = _gameState.Grid.GetCellState(col, row);
            if (Tilesets.TilesetTextureNames.TryGetValue(cellState.type, out var name))
            {
                var offset = Tilesets.GetTileCoords(_gameState, col, row);
                var tilesetSourceRect = new Rectangle(offset.Item1 * GameConfig.CellTextureSizePx,
                    offset.Item2 * GameConfig.CellTextureSizePx,
                    GameConfig.CellTextureSizePx, GameConfig.CellTextureSizePx);
                spriteBatch.Draw(_textures[name], GetGridCellRect(col, row),
                    tilesetSourceRect, Color.White);
                if (cellState.hasLavaWell)
                    spriteBatch.Draw(_textures["radial-light"], GetGridCellRect(col, row),
                        Color.Yellow);
            }
            else if (cellState.type == CellType.Floor)
            {
                var tilesetSourceRect = new Rectangle(3 * GameConfig.CellTextureSizePx,
                    GameConfig.CellTextureSizePx,
                    GameConfig.CellTextureSizePx, GameConfig.CellTextureSizePx);

                spriteBatch.Draw(_textures["rock-tileset"], GetGridCellRect(col, row),
                    tilesetSourceRect,
                    Color.White);
            }
            else
            {
                var tilesetSourceRect = new Rectangle(3 * GameConfig.CellTextureSizePx,
                    GameConfig.CellTextureSizePx,
                    GameConfig.CellTextureSizePx, GameConfig.CellTextureSizePx);
                spriteBatch.Draw(_textures["rock-tileset"], GetGridCellRect(col, row),
                    tilesetSourceRect,
                    Color.Black);
            }
        }
    }
}