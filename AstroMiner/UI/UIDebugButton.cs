using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public sealed class UIDebugButton : UIElement
{
    public UIDebugButton(BaseGame game) : base(game)
    {
        ChildrenDirection = ChildrenDirection.Column;
        ChildrenAlign = ChildrenAlign.End;
        ChildrenJustify = ChildrenJustify.Start;
        Children = UIHelpers.FilterNull([
            game.StateManager.Ui.State.IsDebugMenuOpen
                ? new UIDebugMenu(game)
                : null,
            new UITextElement(game)
            {
                Text = "DEBUG",
                Scale = 3,
                Padding = 10,
                Color = game.StateManager.Ui.State.IsDebugMenuOpen ? Color.Black : Color.Aqua,
                BackgroundColor = game.StateManager.Ui.State.IsDebugMenuOpen ? Color.Aqua : Color.Black,
                OnClick = () => game.StateManager.Ui.State.IsDebugMenuOpen = !game.StateManager.Ui.State.IsDebugMenuOpen
            }
        ]);
    }
}

public sealed class UIDebugMenu : UIElement
{
    public UIDebugMenu(BaseGame game) : base(game)
    {
        ChildrenDirection = ChildrenDirection.Column;
        ChildrenAlign = ChildrenAlign.Stretch;
        ChildrenJustify = ChildrenJustify.Start;
        BackgroundColor = Color.Black;
        Padding = 10;
        Children =
        [
            new UITextElement(game)
            {
                Text = "SEED",
                Padding = 6,
                Scale = 2,
                Color = game.Debug.showSeed ? Color.Black : Color.Aqua,
                BackgroundColor = game.Debug.showSeed ? Color.Aqua : Color.Black,
                OnClick = () => game.Debug.showSeed = !game.Debug.showSeed
            },
            new UIElement(game)
            {
                FixedHeight = 10
            },
                        new UITextElement(game)
            {
                Text = "ENTITY BOUNDING BOXES",
                Padding = 6,
                Scale = 2,
                Color = game.Debug.showEntityBoundingBoxes ? Color.Black : Color.Aqua,
                BackgroundColor = game.Debug.showEntityBoundingBoxes ? Color.Aqua : Color.Black,
                OnClick = () => game.Debug.showEntityBoundingBoxes = !game.Debug.showEntityBoundingBoxes
            },
            new UIElement(game)
            {
                FixedHeight = 10
            },
            new UITextElement(game)
            {
                Text = "GRID DEBUG",
                Padding = 6,
                Scale = 2,
                Color = game.Debug.showGridDebug ? Color.Black : Color.Aqua,
                BackgroundColor = game.Debug.showGridDebug ? Color.Aqua : Color.Black,
                OnClick = () => game.Debug.showGridDebug = !game.Debug.showGridDebug
            },
            new UIElement(game)
            {
                FixedHeight = 10
            },
            new UITextElement(game)
            {
                Text = "FPS",
                Padding = 6,
                Scale = 2,
                Color = game.Debug.showFps ? Color.Black : Color.Aqua,
                BackgroundColor = game.Debug.showFps ? Color.Aqua : Color.Black,
                OnClick = () => game.Debug.showFps = !game.Debug.showFps
            }
        ];
    }
}