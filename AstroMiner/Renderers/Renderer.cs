using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.Entities;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class Renderer
{
    private readonly Corner[] _cornersInRenderOrder =
        [Corner.TopLeft, Corner.TopRight, Corner.BottomLeft, Corner.BottomRight];

    private readonly DynamiteRenderer _dynamiteRenderer;
    private readonly ExplosionRenderer _explosionRenderer;

    private readonly Color _floorColorCore = new(128, 138, 140);
    private readonly Color _floorColorCrust = new(212, 225, 227);
    private readonly Color _floorColorMantle = new(171, 182, 184);
    private readonly FogOfWarRenderer _fogOfWarRenderer;

    private readonly FrameCounter _frameCounter;
    private readonly GameState _gameState;
    private readonly GraphicsDeviceManager _graphics;
    private readonly Color _lavaLightColor = new(255, 231, 171);
    private readonly MinerRenderer _minerRenderer;
    private readonly BlendState _multiplyBlendState;
    private readonly PlayerRenderer _playerRenderer;
    private readonly ScrollingBackgroundRenderer _scrollingBackgroundRenderer;
    private readonly RendererShared _shared;
    private readonly Dictionary<string, Texture2D> _textures;
    private readonly UserInterfaceRenderer _userInterfaceRenderer;
    private readonly ViewHelpers _viewHelpers;
    private RenderTarget2D _lightingRenderTarget;

    public Renderer(
        GraphicsDeviceManager graphics,
        Dictionary<string, Texture2D> textures,
        GameState gameState,
        FrameCounter frameCounter)
    {
        _shared = new RendererShared(gameState, graphics, textures);
        _gameState = gameState;
        _textures = textures;
        _viewHelpers = new ViewHelpers(gameState, graphics);
        _graphics = graphics;
        _frameCounter = frameCounter;
        _minerRenderer = new MinerRenderer(_shared);
        _playerRenderer = new PlayerRenderer(_shared);
        _dynamiteRenderer = new DynamiteRenderer(_shared);
        _explosionRenderer = new ExplosionRenderer(_shared);
        _userInterfaceRenderer = new UserInterfaceRenderer(_shared);
        _fogOfWarRenderer = new FogOfWarRenderer(_shared);
        _scrollingBackgroundRenderer = new ScrollingBackgroundRenderer(_shared);
        _multiplyBlendState = new BlendState();
        _multiplyBlendState.ColorBlendFunction = BlendFunction.Add;
        _multiplyBlendState.ColorSourceBlend = Blend.DestinationColor;
        _multiplyBlendState.ColorDestinationBlend = Blend.Zero;

        _initializeLightingRenderTarget();
    }

    private void _initializeLightingRenderTarget()
    {
        _lightingRenderTarget?.Dispose();

        var (width, height) = _viewHelpers.GetViewportSize();
        _lightingRenderTarget = new RenderTarget2D(
            _graphics.GraphicsDevice,
            width,
            height,
            false,
            _graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);
    }

    public void HandleWindowResize(object sender, EventArgs e)
    {
        // RenderTarget2d can't change width/height after initialization. Re-init on window resize
        _initializeLightingRenderTarget();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        // Draw RenderTargets first to avoid wiping BackBuffer
        RenderLightingToRenderTarget(spriteBatch);

        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        RenderScene(spriteBatch);
        spriteBatch.End();

        // Multiply lights/shadow with scene
        spriteBatch.Begin(SpriteSortMode.Deferred, _multiplyBlendState, SamplerState.PointClamp);
        var (viewportWidth, viewportHeight) = _viewHelpers.GetViewportSize();
        spriteBatch.Draw(_lightingRenderTarget, new Rectangle(0, 0, viewportWidth, viewportHeight), Color.White);
        spriteBatch.End();

        // Additive lighting pass for glare (explosions, very brights lights)
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp);
        RenderAdditiveLighting(spriteBatch);
        spriteBatch.End();

        // Lastly, draw UI
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        _userInterfaceRenderer.RenderUserInterface(spriteBatch, _frameCounter);
        spriteBatch.End();
    }

    private void RenderScene(SpriteBatch spriteBatch)
    {
        _scrollingBackgroundRenderer.RenderBackground(spriteBatch);

        LoopVisibleCells(
            1, // Account for textures with vertical overlap
            (col, row) =>
            {
                var cellState = _gameState.Asteroid.Grid.GetCellState(col, row);

                foreach (var corner in _cornersInRenderOrder)
                {
                    // Render floor
                    var floorQuadrantSourceRect =
                        Tilesets.GetFloorQuadrantSourceRect(_gameState, col, row, corner);
                    spriteBatch.Draw(_textures["tileset"],
                        _viewHelpers.GetVisibleRectForFloorQuadrant(col, row, corner),
                        floorQuadrantSourceRect,
                        Color.White);

                    // Render wall
                    if (Tilesets.CellIsTilesetType(_gameState, col, row))
                    {
                        var dualTilesetSourceRect =
                            Tilesets.GetWallQuadrantSourceRect(_gameState, col, row, corner);

                        var tintColor = _gameState.Asteroid.Grid.ExplosiveRockCellIsActive(col, row) ? Color.Red :
                            cellState.WallType == WallType.LooseRock ? Color.LightGreen : Color.White;

                        spriteBatch.Draw(_textures["tileset"],
                            _viewHelpers.GetVisibleRectForWallQuadrant(col, row, corner),
                            dualTilesetSourceRect,
                            tintColor);
                    }
                }

                if (_gameState.Asteroid.Grid.GetFloorType(col, row) == FloorType.LavaCracks)
                    spriteBatch.Draw(_textures["cracks"], _viewHelpers.GetVisibleRectForGridCell(col, row),
                        Color.White);
            });

        LoopVisibleCells(FogOfWarRenderer.FogGradientGridRadius,
            (col, row) => { _fogOfWarRenderer.RenderFogOfWar(spriteBatch, col, row); }
        );

        foreach (var entity in _gameState.Asteroid.ActiveEntitiesSortedByDistance)
            if (entity is MinerEntity)
                _minerRenderer.RenderMiner(spriteBatch);
            else if (entity is PlayerEntity)
                _playerRenderer.RenderPlayer(spriteBatch);
            else if (entity is DynamiteEntity dynamiteEntity)
                _dynamiteRenderer.RenderDynamite(spriteBatch, dynamiteEntity);
            else if (entity is ExplosionEntity explosionEntity)
                _explosionRenderer.RenderExplosion(spriteBatch, explosionEntity);
    }

    private void RenderAdditiveLighting(SpriteBatch spriteBatch)
    {
        foreach (var entity in _gameState.Asteroid.ActiveEntitiesSortedByDistance)
            if (entity is ExplosionEntity explosionEntity)
                _explosionRenderer.RenderAdditiveLightSource(spriteBatch, explosionEntity);

        _shared.RenderDirectionalLightSource(spriteBatch, _gameState.Asteroid.Miner.GetDirectionalLightSource(),
            _gameState.Asteroid.Miner.Direction, 192, 0.4f);

        if (!_gameState.Asteroid.IsInMiner)
            _shared.RenderDirectionalLightSource(spriteBatch, _gameState.Asteroid.Player.GetDirectionalLightSource(),
                _gameState.Asteroid.Player.Direction, 128, 0.3f);
    }

    private void RenderLightingToRenderTarget(SpriteBatch spriteBatch)
    {
        _graphics.GraphicsDevice.SetRenderTarget(_lightingRenderTarget);
        _graphics.GraphicsDevice.DepthStencilState = new DepthStencilState { DepthBufferEnable = true };

        _graphics.GraphicsDevice.Clear(Color.White);
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        var (viewportWidth, viewportHeight) = _viewHelpers.GetViewportSize();
        spriteBatch.Draw(_textures["white"], new Rectangle(0, 0, viewportWidth, viewportHeight),
            FogOfWarRenderer.FogColor * 0.8f);


        _shared.RenderDirectionalLightSource(spriteBatch, _gameState.Asteroid.Miner.GetDirectionalLightSource(),
            _gameState.Asteroid.Miner.Direction);

        if (!_gameState.Asteroid.IsInMiner)
            _shared.RenderDirectionalLightSource(spriteBatch, _gameState.Asteroid.Player.GetDirectionalLightSource(),
                _gameState.Asteroid.Player.Direction, 256);

        _shared.RenderRadialLightSource(spriteBatch,
            _gameState.Asteroid.IsInMiner
                ? _gameState.Asteroid.Miner.CenterPosition
                : _gameState.Asteroid.Player.CenterPosition, 512, 0.4f);

        foreach (var entity in _gameState.Asteroid.ActiveEntitiesSortedByDistance)
            if (entity is DynamiteEntity dynamiteEntity)
                _dynamiteRenderer.RenderLightSource(spriteBatch, dynamiteEntity);
            else if (entity is ExplosionEntity explosionEntity)
                _explosionRenderer.RenderLightSource(spriteBatch, explosionEntity);

        // Render any grid-based light sources
        LoopVisibleCells(
            1, // Only lava, small light source
            (col, row) =>
            {
                if (_gameState.Asteroid.Grid.GetFloorType(col, row) == FloorType.Lava)
                {
                    var pos = new Vector2(col + 0.5f, row + 0.5f);
                    _shared.RenderRadialLightSource(spriteBatch, pos, _lavaLightColor, 150, 0.6f);
                }
            });

        // Render overlay gradients in shadow color over lighting to block out light on unexplored cells
        LoopVisibleCells(FogOfWarRenderer.FogGradientGridRadius,
            (col, row) =>
            {
                var cellState = _gameState.Asteroid.Grid.GetCellState(col, row);
                var showOverlay = cellState.DistanceToExploredFloor >= 2 ||
                                  cellState.DistanceToExploredFloor ==
                                  CellState.UninitializedOrAboveMax;
                // At least match FogOpacity to smooth out FOW animation
                var overlayOpacity = showOverlay ? 1f : cellState.FogOpacity;
                if (overlayOpacity > 0f)
                    _fogOfWarRenderer.RenderGradientOverlay(spriteBatch, col, row, 120, overlayOpacity);
            });

        spriteBatch.End();


        _graphics.GraphicsDevice.SetRenderTarget(null);
    }

    private void LoopVisibleCells(int padding, Action<int, int> cellAction)
    {
        var (startCol, startRow, endCol, endRow) = _viewHelpers.GetVisibleGrid(padding);

        for (var row = startRow; row < endRow; row++)
        for (var col = startCol; col < endCol; col++)
            cellAction(col, row);
    }
}