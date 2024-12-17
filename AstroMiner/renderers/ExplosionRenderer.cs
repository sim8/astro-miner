using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class ExplosionRenderer(
    RendererShared shared)
{
    public void RenderExplosion(SpriteBatch spriteBatch, ExplosionEntity explosionEntity)
    {
        // TODO
    }

    public void RenderLightSource(SpriteBatch spriteBatch, ExplosionEntity explosionEntity)
    {
        var lightSourcePos = explosionEntity.CenterPosition;
        var opacity = 1f - explosionEntity.AnimationPercentage;
        shared.RenderRadialLightSource(spriteBatch, lightSourcePos, 256, opacity);
    }
}