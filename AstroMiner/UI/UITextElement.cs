using System.Collections.Generic;
using System.Linq;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public class UITextElement(Dictionary<string, Texture2D> textures) : UIElement(textures)
{
    public string Text { get; set; }
    public Color Color { get; set; } = Color.White;
    private List<(int x, int y, int width, int height)> StringPos { get; set; }

    protected override (int width, int height) MeasureChildren(int availableWidth, int availableHeight)
    {
        StringPos = FontHelpers.TransformString(Text);
        return (StringPos.Sum(c => c.width), StringPos.First().height);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);

        var startX = X;
        var letterXOffset = 0;
        var scale = 1;
        foreach (var (x, y, width, height) in StringPos)
        {
            var sourceRect = new Rectangle(x, y, width, 8);
            var destRect = new Rectangle(startX + letterXOffset * scale, Y, width * scale, height * scale);
            spriteBatch.Draw(textures["dogica-font"], destRect, sourceRect, Color.LimeGreen);
            letterXOffset += width;
        }
    }
}