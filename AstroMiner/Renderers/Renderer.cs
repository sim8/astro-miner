using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Renderers.AsteroidWorld;
using AstroMiner.Renderers.HomeWorld;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class Renderer
{
    private readonly GameState _gameState;
    private readonly RendererShared _shared;
    private readonly AsteroidRenderer AsteroidRenderer;
    private readonly HomeWorldRenderer HomeWorldRenderer;
    private readonly DynamiteRenderer _dynamiteRenderer;
    private readonly ExplosionRenderer _explosionRenderer;
    private readonly MinerRenderer _minerRenderer;
    private readonly BlendState _multiplyBlendState;
    private readonly PlayerRenderer _playerRenderer;
    private readonly ScrollingBackgroundRenderer _scrollingBackgroundRenderer;
    private readonly UserInterfaceRenderer _userInterfaceRenderer;
    private RenderTarget2D _lightingRenderTarget;

    public Renderer(
        GraphicsDeviceManager graphics,
        Dictionary<string, Texture2D> textures,
        GameState gameState,
        FrameCounter frameCounter)
    {
        _shared = new RendererShared(gameState, graphics, textures);
        _gameState = gameState;
        _minerRenderer = new MinerRenderer(_shared);
        _playerRenderer = new PlayerRenderer(_shared);
        _dynamiteRenderer = new DynamiteRenderer(_shared);
        _explosionRenderer = new ExplosionRenderer(_shared);
        _scrollingBackgroundRenderer = new ScrollingBackgroundRenderer(_shared);
        _userInterfaceRenderer = new UserInterfaceRenderer(_shared, frameCounter);
        AsteroidRenderer = new AsteroidRenderer(_shared);
        HomeWorldRenderer = new HomeWorldRenderer(_shared);
        _multiplyBlendState = new BlendState();
        _multiplyBlendState.ColorBlendFunction = BlendFunction.Add;
        _multiplyBlendState.ColorSourceBlend = Blend.DestinationColor;
        _multiplyBlendState.ColorDestinationBlend = Blend.Zero;

        _initializeLightingRenderTarget();
    }

    public void HandleWindowResize(object sender, EventArgs e)
    {
        // RenderTarget2d can't change width/height after initialization. Re-init on window resize
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

        // TODO ActiveWorldRenderer (interface?)
        switch (_gameState.ActiveWorld)
        {
            case World.Asteroid:
                AsteroidRenderer.RenderWorld(spriteBatch);
                break;
            case World.Home:
                HomeWorldRenderer.Render(spriteBatch);
                break;
            default:
                throw new Exception("Invalid world");
        }

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

    private void RenderLightingToRenderTarget(SpriteBatch spriteBatch)
    {
        _shared.Graphics.GraphicsDevice.SetRenderTarget(_lightingRenderTarget);
        _shared.Graphics.GraphicsDevice.DepthStencilState = new DepthStencilState { DepthBufferEnable = true };

        _shared.Graphics.GraphicsDevice.Clear(Color.White);
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

        // TODO ActiveWorldRenderer (interface?)
        switch (_gameState.ActiveWorld)
        {
            case World.Asteroid:
                AsteroidRenderer.RenderWorldOverlay(spriteBatch);
                break;
            default:
                break;
        }

        foreach (var directionalLightSourceComponent in _gameState.Ecs.GetAllComponents<DirectionalLightSourceComponent>())
        {
            var positionComponent = _gameState.Ecs.GetComponent<PositionComponent>(directionalLightSourceComponent.EntityId);
            var movementComponent = _gameState.Ecs.GetComponent<MovementComponent>(directionalLightSourceComponent.EntityId);

            if (positionComponent.EntityId == _gameState.Ecs.PlayerEntityId && _gameState.AsteroidWorld.IsInMiner) continue;

            var lightSourcePos = movementComponent.Direction switch
            {
                Direction.Top => positionComponent.Position + directionalLightSourceComponent.TopOffset,
                Direction.Right => positionComponent.Position + directionalLightSourceComponent.RightOffset,
                Direction.Bottom => positionComponent.Position + directionalLightSourceComponent.BottomOffset,
                Direction.Left => positionComponent.Position + directionalLightSourceComponent.LeftOffset,
                _ => positionComponent.Position
            };

            _shared.RenderDirectionalLightSource(spriteBatch, lightSourcePos, movementComponent.Direction, directionalLightSourceComponent.SizePx);
        }

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


        // TODO ActiveWorldRenderer (interface?)
        switch (_gameState.ActiveWorld)
        {
            case World.Asteroid:
                AsteroidRenderer.RenderWorldLighting(spriteBatch);
                break;
            default:
                break;
        }

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

        // Final pass to apply shadows over light sources
        // TODO ActiveWorldRenderer (interface?)
        switch (_gameState.ActiveWorld)
        {
            case World.Asteroid:
                AsteroidRenderer.RenderShadows(spriteBatch);
                break;
            default:
                break;
        }

        spriteBatch.End();


        _shared.Graphics.GraphicsDevice.SetRenderTarget(null);
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
}