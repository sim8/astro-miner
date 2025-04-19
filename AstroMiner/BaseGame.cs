using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner
{
    public abstract class BaseGame : Game
    {
        // Common fields
        protected readonly GraphicsDeviceManager Graphics;
        protected readonly Dictionary<string, Texture2D> Textures = new();
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
        }

        // Common method for loading textures
        protected void LoadTexture(string name)
        {
            Textures[name] = Content.Load<Texture2D>($"img/{name}");
        }

        // Abstract methods that derived classes must implement
        protected abstract void InitializeControls();
    }
}