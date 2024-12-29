using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class ExplosionRenderer(
    RendererShared shared)
{
    private const int SizePx = 96;
    private const int AnimationFrames = 10;

    private static readonly float[] FrameLightOpacity = { 0.5f, 1f, 1f, 0.8f, 0.6f, 0.4f, 0.3f, 0.2f, 0.1f, 0.05f };

    public void RenderExplosion(SpriteBatch spriteBatch, ExplosionEntity explosionEntity)
    {
        var frameIndex = (int)(explosionEntity.AnimationPercentage * AnimationFrames);

        var sourceRectangle = new Rectangle(frameIndex * SizePx, 0, SizePx, SizePx);
        var destinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(explosionEntity.Position,
            SizePx, SizePx, -(SizePx / 2), -(SizePx / 2));
        spriteBatch.Draw(shared.Textures["explosion"], destinationRectangle, sourceRectangle, Color.White);
    }

    public void RenderLightSource(SpriteBatch spriteBatch, ExplosionEntity explosionEntity)
    {
        var frameIndex = (int)(explosionEntity.AnimationPercentage * AnimationFrames);
        var opacity = FrameLightOpacity[frameIndex];
        shared.RenderRadialLightSource(spriteBatch, explosionEntity.Position, 512, opacity);
    }

    public void RenderAdditiveLightSource(SpriteBatch spriteBatch, ExplosionEntity explosionEntity)
    {
        var frameIndex = (int)(explosionEntity.AnimationPercentage * AnimationFrames);
        var opacity = FrameLightOpacity[frameIndex];
        shared.RenderRadialLightSource(spriteBatch, explosionEntity.Position, 512, opacity);
    }
}