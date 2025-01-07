using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.ProceduralGenViewer;

public class ProceduralGenViewerRenderer
{
    private const int CellSizePx = 5;
    private const int CellBorderPx = 1;

    private readonly Dictionary<AsteroidLayer, Color> _asteroidLayerColors = new()
    {
        { AsteroidLayer.Crust, Color.Yellow },
        { AsteroidLayer.Mantle, Color.Orange },
        { AsteroidLayer.Core, Color.Red }
    };

    private readonly Dictionary<CellType, Color> _cellColors = new()
    {
        { CellType.Diamond, new Color(144, 248, 255) },
        { CellType.Ruby, new Color(200, 0, 0) },
        { CellType.SolidRock, new Color(60, 60, 60) },
        { CellType.LooseRock, new Color(180, 180, 180) },
        { CellType.Rock, new Color(120, 120, 120) },
        { CellType.Empty, new Color(0, 0, 0) },
        { CellType.Floor, new Color(240, 240, 240) },
        { CellType.Lava, Color.Orange },
        { CellType.Nickel, Color.DarkGreen },
        { CellType.Gold, Color.Yellow },
        { CellType.ExplosiveRock, Color.Purple }
    };

    private readonly GameState _gameState;
    private readonly ProceduralGenViewerState _proceduralGenViewerState;


    private readonly Dictionary<string, Texture2D> _textures;

    public ProceduralGenViewerRenderer(
        Dictionary<string, Texture2D> textures,
        GameState gameState, ProceduralGenViewerState proceduralGenViewerState)
    {
        _gameState = gameState;
        _textures = textures;
        _proceduralGenViewerState = proceduralGenViewerState;
    }

    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        RenderScene(spriteBatch);
        RenderString(spriteBatch, 30, 30, "SEED " + _gameState.Seed);
        spriteBatch.End();
    }

    private Rectangle GetGridCellRect(int col, int row)
    {
        return new Rectangle(col * CellSizePx + CellBorderPx, row * CellSizePx + CellBorderPx,
            CellSizePx - CellBorderPx, CellSizePx - CellBorderPx);
    }

    private void RenderScene(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < GameConfig.GridSize; row++)
        for (var col = 0; col < GameConfig.GridSize; col++)
        {
            var cellState = _gameState.Grid.GetCellState(col, row);

            spriteBatch.Draw(_textures["white"], GetGridCellRect(col, row),
                _cellColors[cellState.Type]);

            if (_proceduralGenViewerState.hasToggledView && cellState.Layer != AsteroidLayer.None)
                spriteBatch.Draw(_textures["white"], GetGridCellRect(col, row),
                    _asteroidLayerColors[cellState.Layer] * 0.6f);
        }
    }

    private void RenderString(SpriteBatch spriteBatch, int startX, int startY, string str, int scale = 3)
    {
        var linePxCount = 0;
        foreach (var (x, y, width) in FontHelpers.TransformString(str))
        {
            var sourceRect = new Rectangle(x, y, width, 8);
            var destRect = new Rectangle(startX + linePxCount * scale, startY + 10, width * scale, 8 * scale);
            spriteBatch.Draw(_textures["dogica-font"], destRect, sourceRect, Color.LimeGreen);
            linePxCount += width;
        }
    }
}