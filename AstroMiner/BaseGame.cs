using System.Collections.Generic;
using AstroMiner.Model;
using AstroMiner.Storage;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner;

public abstract class BaseGame : Game
{
    public readonly DebugOptions Debug = new();
    public readonly FrameCounter FrameCounter = new();
    public readonly GameStateStorage GameStateStorage;
    public readonly GraphicsDeviceManager Graphics;
    public readonly Dictionary<string, Texture2D> Textures = new();
    protected SpriteBatch SpriteBatch;

    protected BaseGame()
    {
        Graphics = new GraphicsDeviceManager(this);
        Graphics.PreferredBackBufferWidth = 1280;
        Graphics.PreferredBackBufferHeight = 1024;
        Graphics.IsFullScreen = false;
        Window.AllowUserResizing = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        GameStateStorage = new GameStateStorage(this);
    }

    public GameModel Model { get; set; }

    public GameStateManager StateManager { get; protected set; }

    // Common method for loading textures
    protected void LoadTexture(string name)
    {
        Textures[name] = Content.Load<Texture2D>($"img/{name}");
    }

    // Abstract methods that derived classes must implement
    protected abstract void InitializeControls();
}