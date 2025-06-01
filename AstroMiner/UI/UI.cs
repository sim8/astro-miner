using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIState
{
    public bool IsInMainMenu { get; set; } = true;
    public bool IsDebugMenuOpen { get; set; } = false;
    public bool IsInventoryOpen { get; set; } = false;
    public bool IsLaunchConsoleOpen { get; set; } = false;
    public bool IsInShopMenu { get; set; } = false;
    public int sellConfirmationItemIndex { get; set; } = -1;

    // TODO all very temporary. Need a proper way of tracking dialog
    public bool IsInDialog { get; set; } = false;
    public int DialogIndex { get; set; } = 0;
}

public class UI(BaseGame game)
{
    public UIElement Root { get; private set; }
    public UIState State { get; init; } = new();

    public int UIScale { get; set; } = 2;

    public UIElement GetTree()
    {
        if (game.StateManager.Ui.State.IsInMainMenu) return new UIMainMenu(game);

        var root = new UIElement(game)
        {
            FullWidth = true,
            FullHeight = true,
            ChildrenDirection = ChildrenDirection.Column,
            ChildrenAlign = ChildrenAlign.Center,
            ChildrenJustify = ChildrenJustify.SpaceBetween,
            Children =
                UIHelpers.FilterNull([
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
                                FixedWidth = 200 * UIScale,
                                FixedHeight = 100 * UIScale
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

                    game.StateManager.Ui.State.IsInDialog ? new UIDialog(game) : null,
                    game.StateManager.Ui.State.IsInventoryOpen ? new UIInventory(game) : null,
                    !game.StateManager.Ui.State.IsInventoryOpen ? new UIInventoryFooter(game) : null,
                    game.StateManager.Ui.State.IsLaunchConsoleOpen ? new UILaunchConsole(game) : null,
                    game.StateManager.Ui.State.IsInShopMenu ? new UIShop(game) : null,
                    new UIElement(game)
                    {
                        FullWidth = true,
                        FullHeight = true,
                        Position = PositionMode.Absolute,
                        ChildrenAlign = ChildrenAlign.End,
                        ChildrenDirection = ChildrenDirection.Row,
                        ChildrenJustify = ChildrenJustify.End,
                        Children =
                        [
                            new UIDebugButton(game)
                        ]
                    }
                ])
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