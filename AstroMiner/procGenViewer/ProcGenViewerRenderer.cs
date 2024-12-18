using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class ProcGenViewerRenderer
{
    private const int CellSizePx = 10;
    private const int CellBorderPx = 1;

    private readonly Dictionary<CellType, Color> _cellColors = new()
    {
        { CellType.Diamond, new Color(144, 248, 255) },
        { CellType.Ruby, new Color(200, 0, 0) },
        { CellType.SolidRock, new Color(100, 100, 100) },
        { CellType.Rock, new Color(160, 160, 160) },
        { CellType.Empty, new Color(0, 0, 0) },
        { CellType.Floor, new Color(240, 240, 240) }
    };

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

    private Rectangle GetGridCellRect(int col, int row)
    {
        return new Rectangle(col * CellSizePx + CellBorderPx, row * CellSizePx + CellBorderPx,
            CellSizePx - CellBorderPx * 2, CellSizePx - CellBorderPx * 2);
    }

    private void RenderScene(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < GameConfig.GridSize; row++)
        for (var col = 0; col < GameConfig.GridSize; col++)
        {
            var cellState = _gameState.Grid.GetCellState(col, row);

            spriteBatch.Draw(_textures["white"], GetGridCellRect(col, row),
                _cellColors[cellState.type]);
        }
    }
}