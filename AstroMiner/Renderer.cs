using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class Renderer
{
    private readonly DynamiteRenderer _dynamiteRenderer;
    private readonly GameState _gameState;
    private readonly GraphicsDeviceManager _graphics;
    private readonly RenderTarget2D _lightingRenderTarget;
    private readonly MinerRenderer _minerRenderer;
    private readonly BlendState _multiplyBlendState;
    private readonly PlayerRenderer _playerRenderer;
    private readonly RendererHelpers _rendererHelpers;
    private readonly Dictionary<string, Texture2D> _textures;
    private readonly UserInterfaceRenderer _userInterfaceRenderer;
    private readonly ViewHelpers _viewHelpers;

    public Renderer(
        GraphicsDeviceManager graphics,
        Dictionary<string, Texture2D> textures,
        GameState gameState)
    {
        _gameState = gameState;
        _textures = textures;
        _viewHelpers = new ViewHelpers(gameState, graphics);
        _graphics = graphics;
        _minerRenderer = new MinerRenderer(textures, _gameState, _viewHelpers);
        _playerRenderer = new PlayerRenderer(textures, _gameState, _viewHelpers);
        _dynamiteRenderer = new DynamiteRenderer(textures, _gameState, _viewHelpers);
        _userInterfaceRenderer = new UserInterfaceRenderer(textures, _gameState, _viewHelpers);
        _rendererHelpers = new RendererHelpers(_viewHelpers, textures);
        _multiplyBlendState = new BlendState();
        _multiplyBlendState.ColorBlendFunction = BlendFunction.Add;
        _multiplyBlendState.ColorSourceBlend = Blend.DestinationColor;
        _multiplyBlendState.ColorDestinationBlend = Blend.Zero;

        _lightingRenderTarget = new RenderTarget2D(
            graphics.GraphicsDevice,
            graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
            graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
            false,
            graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);
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
        spriteBatch.Draw(_lightingRenderTarget, new Rectangle(0, 0, viewportWidth, viewportHeight), Color.White * 1f);
        spriteBatch.End();

        // Lastly, draw UI
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        _userInterfaceRenderer.RenderUserInterface(spriteBatch);
        spriteBatch.End();
    }

    private void RenderScene(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < GameConfig.GridSize; row++)
        for (var col = 0; col < GameConfig.GridSize; col++)
        {
            var cellState = _gameState.Grid.GetCellState(col, row);
            if (Tilesets.TilesetTextureNames.TryGetValue(cellState, out var name))
            {
                var offset = Tilesets.GetTileCoords(_gameState, col, row);
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
            else if (_gameState.Grid.GetCellState(col, row) == CellState.Floor)
            {
                var tilesetSourceRect = new Rectangle(3 * GameConfig.CellTextureSizePx,
                    GameConfig.CellTextureSizePx,
                    GameConfig.CellTextureSizePx, GameConfig.CellTextureSizePx);
                spriteBatch.Draw(_textures["rock-tileset"], _viewHelpers.GetVisibleRectForGridCell(col, row),
                    tilesetSourceRect,
                    Color.White);
            }
        }

        foreach (var entity in _gameState.ActiveEntitiesSortedByDistance)
            if (entity is MinerEntity)
                _minerRenderer.RenderMiner(spriteBatch);
            else if (entity is PlayerEntity)
                _playerRenderer.RenderPlayer(spriteBatch);
            else if (entity is DynamiteEntity dynamiteEntity)
                _dynamiteRenderer.RenderDynamite(spriteBatch, dynamiteEntity);
    }

    private void RenderLightingToRenderTarget(SpriteBatch spriteBatch)
    {
        _graphics.GraphicsDevice.SetRenderTarget(_lightingRenderTarget);
        _graphics.GraphicsDevice.DepthStencilState = new DepthStencilState { DepthBufferEnable = true };

        _graphics.GraphicsDevice.Clear(Color.White);
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        var (viewportWidth, viewportHeight) = _viewHelpers.GetViewportSize();
        spriteBatch.Draw(_textures["dark-screen"], new Rectangle(0, 0, viewportWidth, viewportHeight),
            Color.White * 0.8f);


        _rendererHelpers.RenderDirectionalLightSource(spriteBatch, _gameState.Miner.GetDirectionalLightSource(),
            _gameState.Miner.Direction);

        if (!_gameState.IsInMiner)
            _rendererHelpers.RenderDirectionalLightSource(spriteBatch, _gameState.Player.GetDirectionalLightSource(),
                _gameState.Player.Direction, 128);

        _rendererHelpers.RenderRadialLightSource(spriteBatch,
            _gameState.IsInMiner ? _gameState.Miner.CenterPosition : _gameState.Player.CenterPosition, 256, 0.4f);

        foreach (var entity in _gameState.ActiveEntitiesSortedByDistance)
            if (entity is DynamiteEntity dynamiteEntity)
                _dynamiteRenderer.RenderLightSource(spriteBatch, dynamiteEntity, _rendererHelpers);

        spriteBatch.End();

        _graphics.GraphicsDevice.SetRenderTarget(null);
    }

    private bool HasFloorWithinTwoTiles(int col, int row)
    {
        for (var x = col - 1; x <= col + 1; x++)
            if ((ViewHelpers.IsValidGridPosition(x, row - 2) &&
                 _gameState.Grid.GetCellState(x, row - 2) == CellState.Floor) ||
                (ViewHelpers.IsValidGridPosition(x, row + 2) &&
                 _gameState.Grid.GetCellState(x, row + 2) == CellState.Floor))
                return true;
        for (var y = row - 1; y <= row + 1; y++)
            if ((ViewHelpers.IsValidGridPosition(col + 2, y) &&
                 _gameState.Grid.GetCellState(col + 2, y) == CellState.Floor) ||
                (ViewHelpers.IsValidGridPosition(col - 2, y) &&
                 _gameState.Grid.GetCellState(col - 2, y) == CellState.Floor))
                return true;
        return false;
    }
}