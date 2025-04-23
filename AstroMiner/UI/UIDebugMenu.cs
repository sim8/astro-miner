using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public sealed class UIDebugMenu : UIElement
{
    public UIDebugMenu(Dictionary<string, Texture2D> textures) : base(textures)
    {
        Children =
        [
            new UIElement(textures)
            {
                ChildrenDirection = ChildrenDirection.Column,
                ChildrenAlign = ChildrenAlign.End,
                ChildrenJustify = ChildrenJustify.Start,
                Children =
                [
                    new UITextElement(textures)
                    {
                        Text = "DEBUG",
                        Scale = 4,
                        Color = Color.Aqua,
                        BackgroundColor = Color.Red,
                        OnClick = () => Console.WriteLine("Debug")
                    },
                    new UIElement(textures)
                    {
                        BackgroundColor = Color.Pink,
                        FixedWidth = 200,
                        FixedHeight = 100
                    }
                ]
            }
        ];
    }
}