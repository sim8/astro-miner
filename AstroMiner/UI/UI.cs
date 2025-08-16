using AstroMiner.Definitions;
using AstroMiner.Effects;
using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIState(BaseGame game)
{
    public bool IsInMainMenu { get; set; } = true;
    public bool IsInPauseMenu { get; set; }
    public bool IsDebugMenuOpen { get; set; } = false;
    public bool IsInventoryOpen { get; set; }
    public bool IsLaunchConsoleOpen { get; set; }
    public bool IsInShopMenu { get; set; }
    public int sellConfirmationItemIndex { get; set; } = -1;

    // TODO all very temporary. Need a proper way of tracking dialog
    public bool IsInDialog { get; set; } = false;
    public int DialogIndex { get; set; } = 0;

    public ScrollingEffectManager StarBackground { get; init; } = new();

    public void Update(GameTime gameTime, ActiveControls activeControls)
    {
        if (IsInMainMenu)
        {
            if (StarBackground.Layers.Count == 0)
                StarBackground.AddLayer(new ScrollingEffectLayer
                {
                    TextureName = "star",
                    TextureSize = 170,
                    Speed = 50f,
                    Density = 2f,
                    MinOpacity = 0.3f,
                    MaxOpacity = 1.0f
                });
            else
                StarBackground.Update(gameTime, game.Graphics.GraphicsDevice.Viewport.Width,
                    game.Graphics.GraphicsDevice.Viewport.Height);
        }
        else if (StarBackground.Layers.Count > 0)
        {
            StarBackground.Layers.Clear();
        }
    }
}

public class UI(BaseGame game)
{
    public UIElement Root { get; private set; }
    public UIState State { get; init; } = new(game);

    public int UIScale { get; set; } = 2;

    public UIElement GetTree()
    {
        if (game.StateManager.Ui.State.IsInMainMenu) return new UIMainMenu(game);
        if (game.StateManager.Ui.State.IsInPauseMenu) return new UIPauseMenu(game);

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
                            game.Model.ActiveWorld == World.Asteroid ? new UIAsteroidHUD(game) : new UIElement(game),
                            new UITextElement(game)
                            {
                                Text = "CREDITS " + game.Model.Inventory.Credits,
                                Scale = 3,
                                Color = Colors.LightBlue,
                                Padding = 10
                            }
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
                        ChildrenDirection = ChildrenDirection.Column,
                        ChildrenJustify = ChildrenJustify.End,
                        Children =
                        [
                            .. game.Debug.showSeed
                                ? new UIElement[]
                                {
                                    new UITextElement(game)
                                    {
                                        Text = "SEED " + game.Model.Asteroid.Seed,
                                        Color = Color.Aqua,
                                        Padding = 10,
                                        Scale = 3
                                    }
                                }
                                : [],
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
                                : [],
                            new UIDebugButton(game)
                        ]
                    }
                ])
        };

        return root;
    }

    public void Update(GameTime gameTime, ActiveControls activeControls)
    {
        // Handle user events
        if (activeControls.Global.Contains(GlobalControls.PauseGame) && !State.IsInMainMenu)
            State.IsInPauseMenu = !State.IsInPauseMenu;

        if (activeControls.Global.Contains(GlobalControls.ToggleInventory))
        {
            if (State.IsLaunchConsoleOpen || State.IsInShopMenu)
            {
                State.IsLaunchConsoleOpen = false;
                State.IsInShopMenu = false;
                State.sellConfirmationItemIndex = -1;
            }
            else
            {
                State.IsInventoryOpen = !State.IsInventoryOpen;
            }
        }

        // Update state animations etc
        State.Update(gameTime, activeControls);

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