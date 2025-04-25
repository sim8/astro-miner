using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIState
{
    public bool IsDebugMenuOpen { get; set; } = false;
    public bool IsInventoryOpen { get; set; } = false;
}

public class UI(BaseGame game)
{
    public UIElement Root { get; private set; }
    public UIState State { get; init; } = new();

    public UIElement GetTree()
    {
        var root = new UIElement(game)
        {
            FullWidth = true,
            FullHeight = true,
            ChildrenDirection = ChildrenDirection.Column,
            ChildrenAlign = ChildrenAlign.Center,
            ChildrenJustify = ChildrenJustify.SpaceBetween,
            Children =
            [
                new UIElement(game)
                {
                    FullWidth = true,
                    ChildrenAlign = ChildrenAlign.Start,
                    ChildrenDirection = ChildrenDirection.Row,
                    ChildrenJustify = ChildrenJustify.SpaceBetween,
                    Children =
                    [
                        new UIElement(game)
                        {
                            // TODO put something here
                            FixedWidth = 200,
                            FixedHeight = 100
                        },

                        .. game.Debug.showFps
                            ? new UIElement[]
                            {
                                new UITextElement(game)
                                {
                                    Text = "FPS " + game.FrameCounter.AverageFramesPerSecond.ToString("F0"),
                                    Color = Color.Aqua,
                                    Padding = 10,
                                    Scale = 3
                                }
                            }
                            : []
                    ]
                },
                .. game.StateManager.Ui.State.IsInventoryOpen
                    ? new UIElement[]
                    {
                        new UIInventory(game)
                    }
                    : [],
                new UIElement(game)
                {
                    FullWidth = true,
                    ChildrenAlign = ChildrenAlign.End,
                    ChildrenDirection = ChildrenDirection.Row,
                    ChildrenJustify = ChildrenJustify.End,
                    Children =
                    [
                        new UIDebugButton(game)
                    ]
                },
                .. !game.StateManager.Ui.State.IsInventoryOpen
                    ? new UIElement[]
                    {
                        new UIInventoryFooter(game)
                    }
                    : []
            ]
        };

        return root;
    }

    public void Update(GameTime gameTime)
    {
        Root = GetTree();

        Root.ComputeDimensions(game.Graphics.GraphicsDevice.Viewport.Width,
            game.Graphics.GraphicsDevice.Viewport.Height);
        Root.ComputePositions(0, 0);
    }

    /// <summary>
    ///     Handles mouse clicks and routes them to the appropriate UI element
    /// </summary>
    /// <param name="x">The x-coordinate of the mouse click</param>
    /// <param name="y">The y-coordinate of the mouse click</param>
    public void OnMouseClick(int x, int y)
    {
        if (Root != null) Root.HandleClick(x, y);
    }
}