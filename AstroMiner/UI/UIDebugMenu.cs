using System;
using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public sealed class UIDebugMenu : UIElement
{
    public UIDebugMenu(BaseGame game) : base(game)
    {
        Children =
        [
            new UIElement(game)
            {
                ChildrenDirection = ChildrenDirection.Column,
                ChildrenAlign = ChildrenAlign.End,
                ChildrenJustify = ChildrenJustify.Start,
                Children =
                [
                    new UITextElement(game)
                    {
                        Text = "DEBUG",
                        Scale = 4,
                        Color = Color.Aqua,
                        BackgroundColor = Color.Red,
                        OnClick = () => Console.WriteLine("Debug")
                    },
                    new UIElement(game)
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