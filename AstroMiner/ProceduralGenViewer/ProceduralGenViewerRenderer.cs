using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.ProceduralGenViewer;

public class ProceduralGenViewerRenderer(BaseGame game, ProceduralGenViewerState proceduralGenViewerState)
{
    private const int CellSizePx = 5;
    private const int CellBorderPx = 1;

    private readonly Dictionary<AsteroidLayer, Color> _asteroidLayerColors = new()
    {
        { AsteroidLayer.Crust, Color.Yellow },
        { AsteroidLayer.Mantle, Color.Orange },
        { AsteroidLayer.Core, Color.Red }
    };

    private readonly Dictionary<FloorType, Color> _floorColors = new()
    {
        { FloorType.Floor, Color.White },
        { FloorType.Lava, Color.Orange },
        { FloorType.LavaCracks, Color.Salmon }
    };

    private readonly Dictionary<WallType, Color> _wallColors = new()
    {
        { WallType.Diamond, new Color(144, 248, 255) },
        { WallType.Ruby, new Color(200, 0, 0) },
        { WallType.SolidRock, new Color(60, 60, 60) },
        { WallType.LooseRock, new Color(180, 180, 180) },
        { WallType.Rock, new Color(120, 120, 120) },
        { WallType.Nickel, Color.DarkGreen },
        { WallType.Gold, Color.Yellow },
        { WallType.ExplosiveRock, Color.Purple }
    };

    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        RenderScene(spriteBatch);
        RenderString(spriteBatch, 30, 30, "SEED " + game.Model.Asteroid.Seed);
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
                var cellState = game.StateManager.AsteroidWorld.Grid.GetCellState(col, row);

                if (cellState.FloorType != FloorType.Empty)
                    spriteBatch.Draw(game.Textures[Tx.White], GetGridCellRect(col, row),
                        _floorColors[cellState.FloorType]);

                if (proceduralGenViewerState.showWalls && cellState.WallType != WallType.Empty)
                    spriteBatch.Draw(game.Textures[Tx.White], GetGridCellRect(col, row),
                        _wallColors[cellState.WallType]);

                if (proceduralGenViewerState.showLayers && cellState.Layer != AsteroidLayer.None)
                    spriteBatch.Draw(game.Textures[Tx.White], GetGridCellRect(col, row),
                        _asteroidLayerColors[cellState.Layer] * 0.6f);
            }

        RenderStartPos(spriteBatch, game.Model.Asteroid.MinerStartingPos);
    }

    private void RenderStartPos(SpriteBatch spriteBatch, Vector2 startPos)
    {
        var x = (int)(startPos.X * CellSizePx);
        var y = (int)(startPos.Y * CellSizePx);

        spriteBatch.Draw(game.Textures[Tx.White], new Rectangle(x, y, 6, 6), Color.Aqua);
        spriteBatch.Draw(game.Textures[Tx.White], new Rectangle(x - 6, y - 6, 6, 6), Color.Aqua);
    }

    private void RenderString(SpriteBatch spriteBatch, int startX, int startY, string str, int scale = 3)
    {
        var linePxCount = 0;
        foreach (var (x, y, width, height) in FontHelpers.TransformString(str))
        {
            var sourceRect = new Rectangle(x, y, width, 8);
            var destRect = new Rectangle(startX + linePxCount * scale, startY + 10, width * scale, height * scale);
            spriteBatch.Draw(game.Textures[Tx.DogicaFont], destRect, sourceRect, Color.LimeGreen);
            linePxCount += width;
        }
    }
}