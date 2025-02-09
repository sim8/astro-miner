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

    private readonly DynamiteRenderer _dynamiteRenderer;
    private readonly ExplosionRenderer _explosionRenderer;

    private readonly FogOfWarRenderer _fogOfWarRenderer;

    private readonly GameState _gameState;
    private readonly Color _lavaLightColor = new(255, 231, 171);
    private readonly MinerRenderer _minerRenderer;
    private readonly BlendState _multiplyBlendState;
    private readonly PlayerRenderer _playerRenderer;
    private readonly ScrollingBackgroundRenderer _scrollingBackgroundRenderer;
    private readonly RendererShared _shared;
    private readonly UserInterfaceRenderer _userInterfaceRenderer;
    private RenderTarget2D _lightingRenderTarget;

    public AsteroidRenderer(
        RendererShared shared)
    {
        _gameState = shared.GameState;
        _shared = shared;
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

        var (width, height) = _shared.ViewHelpers.GetViewportSize();
        _lightingRenderTarget = new RenderTarget2D(
            _shared.Graphics.GraphicsDevice,
            width,
            height,
            false,
            _shared.Graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);
    }

    // TODO move out to top level renderer
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
        var (viewportWidth, viewportHeight) = _shared.ViewHelpers.GetViewportSize();
        spriteBatch.Draw(_lightingRenderTarget, new Rectangle(0, 0, viewportWidth, viewportHeight), Color.White);
        spriteBatch.End();

        // Additive lighting pass for glare (explosions, very brights lights)
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp);
        RenderAdditiveLighting(spriteBatch);
        spriteBatch.End();

        // Lastly, draw UI
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        _userInterfaceRenderer.RenderUserInterface(spriteBatch);
        spriteBatch.End();
    }

    private void RenderScene(SpriteBatch spriteBatch)
    {
        _scrollingBackgroundRenderer.RenderBackground(spriteBatch);

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

        // Render ECS entities
        foreach (var entityId in _gameState.Ecs.GetAllEntityIds())
        {
            // Render miner
            if (_gameState.Ecs.HasComponent<MinerTag>(entityId))
                _minerRenderer.RenderMiner(spriteBatch, entityId);

            // Render dynamite
            if (_gameState.Ecs.HasComponent<DynamiteTag>(entityId))
                _dynamiteRenderer.RenderDynamite(spriteBatch, entityId);

            // Render explosions
            if (_gameState.Ecs.HasComponent<ExplosionTag>(entityId))
                _explosionRenderer.RenderExplosion(spriteBatch, entityId);

            // Render player
            if (_gameState.Ecs.HasComponent<PlayerTag>(entityId) && !_gameState.AsteroidWorld.IsInMiner)
                _playerRenderer.RenderPlayer(spriteBatch, entityId);
        }
    }

    private void RenderAdditiveLighting(SpriteBatch spriteBatch)
    {
        // Render ECS entity lighting
        foreach (var entityId in _gameState.Ecs.GetAllEntityIds())
        {
            // Render explosion lighting
            if (_gameState.Ecs.HasComponent<ExplosionTag>(entityId))
                _explosionRenderer.RenderAdditiveLightSource(spriteBatch, entityId);
        }

        // TODO
        // _shared.RenderDirectionalLightSource(spriteBatch, _gameState.AsteroidWorld.Miner.GetDirectionalLightSource(),
        //     _gameState.AsteroidWorld.Miner.Direction, 192, 0.4f);

        // if (!_gameState.AsteroidWorld.IsInMiner)
        //     _shared.RenderDirectionalLightSource(spriteBatch, _gameState.AsteroidWorld.Player.GetDirectionalLightSource(),
        //         _gameState.AsteroidWorld.Player.Direction, 128, 0.3f);
    }

    private void RenderLightingToRenderTarget(SpriteBatch spriteBatch)
    {
        _shared.Graphics.GraphicsDevice.SetRenderTarget(_lightingRenderTarget);
        _shared.Graphics.GraphicsDevice.DepthStencilState = new DepthStencilState { DepthBufferEnable = true };

        _shared.Graphics.GraphicsDevice.Clear(Color.White);
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        var (viewportWidth, viewportHeight) = _shared.ViewHelpers.GetViewportSize();
        spriteBatch.Draw(_shared.Textures["white"], new Rectangle(0, 0, viewportWidth, viewportHeight),
            FogOfWarRenderer.FogColor * 0.8f);

        // TODO
        // _shared.RenderDirectionalLightSource(spriteBatch, _gameState.AsteroidWorld.Miner.GetDirectionalLightSource(),
        //     _gameState.AsteroidWorld.Miner.Direction);

        // if (!_gameState.AsteroidWorld.IsInMiner)
        //     _shared.RenderDirectionalLightSource(spriteBatch, _gameState.AsteroidWorld.Player.GetDirectionalLightSource(),
        //         _gameState.AsteroidWorld.Player.Direction, 256);

        // _shared.RenderRadialLightSource(spriteBatch,
        //     _gameState.AsteroidWorld.IsInMiner
        //         ? _gameState.AsteroidWorld.Miner.CenterPosition
        //         : _gameState.AsteroidWorld.Player.CenterPosition, 512, 0.4f);

        // Render ECS entity lighting
        foreach (var entityId in _gameState.Ecs.GetAllEntityIds())
        {
            // Render dynamite lighting
            if (_gameState.Ecs.HasComponent<DynamiteTag>(entityId))
                _dynamiteRenderer.RenderLightSource(spriteBatch, entityId);

            // Render explosion lighting
            if (_gameState.Ecs.HasComponent<ExplosionTag>(entityId))
                _explosionRenderer.RenderLightSource(spriteBatch, entityId);
        }

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

        spriteBatch.End();


        _shared.Graphics.GraphicsDevice.SetRenderTarget(null);
    }

    private void LoopVisibleCells(int padding, Action<int, int> cellAction)
    {
        var (startCol, startRow, endCol, endRow) = _shared.ViewHelpers.GetVisibleGrid(padding);

        for (var row = startRow; row < endRow; row++)
            for (var col = startCol; col < endCol; col++)
                cellAction(col, row);
    }
}