using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public sealed class UIInventoryFooter : UIElement
{
    public UIInventoryFooter(BaseGame game) : base(game)
    {
        ChildrenDirection = ChildrenDirection.Row;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Start;
        Children =
        [
        ];
    }
}

public sealed class UIInventoryFooterItem : UIElement
{
    public UIInventoryFooterItem(BaseGame game) : base(game)
    {
        FixedWidth = 32;
        FixedHeight = 32;
        BackgroundColor = Color.Red;
        Children =
        [
            new UITextElement(game)
            {
                Text = "DEBUG",
                Scale = 1,
                Padding = 5
            }
        ];
    }
}