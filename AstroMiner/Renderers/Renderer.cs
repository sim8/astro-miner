using System;
using System.Collections.Generic;
using AstroMiner.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class Renderer
{
    private readonly Corner[] _cornersInRenderOrder =
        [Corner.TopLeft, Corner.TopRight, Corner.BottomLeft, Corner.BottomRight];

    private readonly DynamiteRenderer _dynamiteRenderer;
    private readonly ExplosionRenderer _explosionRenderer;
    private readonly Color _floorColor = new(212, 225, 227);
    private readonly FrameCounter _frameCounter;
    private readonly GameState _gameState;
    private readonly GradientOverlayRenderer _gradientOverlayRenderer;
    private readonly GraphicsDeviceManager _graphics;
    private readonly Color _lavaLightColor = new(255, 231, 171);
    private readonly MinerRenderer _minerRenderer;
    private readonly BlendState _multiplyBlendState;
    private readonly PlayerRenderer _playerRenderer;
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
        _gradientOverlayRenderer = new GradientOverlayRenderer(_shared);
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
        for (var row = 0; row < GameConfig.GridSize; row++)
        for (var col = 0; col < GameConfig.GridSize; col++)
            if (Tilesets.CellIsTilesetType(_gameState, col, row))
                foreach (var corner in _cornersInRenderOrder)
                {
                    var dualTilesetSourceRect =
                        Tilesets.GetCellQuadrantSourceRect(_gameState, col, row, corner);
                    spriteBatch.Draw(_textures["tileset"],
                        _viewHelpers.GetVisibleRectForGridQuadrant(col, row, corner),
                        dualTilesetSourceRect,
                        _gameState.Grid.ExplosiveRockCellIsActive(col, row) ? Color.Red : Color.White);
                }
            else if (_gameState.Grid.GetCellType(col, row) == CellType.Floor)
                spriteBatch.Draw(_textures["white"], _viewHelpers.GetVisibleRectForGridCell(col, row),
                    _floorColor);
            else if (_gameState.Grid.GetCellType(col, row) == CellType.Lava)
                spriteBatch.Draw(_textures["white"], _viewHelpers.GetVisibleRectForGridCell(col, row),
                    Color.Orange);

        for (var row = 0; row < GameConfig.GridSize; row++)
        for (var col = 0; col < GameConfig.GridSize; col++)
            _gradientOverlayRenderer.RenderGradientOverlay(spriteBatch, col, row);

        foreach (var entity in _gameState.ActiveEntitiesSortedByDistance)
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
        foreach (var entity in _gameState.ActiveEntitiesSortedByDistance)
            if (entity is ExplosionEntity explosionEntity)
                _explosionRenderer.RenderAdditiveLightSource(spriteBatch, explosionEntity);

        _shared.RenderDirectionalLightSource(spriteBatch, _gameState.Miner.GetDirectionalLightSource(),
            _gameState.Miner.Direction, 192, 0.4f);

        if (!_gameState.IsInMiner)
            _shared.RenderDirectionalLightSource(spriteBatch, _gameState.Player.GetDirectionalLightSource(),
                _gameState.Player.Direction, 128, 0.3f);
    }

    private void RenderLightingToRenderTarget(SpriteBatch spriteBatch)
    {
        _graphics.GraphicsDevice.SetRenderTarget(_lightingRenderTarget);
        _graphics.GraphicsDevice.DepthStencilState = new DepthStencilState { DepthBufferEnable = true };

        _graphics.GraphicsDevice.Clear(Color.White);
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        var (viewportWidth, viewportHeight) = _viewHelpers.GetViewportSize();
        spriteBatch.Draw(_textures["white"], new Rectangle(0, 0, viewportWidth, viewportHeight),
            GradientOverlayRenderer.OverlayColor * 0.8f);


        _shared.RenderDirectionalLightSource(spriteBatch, _gameState.Miner.GetDirectionalLightSource(),
            _gameState.Miner.Direction);

        if (!_gameState.IsInMiner)
            _shared.RenderDirectionalLightSource(spriteBatch, _gameState.Player.GetDirectionalLightSource(),
                _gameState.Player.Direction, 256);

        _shared.RenderRadialLightSource(spriteBatch,
            _gameState.IsInMiner ? _gameState.Miner.CenterPosition : _gameState.Player.CenterPosition, 512, 0.4f);

        foreach (var entity in _gameState.ActiveEntitiesSortedByDistance)
            if (entity is DynamiteEntity dynamiteEntity)
                _dynamiteRenderer.RenderLightSource(spriteBatch, dynamiteEntity);
            else if (entity is ExplosionEntity explosionEntity)
                _explosionRenderer.RenderLightSource(spriteBatch, explosionEntity);

        // Render any grid-based light sources
        for (var row = 0; row < GameConfig.GridSize; row++)
        for (var col = 0; col < GameConfig.GridSize; col++)
            if (_gameState.Grid.GetCellType(col, row) == CellType.Lava)
            {
                var pos = new Vector2(col + 0.5f, row + 0.5f);
                _shared.RenderRadialLightSource(spriteBatch, pos, _lavaLightColor, 150, 0.6f);
            }

        // Render overlay gradients in shadow color over lighting to block out light on unexplored cells
        for (var row = 0; row < GameConfig.GridSize; row++)
        for (var col = 0; col < GameConfig.GridSize; col++)
            _gradientOverlayRenderer.RenderGradientOverlay(spriteBatch, col, row, 2, 120);

        spriteBatch.End();


        _graphics.GraphicsDevice.SetRenderTarget(null);
    }
}