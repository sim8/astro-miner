using System;
using AstroMiner.Definitions;
using AstroMiner.Model;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.AsteroidWorld;

public class AsteroidWorldRenderer : BaseWorldRenderer
{
    private readonly Corner[] _cornersInRenderOrder =
        [Corner.TopLeft, Corner.TopRight, Corner.BottomLeft, Corner.BottomRight];

    private readonly FogOfWarRenderer _fogOfWarRenderer;
    private readonly GameStateManager _gameStateManager;
    private readonly Color _lavaLightColor = new(255, 231, 171);

    public AsteroidWorldRenderer(RendererShared shared) : base(shared)
    {
        _gameStateManager = shared.GameStateManager;
        _fogOfWarRenderer = new FogOfWarRenderer(shared);
    }

    public override void RenderWorld(SpriteBatch spriteBatch)
    {
        LoopVisibleCells(
            1, // Account for textures with vertical overlap
            (col, row) =>
            {
                var cellState = _gameStateManager.AsteroidWorld.Grid.GetCellState(col, row);

                if (cellState.FloorType == FloorType.Lava)
                    spriteBatch.Draw(Shared.Textures["lava"],
                        Shared.ViewHelpers.GetVisibleRectForGridCell(col, row), Color.White);

                foreach (var corner in _cornersInRenderOrder)
                {
                    // Render floor
                    var floorQuadrantSourceRect =
                        Tilesets.GetFloorQuadrantSourceRect(Shared.Game, col, row, corner);
                    spriteBatch.Draw(Shared.Textures["tileset"],
                        Shared.ViewHelpers.GetVisibleRectForFloorQuadrant(col, row, corner),
                        floorQuadrantSourceRect,
                        Color.White);

                    // Render wall
                    if (Tilesets.CellIsTilesetType(Shared.Game, col, row))
                    {
                        var dualTilesetSourceRect =
                            Tilesets.GetWallQuadrantSourceRect(Shared.Game, col, row, corner);

                        var tintColor = _gameStateManager.AsteroidWorld.Grid.ExplosiveRockCellIsActive(col, row)
                            ? Color.Red
                            : cellState.WallType == WallType.LooseRock
                                ? Color.LightGreen
                                : Color.White;

                        spriteBatch.Draw(Shared.Textures["tileset"],
                            Shared.ViewHelpers.GetVisibleRectForWallQuadrant(col, row, corner),
                            dualTilesetSourceRect,
                            tintColor);
                    }
                }

                // Render after floor+wall rendering as they're done by quadrant
                RenderLavaCracks(spriteBatch, col, row);
            });

        LoopVisibleCells(FogOfWarRenderer.FogGradientGridRadius,
            (col, row) => { _fogOfWarRenderer.RenderFogOfWar(spriteBatch, col, row); }
        );

        if (Shared.Game.Debug.showGridDebug) RenderGridDebugOverlay(spriteBatch);
    }

    public override void RenderWorldOverlay(SpriteBatch spriteBatch)
    {
        var (viewportWidth, viewportHeight) = Shared.ViewHelpers.GetViewportSize();
        spriteBatch.Draw(Shared.Textures["white"], new Rectangle(0, 0, viewportWidth, viewportHeight),
            FogOfWarRenderer.FogColor * 0.8f);
    }

    public override void RenderWorldLighting(SpriteBatch spriteBatch)
    {
        // Render any grid-based light sources
        LoopVisibleCells(
            1, // Only lava, small light source
            (col, row) =>
            {
                var floorType = _gameStateManager.AsteroidWorld.Grid.GetFloorType(col, row);
                if (FloorTypes.IsLavaLike(floorType))
                {
                    var baseOpacity = 0.6f;
                    if (floorType == FloorType.CollapsingLavaCracks)
                        baseOpacity *= _gameStateManager.AsteroidWorld.Grid.GetCollapsingCompletion(col, row);

                    var pos = new Vector2(col + 0.5f, row + 0.5f);
                    Shared.RenderRadialLightSource(spriteBatch, pos, _lavaLightColor, 150, baseOpacity);
                }
            });

        // Render overlay gradients in shadow color over lighting to block out light on unexplored cells
        LoopVisibleCells(FogOfWarRenderer.FogGradientGridRadius,
            (col, row) =>
            {
                var cellState = _gameStateManager.AsteroidWorld.Grid.GetCellState(col, row);
                var showOverlay = cellState.DistanceToExploredFloor >= 2 ||
                                  cellState.DistanceToExploredFloor ==
                                  CellState.UninitializedOrAboveMax;
                // At least match FogOpacity to smooth out FOW animation
                var overlayOpacity = showOverlay ? 1f : cellState.FogOpacity;
                if (overlayOpacity > 0f)
                    _fogOfWarRenderer.RenderGradientOverlay(spriteBatch, col, row, 120, overlayOpacity);
            });
    }

    private void RenderLavaCracks(SpriteBatch spriteBatch, int col, int row)
    {
        var cellState = _gameStateManager.AsteroidWorld.Grid.GetCellState(col, row);

        // Happens after floor+wall rendering, should never overlay walls
        if (cellState.WallType != WallType.Empty)
            return;

        if (cellState.FloorType == FloorType.LavaCracks)
            spriteBatch.Draw(Shared.Textures["cracks"],
                Shared.ViewHelpers.GetVisibleRectForGridCell(col, row),
                new Rectangle(0, 0, 32, 32),
                Color.White);

        if (cellState.FloorType == FloorType.CollapsingLavaCracks)
        {
            var completionPercentage =
                Shared.Game.StateManager.AsteroidWorld.Grid.GetCollapsingCompletion(col, row);
            var keyframe = (int)(completionPercentage * 4); // 4 keyframes
            var sourceRect = new Rectangle(32 * (keyframe + 1), 0, 32, 36); // +1 because 1st frame is non-collapsing
            spriteBatch.Draw(Shared.Textures["cracks"],
                Shared.ViewHelpers.GetVisibleRectForGridCell(col, row, 1f, 1.125f), // Is 32x36
                sourceRect,
                Color.White);
        }
    }

    public override void RenderShadows(SpriteBatch spriteBatch)
    {
        // Render overlay gradients in shadow color over lighting to block out light on unexplored cells
        LoopVisibleCells(FogOfWarRenderer.FogGradientGridRadius,
            (col, row) =>
            {
                var cellState = _gameStateManager.AsteroidWorld.Grid.GetCellState(col, row);
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
        var (startCol, startRow, endCol, endRow) = Shared.ViewHelpers.GetVisibleGrid(padding);

        for (var row = startRow; row < endRow; row++)
        for (var col = startCol; col < endCol; col++)
            cellAction(col, row);
    }

    private void RenderGridDebugOverlay(SpriteBatch spriteBatch)
    {
        LoopVisibleCells(
            1,
            (col, row) =>
            {
                var cellRect = Shared.ViewHelpers.GetVisibleRectForGridCell(col, row);
                var leftBorderRect = cellRect;
                leftBorderRect.Width = 1;
                spriteBatch.Draw(Shared.Textures["white"], leftBorderRect, Color.Black);

                var topBorderRect = cellRect;
                topBorderRect.Height = 1;
                spriteBatch.Draw(Shared.Textures["white"], topBorderRect, Color.Black);


                var coordinatesStr = col + " " + row;
                Shared.RenderString(spriteBatch, cellRect.X, cellRect.Y, coordinatesStr, 1);
            });
    }
}