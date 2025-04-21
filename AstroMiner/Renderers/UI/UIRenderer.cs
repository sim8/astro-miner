using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.UI;

public class UIRenderer(BaseGame game)
{
    public void Render(SpriteBatch spriteBatch)
    {
        game.StateManager.UIState.Root.Render(spriteBatch);
    }
}