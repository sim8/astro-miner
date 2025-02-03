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

    public void RenderExplosion(SpriteBatch spriteBatch, Vector2 position, ExplosionComponent explosion)
    {
        var frameIndex = (int)(explosion.AnimationPercentage * AnimationFrames);

        var sourceRectangle = new Rectangle(frameIndex * SourceSizePx, 0, SourceSizePx, SourceSizePx);
        var destinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(position,
            SizePx, SizePx, -(SizePx / 2), -(SizePx / 2));
        spriteBatch.Draw(shared.Textures["explosion"], destinationRectangle, sourceRectangle, Color.White);
    }

    public void RenderLightSource(SpriteBatch spriteBatch, Vector2 position, ExplosionComponent explosion)
    {
        var frameIndex = (int)(explosion.AnimationPercentage * AnimationFrames);
        var opacity = FrameLightOpacity[frameIndex];
        shared.RenderRadialLightSource(spriteBatch, position, 1024, opacity);
    }

    public void RenderAdditiveLightSource(SpriteBatch spriteBatch, Vector2 position, ExplosionComponent explosion)
    {
        // Same for now
        RenderLightSource(spriteBatch, position, explosion);
    }
}