using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public class ProcGenViewerGame : Game
{
    private readonly GraphicsDeviceManager _graphics;

    private readonly Dictionary<string, Texture2D> _textures = new();
    private GameState _gameState;
    private ProcGenViewerRenderer _renderer;
    private SpriteBatch _spriteBatch;
    private bool prevPressedNew;


    public ProcGenViewerGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 1024;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _gameState = new GameState();
        _renderer = new ProcGenViewerRenderer(_textures, _gameState);
        base.Initialize();
    }

    private void LoadTexture(string name)
    {
        _textures[name] = Content.Load<Texture2D>($"img-proc-gen/{name}");
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        LoadTexture("rock-tileset");
        LoadTexture("solid-rock-tileset");
        LoadTexture("ruby-tileset");
        LoadTexture("diamond-tileset");
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.N))

        {
            if (!prevPressedNew)
            {
                Console.WriteLine("new woooorld");
                _gameState.Initialize();
            }

            prevPressedNew = true;
        }
        else
        {
            prevPressedNew = false;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.DarkGreen);

        _renderer.Render(_spriteBatch);

        base.Draw(gameTime);
    }
}