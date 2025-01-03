﻿using System.Collections.Generic;
using AstroMiner.Renderers;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public class AstroMinerGame : Game
{
    // TODO these should live elsewhere but needed by a few places
    private readonly HashSet<MiningControls> _activeMiningControls = new();
    private readonly FrameCounter _frameCounter = new();

    private readonly GraphicsDeviceManager _graphics;

    private readonly Dictionary<string, Texture2D> _textures = new();
    private GameState _gameState;
    private Renderer _renderer;
    private SpriteBatch _spriteBatch;


    public AstroMinerGame()
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
        _renderer = new Renderer(_graphics, _textures, _gameState, _frameCounter);
        Window.ClientSizeChanged += _renderer.HandleWindowResize;
        base.Initialize();
    }

    private void LoadTexture(string name)
    {
        _textures[name] = Content.Load<Texture2D>($"img/{name}");
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        LoadTexture("dogica-font");
        LoadTexture("gradient-set");
        LoadTexture("white");
        LoadTexture("tileset");
        LoadTexture("player");
        LoadTexture("miner-no-tracks");
        LoadTexture("tracks-0");
        LoadTexture("tracks-1");
        LoadTexture("tracks-2");
        LoadTexture("radial-light");
        LoadTexture("directional-light");
        LoadTexture("dynamite");
        LoadTexture("explosion");
    }

    protected override void Update(GameTime gameTime)
    {
        _activeMiningControls.Clear();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var keyboardState = Keyboard.GetState();

        // TODO use a map for this instead?
        if (keyboardState.IsKeyDown(Keys.W))
            _activeMiningControls.Add(MiningControls.MoveUp);
        if (keyboardState.IsKeyDown(Keys.D))
            _activeMiningControls.Add(MiningControls.MoveRight);
        if (keyboardState.IsKeyDown(Keys.S))
            _activeMiningControls.Add(MiningControls.MoveDown);
        if (keyboardState.IsKeyDown(Keys.A))
            _activeMiningControls.Add(MiningControls.MoveLeft);
        if (keyboardState.IsKeyDown(Keys.Space))
            _activeMiningControls.Add(MiningControls.Drill);
        if (keyboardState.IsKeyDown(Keys.E))
            _activeMiningControls.Add(MiningControls.EnterOrExit);
        if (keyboardState.IsKeyDown(Keys.R))
            _activeMiningControls.Add(MiningControls.PlaceDynamite);
        if (keyboardState.IsKeyDown(Keys.N))
            _activeMiningControls.Add(MiningControls.NewGame);

        _gameState.Update(_activeMiningControls, gameTime.ElapsedGameTime.Milliseconds);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _frameCounter.Update(deltaTime);


        GraphicsDevice.Clear(Color.Black);

        _renderer.Render(_spriteBatch);

        base.Draw(gameTime);
    }
}