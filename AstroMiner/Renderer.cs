using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AstroMiner;

public class Renderer
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly RenderTarget2D _lightingRenderTarget;
    private readonly MinerRenderer _minerRenderer;
    private readonly GameState _gameState;
    private readonly BlendState _multiplyBlendState;
    private readonly PlayerRenderer _playerRenderer;
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
        _userInterfaceRenderer = new UserInterfaceRenderer(textures, _gameState, _viewHelpers);
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

        // TODO ordered render of all visible entities
        if (_gameState.Player.Position.Y > _gameState.Miner.Position.Y)
        {
            _minerRenderer.RenderMiner(spriteBatch);
            if (!_gameState.IsInMiner) _playerRenderer.RenderPlayer(spriteBatch);
        }
        else
        {
            if (!_gameState.IsInMiner) _playerRenderer.RenderPlayer(spriteBatch);
            _minerRenderer.RenderMiner(spriteBatch);
        }
    }

    private void RenderRadialLightSource(SpriteBatch spriteBatch, Vector2 pos, int size = 256, float opacity = 1)
    {
        var offset = -(size / 2);
        var destinationRect = _viewHelpers.GetVisibleRectForObject(pos, size, size, offset, offset);
        spriteBatch.Draw(_textures["radial-light"], destinationRect, Color.White * opacity);
    }

    private void RenderDirectionalLightSource(SpriteBatch spriteBatch, Vector2 pos, Direction dir, int size = 256)
    {
        var destinationRect = _viewHelpers.GetVisibleRectForObject(pos, size, size);


        var radians = dir switch
        {
            Direction.Top => 0f,
            Direction.Right => (float)Math.PI / 2,
            Direction.Bottom => (float)Math.PI,
            Direction.Left => (float)(3 * Math.PI / 2),
            _ => 0
        };

        var origin = new Vector2(_textures["directional-light"].Width / 2f, _textures["directional-light"].Height);

        spriteBatch.Draw(_textures["directional-light"], destinationRect, null, Color.White, radians, origin,
            SpriteEffects.None, 0);
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


        RenderDirectionalLightSource(spriteBatch, _gameState.Miner.GetDirectionalLightSource(),
            _gameState.Miner.Direction);

        if (!_gameState.IsInMiner)
            RenderDirectionalLightSource(spriteBatch, _gameState.Player.GetDirectionalLightSource(),
                _gameState.Player.Direction, 128);

        RenderRadialLightSource(spriteBatch,
            _gameState.IsInMiner ? _gameState.Miner.CenterPosition : _gameState.Player.CenterPosition, 256, 0.4f);

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