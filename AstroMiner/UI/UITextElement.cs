using System.Collections.Generic;
using System.Linq;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public class UITextElement(BaseGame game) : UIElement(game)
{
    public string Text { get; set; }
    public int Scale { get; set; } = 1;
    public Color Color { get; set; } = Color.White;
    private List<(int x, int y, int width, int height)> StringSourceRects { get; set; }

    protected override (int width, int height) MeasureChildren(int availableWidth, int availableHeight)
    {
        StringSourceRects = FontHelpers.TransformString(Text);
        return (StringSourceRects.Sum(c => c.width * Scale), StringSourceRects.First().height * Scale);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);

        var currentXOffset = Padding; // Start with padding offset
        var yOffset = Padding; // Add padding to Y position as well

        foreach (var (charX, charY, charWidth, charHeight) in StringSourceRects) // TODO change this to Rectangle types
        {
            var sourceRect = new Rectangle(charX, charY, charWidth, charHeight);
            var scaledCharWidth = charWidth * Scale;
            var scaledCharHeight = charHeight * Scale;
            var destRect = new Rectangle(X + currentXOffset, Y + yOffset, scaledCharWidth, scaledCharHeight);
            spriteBatch.Draw(game.Textures["dogica-font"], destRect, sourceRect, Color);
            currentXOffset += scaledCharWidth;
        }
    }
}