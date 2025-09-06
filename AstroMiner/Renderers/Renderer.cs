using System;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Renderers.AsteroidWorld;
using AstroMiner.Renderers.Entities;
using AstroMiner.Renderers.StaticWorld;
using AstroMiner.Renderers.UI;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public class Renderer
{
    private readonly BaseWorldRenderer _asteroidWorldRenderer;
    private readonly DynamiteRenderer _dynamiteRenderer;
    private readonly ExplosionRenderer _explosionRenderer;
    private readonly BaseGame _game;
    private readonly MinerRenderer _minerRenderer;
    private readonly BlendState _multiplyBlendState;
    private readonly PlayerRenderer _playerRenderer;
    private readonly ScrollingBackgroundRenderer _scrollingBackgroundRenderer;
    private readonly RendererShared _shared;
    private readonly BaseWorldRenderer _staticWorldRenderer;
    private readonly UIRenderer _uiRenderer;
    private readonly UserInterfaceRenderer _userInterfaceRenderer;
    private RenderTarget2D _lightingRenderTarget;

    public Renderer(
        BaseGame game)
    {
        _game = game;
        _shared = new RendererShared(_game);
        _uiRenderer = new UIRenderer(_game);
        _minerRenderer = new MinerRenderer(_shared);
        _playerRenderer = new PlayerRenderer(_shared);
        _dynamiteRenderer = new DynamiteRenderer(_shared);
        _explosionRenderer = new ExplosionRenderer(_shared);
        _scrollingBackgroundRenderer = new ScrollingBackgroundRenderer(_shared);
        _userInterfaceRenderer = new UserInterfaceRenderer(_shared);
        _asteroidWorldRenderer = new AsteroidWorldRenderer(_shared);// TODO deprecate
        _staticWorldRenderer = new StaticWorldRenderer(_shared);
        _multiplyBlendState = new BlendState();
        _multiplyBlendState.ColorBlendFunction = BlendFunction.Add;
        _multiplyBlendState.ColorSourceBlend = Blend.DestinationColor;
        _multiplyBlendState.ColorDestinationBlend = Blend.Zero;

        _initializeLightingRenderTarget();
    }

    private BaseWorldRenderer ActiveWorldRenderer =>
        _game.Model.ActiveWorld switch
        {
            World.Asteroid => _asteroidWorldRenderer,
            _ => _staticWorldRenderer
        };

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
        if (_game.StateManager.IsInGame)
        {
            // Draw RenderTargets first to avoid wiping BackBuffer
            RenderLightingToRenderTarget(spriteBatch);

            var (viewportWidth, viewportHeight) = _shared.ViewHelpers.GetViewportSize();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            // Draw base color (even though we set in GraphicsDevice.Clear, wiped by BackBuffer)
            spriteBatch.Draw(_shared.Textures[Tx.White], new Rectangle(0, 0, viewportWidth, viewportHeight),
                Colors.VeryDarkBlue);
            RenderScene(spriteBatch);

            spriteBatch.End();

            // Multiply lights/shadow with scene
            spriteBatch.Begin(SpriteSortMode.Deferred, _multiplyBlendState, SamplerState.PointClamp);
            spriteBatch.Draw(_lightingRenderTarget, new Rectangle(0, 0, viewportWidth, viewportHeight), Color.White);
            spriteBatch.End();

            // Additive lighting pass for glare (explosions, very brights lights)
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp);
            RenderAdditiveLighting(spriteBatch);
            spriteBatch.End();
        }

        // Lastly, draw UI / overlays
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

        if (_game.StateManager.TransitionManager.Opacity > 0)
        {
            var (viewportWidth, viewportHeight) = _shared.ViewHelpers.GetViewportSize();
            spriteBatch.Draw(_shared.Textures[Tx.White], new Rectangle(0, 0, viewportWidth, viewportHeight),
                Colors.VeryDarkBlue * _game.StateManager.TransitionManager.Opacity);
        }

        _uiRenderer.Render(spriteBatch);

        spriteBatch.End();
    }

    private void RenderEntities(SpriteBatch spriteBatch, EntityRenderLayer targetLayer)
    {
        foreach (var entityId in _game.StateManager.Ecs.EntityIdsInActiveWorldSortedByDistance)
        {
            // Check if entity has the target render layer
            var renderLayerComponent = _game.StateManager.Ecs.GetComponent<RenderLayerComponent>(entityId);
            var entityLayer = renderLayerComponent?.EntityRenderLayer ?? EntityRenderLayer.Default;
            if (entityLayer != targetLayer) continue;

            // Render miner
            if (_game.StateManager.Ecs.HasComponent<MinerTag>(entityId))
                _minerRenderer.RenderMiner(spriteBatch, entityId);

            // Render dynamite
            if (_game.StateManager.Ecs.HasComponent<DynamiteTag>(entityId))
                _dynamiteRenderer.RenderDynamite(spriteBatch, entityId);

            // Render explosions
            if (_game.StateManager.Ecs.HasComponent<ExplosionTag>(entityId))
                _explosionRenderer.RenderExplosion(spriteBatch, entityId);

            // Render player or NPCs
            if ((_game.StateManager.Ecs.HasComponent<PlayerTag>(entityId) &&
                 !_game.StateManager.AsteroidWorld.IsInMiner) ||
                _game.StateManager.Ecs.HasComponent<NpcComponent>(entityId))
                _playerRenderer.RenderPlayer(spriteBatch, entityId);

            if (_game.StateManager.Ecs.HasComponent<TextureComponent>(entityId))
            {
                var textureComponent = _game.StateManager.Ecs.GetComponent<TextureComponent>(entityId);
                var positionComponent = _game.StateManager.Ecs.GetComponent<PositionComponent>(entityId);

                var textureWidth = positionComponent.WidthPx + textureComponent.LeftPaddingPx +
                                   textureComponent.RightPaddingPx;
                var textureHeight = positionComponent.HeightPx + textureComponent.TopPaddingPx +
                                    textureComponent.BottomPaddingPx;
                var texturePos = positionComponent.Position -
                                 new Vector2(ViewHelpers.ConvertTexturePxToGridUnits(textureComponent.LeftPaddingPx),
                                     ViewHelpers.ConvertTexturePxToGridUnits(textureComponent.TopPaddingPx));
                var destinationRectangle = _shared.ViewHelpers.GetVisibleRectForObject(texturePos,
                    textureWidth, textureHeight);
                var combinedXOffset = textureComponent.TextureOffsetXPx + textureComponent.frameIndex * textureWidth;
                var sourceRectangle = new Rectangle(combinedXOffset, textureComponent.TextureOffsetYPx, textureWidth, textureHeight);
                spriteBatch.Draw(_shared.Textures[textureComponent.TextureName], destinationRectangle, sourceRectangle,
                    Color.White);
            }

            if (_game.Debug.showEntityBoundingBoxes)
            {
                const int borderWidth = 3;
                Color borderColor = Color.Red;
                var positionComponent = _game.StateManager.Ecs.GetComponent<PositionComponent>(entityId);
                var destinationRectangle = _shared.ViewHelpers.GetVisibleRectForObject(positionComponent.Position,
                    positionComponent.WidthPx, positionComponent.HeightPx);

                var topBorder = new Rectangle(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, borderWidth);
                spriteBatch.Draw(_shared.Textures[Tx.White], topBorder, borderColor);

                var bottomBorder = new Rectangle(destinationRectangle.X, destinationRectangle.Y + destinationRectangle.Height - borderWidth, destinationRectangle.Width, borderWidth);
                spriteBatch.Draw(_shared.Textures[Tx.White], bottomBorder, borderColor);

                var leftBorder = new Rectangle(destinationRectangle.X, destinationRectangle.Y, borderWidth, destinationRectangle.Height);
                spriteBatch.Draw(_shared.Textures[Tx.White], leftBorder, borderColor);

                var rightBorder = new Rectangle(destinationRectangle.X + destinationRectangle.Width - borderWidth, destinationRectangle.Y, borderWidth, destinationRectangle.Height);
                spriteBatch.Draw(_shared.Textures[Tx.White], rightBorder, borderColor);
            }
        }
    }

    private void RenderScene(SpriteBatch spriteBatch)
    {
        if (_game.Model.ActiveWorld == World.Asteroid || _game.Model.ActiveWorld == World.ShipUpstairs ||
            _game.Model.ActiveWorld == World.ShipDownstairs) _scrollingBackgroundRenderer.RenderBackground(spriteBatch);

        RenderEntities(spriteBatch, EntityRenderLayer.BehindWorld);

        ActiveWorldRenderer.RenderWorld(spriteBatch);

        RenderEntities(spriteBatch, EntityRenderLayer.BehindEntities);

        RenderEntities(spriteBatch, EntityRenderLayer.Default);
    }

    private void RenderLightingToRenderTarget(SpriteBatch spriteBatch)
    {
        _shared.Graphics.GraphicsDevice.SetRenderTarget(_lightingRenderTarget);
        _shared.Graphics.GraphicsDevice.DepthStencilState = new DepthStencilState { DepthBufferEnable = true };

        _shared.Graphics.GraphicsDevice.Clear(Color.White);
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

        ActiveWorldRenderer.RenderWorldOverlay(spriteBatch);


        foreach (var directionalLightSourceComponent in _game.StateManager.Ecs
                     .GetAllComponentsInActiveWorld<DirectionalLightSourceComponent>())
        {
            var positionComponent =
                _game.StateManager.Ecs.GetComponent<PositionComponent>(directionalLightSourceComponent.EntityId);
            var movementComponent =
                _game.StateManager.Ecs.GetComponent<MovementComponent>(directionalLightSourceComponent.EntityId);
            var directionComponent =
                _game.StateManager.Ecs.GetComponent<DirectionComponent>(directionalLightSourceComponent.EntityId);

            if (positionComponent.EntityId == _game.StateManager.Ecs.PlayerEntityId &&
                _game.StateManager.AsteroidWorld.IsInMiner) continue;

            var lightSourcePos = directionComponent.Direction switch
            {
                Direction.Top => positionComponent.Position + directionalLightSourceComponent.TopOffset,
                Direction.Right => positionComponent.Position + directionalLightSourceComponent.RightOffset,
                Direction.Bottom => positionComponent.Position + directionalLightSourceComponent.BottomOffset,
                Direction.Left => positionComponent.Position + directionalLightSourceComponent.LeftOffset,
                _ => positionComponent.Position
            };

            _shared.RenderDirectionalLightSource(spriteBatch, lightSourcePos, directionComponent.Direction,
                directionalLightSourceComponent.SizePx);
        }

        // TODO radial light sources?

        ActiveWorldRenderer.RenderWorldLighting(spriteBatch);

        // Render ECS entity lighting
        foreach (var entityId in _game.StateManager.Ecs.EntityIdsInActiveWorldSortedByDistance)
        {
            // Render dynamite lighting
            if (_game.StateManager.Ecs.HasComponent<DynamiteTag>(entityId))
                _dynamiteRenderer.RenderLightSource(spriteBatch, entityId);

            // Render explosion lighting
            if (_game.StateManager.Ecs.HasComponent<ExplosionTag>(entityId))
                _explosionRenderer.RenderLightSource(spriteBatch, entityId);


            if (_game.StateManager.Ecs.HasComponent<RadialLightSourceComponent>(entityId))
            {
                var radialLightSourceComponent =
                    _game.StateManager.Ecs.GetComponent<RadialLightSourceComponent>(entityId);
                var positionComponent = _game.StateManager.Ecs.GetComponent<PositionComponent>(entityId);

                _shared.RenderRadialLightSource(spriteBatch, positionComponent.CenterPosition,
                    radialLightSourceComponent.Tint, radialLightSourceComponent.SizePx,
                    radialLightSourceComponent.Opacity);
            }
        }

        // Final pass to apply shadows over light sources
        ActiveWorldRenderer.RenderShadows(spriteBatch);

        spriteBatch.End();

        _shared.Graphics.GraphicsDevice.SetRenderTarget(null);
    }

    private void RenderAdditiveLighting(SpriteBatch spriteBatch)
    {
        // Render ECS entity lighting
        foreach (var entityId in _game.StateManager.Ecs.EntityIdsInActiveWorldSortedByDistance)
            // Render explosion lighting
            if (_game.StateManager.Ecs.HasComponent<ExplosionTag>(entityId))
                _explosionRenderer.RenderAdditiveLightSource(spriteBatch, entityId);

        // TODO
        // _shared.RenderDirectionalLightSource(spriteBatch, _game.State.AsteroidWorld.Miner.GetDirectionalLightSource(),
        //     _game.State.AsteroidWorld.Miner.Direction, 192, 0.4f);

        // if (!_game.State.AsteroidWorld.IsInMiner)
        //     _shared.RenderDirectionalLightSource(spriteBatch, _game.State.AsteroidWorld.Player.GetDirectionalLightSource(),
        //         _game.State.AsteroidWorld.Player.Direction, 128, 0.3f);
    }
}