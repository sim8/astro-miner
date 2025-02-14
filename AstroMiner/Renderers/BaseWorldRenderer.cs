using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers;

public abstract class BaseWorldRenderer
{
    protected readonly RendererShared Shared;

    protected BaseWorldRenderer(RendererShared shared)
    {
        Shared = shared;
    }

    public virtual void RenderWorld(SpriteBatch spriteBatch) { }

    public virtual void RenderWorldOverlay(SpriteBatch spriteBatch) { }

    public virtual void RenderWorldLighting(SpriteBatch spriteBatch) { }

    public virtual void RenderShadows(SpriteBatch spriteBatch) { }
}