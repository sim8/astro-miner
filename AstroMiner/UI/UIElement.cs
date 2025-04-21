using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public class UIElement(BaseGame game)
{
    public Color? BackgroundColor { get; set; }
    public int? FixedWidth { get; set; }
    public int? FixedHeight { get; set; }

    public int ComputedWidth { get; set; } = 0;
    public int ComputedHeight { get; set; } = 0;
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public List<UIElement> Children { get; } = new();

    public void Render(SpriteBatch spriteBatch)
    {
        var width = FixedWidth ?? ComputedWidth;
        var height = FixedHeight ?? ComputedHeight;

        if (BackgroundColor.HasValue)
            spriteBatch.Draw(game.Textures["white"],
                new Rectangle(X, Y, width, height),
                BackgroundColor.Value);

        foreach (var child in Children) child.Render(spriteBatch);
    }
}