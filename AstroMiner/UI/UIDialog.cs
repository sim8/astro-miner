using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public sealed class UIDialog : UIElement
{
    public UIDialog(BaseGame game) : base(game)
    {
        ChildrenDirection = ChildrenDirection.Row;
        Children =
        [
            new UIElement(game)
            {
                FixedWidth = 64 * game.StateManager.Ui.UIScale,
                FixedHeight = 64 * game.StateManager.Ui.UIScale,
                BackgroundColor = Color.Brown
            },
            new UIElement(game)
            {
                FixedWidth = DialogHelpers.DialogBoxWidth * game.StateManager.Ui.UIScale,
                FixedHeight = DialogHelpers.DialogBoxHeight * game.StateManager.Ui.UIScale,
                BackgroundColor = Color.ForestGreen,
                Children = CreateTextElements(game, "Hello there hows it going what are you up to and so on".ToUpper())
            }
        ];
    }

    private List<UIElement> CreateTextElements(BaseGame game, string text)
    {
        var visibleLines = DialogHelpers.BreakDialogIntoVisibleLines(text);
        var elements = new List<UIElement>();
        foreach (var visibleLine in visibleLines)
            elements.Add(new UITextElement(game)
            {
                Text = visibleLine,
                Scale = 1 * game.StateManager.Ui.UIScale
            });

        return elements;
    }
}