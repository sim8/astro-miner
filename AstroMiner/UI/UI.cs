using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIState
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

    public StarBackgroundState StarBackground { get; init; } = new();
}

public class StarBackgroundState
{
    public List<Vector2> StarPositions { get; set; } = new();
    public List<float> StarOpacities { get; set; } = new();
    public float ScrollSpeed { get; set; } = 50f; // pixels per second
    public int StarCount { get; set; } = 30;
    public float StarSpacing { get; set; } = 200f; // minimum distance between stars
    public float MinOpacity { get; set; } = 0.3f; // minimum star opacity
    public float MaxOpacity { get; set; } = 1.0f; // maximum star opacity
    public Random Random { get; set; } = new();
    private bool _initialized = false;

    public void Initialize(int screenWidth, int screenHeight)
    {
        if (_initialized) return;

        StarPositions.Clear();
        StarOpacities.Clear();

        // Place stars randomly across the screen and beyond the right edge
        for (int i = 0; i < StarCount; i++)
        {
            var x = Random.Next(-170, screenWidth + 340); // Extra space on both sides
            var y = Random.Next(0, screenHeight);
            var opacity = (float)(MinOpacity + Random.NextDouble() * (MaxOpacity - MinOpacity));

            StarPositions.Add(new Vector2(x, y));
            StarOpacities.Add(opacity);
        }

        _initialized = true;
    }

    public void Update(float deltaTime, int screenWidth, int screenHeight)
    {
        if (!_initialized) Initialize(screenWidth, screenHeight);

        // Move all stars to the left
        for (int i = 0; i < StarPositions.Count; i++)
        {
            var star = StarPositions[i];
            star.X -= ScrollSpeed * deltaTime;

            // If star has moved completely off the left side, recycle it to the right
            if (star.X < -170)
            {
                star.X = screenWidth + Random.Next(0, 170); // Spawn somewhere off the right edge
                star.Y = Random.Next(0, screenHeight);
                StarOpacities[i] = (float)(MinOpacity + Random.NextDouble() * (MaxOpacity - MinOpacity));
            }

            StarPositions[i] = star;
        }
    }
}

public class UI(BaseGame game)
{
    public UIElement Root { get; private set; }
    public UIState State { get; init; } = new();

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