using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class ExplosionRenderer(
    RendererShared shared)
{
    private const int SizePx = 96;
    private const int AnimationFrames = 10;

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
        var lightSourcePos = explosionEntity.CenterPosition;
        var opacity = 1f - explosionEntity.AnimationPercentage;
        shared.RenderRadialLightSource(spriteBatch, lightSourcePos, 256, opacity);
    }
}