using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.AsteroidWorld;

public class ExplosionRenderer(
    RendererShared shared)
{
    private const int SourceSizePx = 96;
    private const int SizePx = 360;
    private const int AnimationFrames = 10;

    private static readonly float[] FrameLightOpacity = { 0.7f, 1f, 1f, 0.8f, 0.5f, 0.3f, 0.2f, 0.15f, 0.1f, 0.05f };

    public void RenderExplosion(SpriteBatch spriteBatch, int entityId)
    {
        var positionComponent = shared.GameState.Ecs.GetComponent<PositionComponent>(entityId);
        var explosionComponent = shared.GameState.Ecs.GetComponent<ExplosionComponent>(entityId);

        if (positionComponent == null || explosionComponent == null)
            return;

        var frameIndex = (int)(explosionComponent.AnimationPercentage * AnimationFrames);

        var sourceRectangle = new Rectangle(frameIndex * SourceSizePx, 0, SourceSizePx, SourceSizePx);
        var destinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(positionComponent.Position,
            SizePx, SizePx, -(SizePx / 2), -(SizePx / 2));
        spriteBatch.Draw(shared.Textures["explosion"], destinationRectangle, sourceRectangle, Color.White);
    }

    public void RenderLightSource(SpriteBatch spriteBatch, int entityId)
    {
        var positionComponent = shared.GameState.Ecs.GetComponent<PositionComponent>(entityId);
        var explosionComponent = shared.GameState.Ecs.GetComponent<ExplosionComponent>(entityId);

        if (positionComponent == null || explosionComponent == null)
            return;

        var frameIndex = (int)(explosionComponent.AnimationPercentage * AnimationFrames);
        var opacity = FrameLightOpacity[frameIndex];
        shared.RenderRadialLightSource(spriteBatch, positionComponent.Position, 1024, opacity);
    }

    public void RenderAdditiveLightSource(SpriteBatch spriteBatch, int entityId)
    {
        // Same for now
        RenderLightSource(spriteBatch, entityId);
    }
}