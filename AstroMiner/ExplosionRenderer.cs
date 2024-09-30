using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public class ExplosionRenderer(
    Dictionary<string, Texture2D> textures,
    GameState gameState,
    ViewHelpers viewHelpers)
{
    public void RenderExplosion(SpriteBatch spriteBatch, ExplosionEntity explosionEntity)
    {
        // TODO
    }

    public void RenderLightSource(SpriteBatch spriteBatch, ExplosionEntity explosionEntity,
        RendererHelpers rendererHelpers)
    {
        var lightSourcePos = explosionEntity.CenterPosition;
        var opacity = 1f - explosionEntity.AnimationPercentage;
        rendererHelpers.RenderRadialLightSource(spriteBatch, lightSourcePos, 256, opacity);
    }
}