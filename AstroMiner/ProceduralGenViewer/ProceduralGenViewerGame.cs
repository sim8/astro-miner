﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner.ProceduralGenViewer;

public class ProceduralGenViewerGame : Game
{
    private readonly GraphicsDeviceManager _graphics;

    private readonly Dictionary<string, Texture2D> _textures = new();
    private GameState _gameState;
    private ProceduralGenViewerRenderer _renderer;
    private SpriteBatch _spriteBatch;
    private bool prevPressedNew;


    public ProceduralGenViewerGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 1024;
        _graphics.IsFullScreen = false;
        Window.AllowUserResizing = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _gameState = new GameState();
        _renderer = new ProceduralGenViewerRenderer(_textures, _gameState);
        base.Initialize();
    }

    private void LoadTexture(string name)
    {
        _textures[name] = Content.Load<Texture2D>($"img/{name}");
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        LoadTexture("white");
        LoadTexture("dogica-font");
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.N))

        {
            if (!prevPressedNew) _gameState.Initialize();

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
        GraphicsDevice.Clear(Color.Black);

        _renderer.Render(_spriteBatch);

        base.Draw(gameTime);
    }
}