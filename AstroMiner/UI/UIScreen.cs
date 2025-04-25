using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIScreen : UIElement
{
    public UIScreen(BaseGame game) : base(game)
    {
        FullWidth = true;
        FullHeight = true;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Start;
        Position = PositionMode.Absolute;
        BackgroundColor = Color.Black * 0.5f;
    }
}