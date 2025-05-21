namespace AstroMiner.UI;

public class UIScreen : UIElement
{
    public UIScreen(BaseGame game) : base(game)
    {
        FullWidth = true;
        FullHeight = true;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Center;
        Position = PositionMode.Absolute;
        BackgroundColor = Colors.VeryDarkBlue * 0.5f;
    }
}