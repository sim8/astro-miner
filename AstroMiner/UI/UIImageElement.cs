using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public class UIImageElement(BaseGame game, string textureName, Rectangle sourceRect) : UIElement(game)
{
    private readonly string TextureName = textureName;

    public override void Render(SpriteBatch spriteBatch)
    {
        var destRect = new Rectangle(X, Y, ComputedWidth, ComputedHeight);
        spriteBatch.Draw(game.Textures[TextureName], destRect, sourceRect, Color.White);

        base.Render(spriteBatch);
    }
}