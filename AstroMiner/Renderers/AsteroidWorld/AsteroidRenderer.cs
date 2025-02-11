using System;
using AstroMiner.AsteroidWorld;
using AstroMiner.Definitions;
using AstroMiner.Entities;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.AsteroidWorld;

public class AsteroidRenderer
{
    private readonly Corner[] _cornersInRenderOrder =
        [Corner.TopLeft, Corner.TopRight, Corner.BottomLeft, Corner.BottomRight];

    private readonly FogOfWarRenderer _fogOfWarRenderer;

    private readonly GameState _gameState;
    private readonly Color _lavaLightColor = new(255, 231, 171);
    private readonly RendererShared _shared;

    public AsteroidRenderer(
        RendererShared shared)
    {
        _gameState = shared.GameState;
        _shared = shared;
        _fogOfWarRenderer = new FogOfWarRenderer(_shared);
    }

    public void RenderWorld(SpriteBatch spriteBatch)
    {

        LoopVisibleCells(
            1, // Account for textures with vertical overlap
            (col, row) =>
            {
                var cellState = _gameState.AsteroidWorld.Grid.GetCellState(col, row);

                foreach (var corner in _cornersInRenderOrder)
                {
                    // Render floor
                    var floorQuadrantSourceRect =
                        Tilesets.GetFloorQuadrantSourceRect(_gameState, col, row, corner);
                    spriteBatch.Draw(_shared.Textures["tileset"],
                        _shared.ViewHelpers.GetVisibleRectForFloorQuadrant(col, row, corner),
                        floorQuadrantSourceRect,
                        Color.White);

                    // Render wall
                    if (Tilesets.CellIsTilesetType(_gameState, col, row))
                    {
                        var dualTilesetSourceRect =
                            Tilesets.GetWallQuadrantSourceRect(_gameState, col, row, corner);

                        var tintColor = _gameState.AsteroidWorld.Grid.ExplosiveRockCellIsActive(col, row) ? Color.Red :
                            cellState.WallType == WallType.LooseRock ? Color.LightGreen : Color.White;

                        spriteBatch.Draw(_shared.Textures["tileset"],
                            _shared.ViewHelpers.GetVisibleRectForWallQuadrant(col, row, corner),
                            dualTilesetSourceRect,
                            tintColor);
                    }
                }

                if (_gameState.AsteroidWorld.Grid.GetFloorType(col, row) == FloorType.LavaCracks)
                    spriteBatch.Draw(_shared.Textures["cracks"],
                        _shared.ViewHelpers.GetVisibleRectForGridCell(col, row),
                        Color.White);
            });

        LoopVisibleCells(FogOfWarRenderer.FogGradientGridRadius,
            (col, row) => { _fogOfWarRenderer.RenderFogOfWar(spriteBatch, col, row); }
        );
    }

    public void RenderWorldOverlay(SpriteBatch spriteBatch)
    {
        var (viewportWidth, viewportHeight) = _shared.ViewHelpers.GetViewportSize();
        spriteBatch.Draw(_shared.Textures["white"], new Rectangle(0, 0, viewportWidth, viewportHeight),
            FogOfWarRenderer.FogColor * 0.8f);
    }

    public void RenderWorldLighting(SpriteBatch spriteBatch)
    {
        // Render any grid-based light sources
        LoopVisibleCells(
            1, // Only lava, small light source
            (col, row) =>
            {
                if (_gameState.AsteroidWorld.Grid.GetFloorType(col, row) == FloorType.Lava)
                {
                    var pos = new Vector2(col + 0.5f, row + 0.5f);
                    _shared.RenderRadialLightSource(spriteBatch, pos, _lavaLightColor, 150, 0.6f);
                }
            });

        // Render overlay gradients in shadow color over lighting to block out light on unexplored cells
        LoopVisibleCells(FogOfWarRenderer.FogGradientGridRadius,
            (col, row) =>
            {
                var cellState = _gameState.AsteroidWorld.Grid.GetCellState(col, row);
                var showOverlay = cellState.DistanceToExploredFloor >= 2 ||
                                  cellState.DistanceToExploredFloor ==
                                  CellState.UninitializedOrAboveMax;
                // At least match FogOpacity to smooth out FOW animation
                var overlayOpacity = showOverlay ? 1f : cellState.FogOpacity;
                if (overlayOpacity > 0f)
                    _fogOfWarRenderer.RenderGradientOverlay(spriteBatch, col, row, 120, overlayOpacity);
            });
    }

    public void RenderShadows(SpriteBatch spriteBatch)
    {
        // Render overlay gradients in shadow color over lighting to block out light on unexplored cells
        LoopVisibleCells(FogOfWarRenderer.FogGradientGridRadius,
            (col, row) =>
            {
                var cellState = _gameState.AsteroidWorld.Grid.GetCellState(col, row);
                var showOverlay = cellState.DistanceToExploredFloor >= 2 ||
                                  cellState.DistanceToExploredFloor ==
                                  CellState.UninitializedOrAboveMax;
                // At least match FogOpacity to smooth out FOW animation
                var overlayOpacity = showOverlay ? 1f : cellState.FogOpacity;
                if (overlayOpacity > 0f)
                    _fogOfWarRenderer.RenderGradientOverlay(spriteBatch, col, row, 120, overlayOpacity);
            });
    }

    // TODO move to renderer shared?
    private void LoopVisibleCells(int padding, Action<int, int> cellAction)
    {
        var (startCol, startRow, endCol, endRow) = _shared.ViewHelpers.GetVisibleGrid(padding);

        for (var row = startRow; row < endRow; row++)
            for (var col = startCol; col < endCol; col++)
                cellAction(col, row);
    }
}