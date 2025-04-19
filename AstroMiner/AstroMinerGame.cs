﻿using System.Collections.Generic;
using AstroMiner.Renderers;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public class AstroMinerGame : Game
{
    private readonly ControlMapper<MiningControls> _miningControlMapper = new();
    public readonly FrameCounter FrameCounter = new();

    public readonly GraphicsDeviceManager Graphics;

    public readonly Dictionary<string, Texture2D> Textures = new();
    private Renderer _renderer;
    private SpriteBatch _spriteBatch;


    public AstroMinerGame()
    {
        Graphics = new GraphicsDeviceManager(this);
        Graphics.PreferredBackBufferWidth = 1280;
        Graphics.PreferredBackBufferHeight = 1024;
        Graphics.IsFullScreen = false;
        Window.AllowUserResizing = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    public GameState State { get; private set; }

    protected override void Initialize()
    {
        State = new GameState(this);
        _renderer = new Renderer(this);
        Window.ClientSizeChanged += _renderer.HandleWindowResize;
        InitializeMiningControls();
        base.Initialize();
    }

    private void InitializeMiningControls()
    {
        _miningControlMapper.AddMapping(MiningControls.MoveUp, Keys.W, Buttons.LeftThumbstickUp, true);
        _miningControlMapper.AddMapping(MiningControls.MoveRight, Keys.D, Buttons.LeftThumbstickRight, true);
        _miningControlMapper.AddMapping(MiningControls.MoveDown, Keys.S, Buttons.LeftThumbstickDown, true);
        _miningControlMapper.AddMapping(MiningControls.MoveLeft, Keys.A, Buttons.LeftThumbstickLeft, true);
        _miningControlMapper.AddMapping(MiningControls.Drill, Keys.Space, Buttons.RightTrigger, true);
        _miningControlMapper.AddMapping(MiningControls.EnterOrExit, Keys.E, Buttons.Y, false);
        _miningControlMapper.AddMapping(MiningControls.PlaceDynamite, Keys.R, Buttons.RightShoulder, false);
        _miningControlMapper.AddMapping(MiningControls.UseGrapple, Keys.G, Buttons.LeftTrigger, true);
        _miningControlMapper.AddMapping(MiningControls.NewGameOrReturnToBase, Keys.N, Buttons.Start, false);
    }

    private void LoadTexture(string name)
    {
        Textures[name] = Content.Load<Texture2D>($"img/{name}");
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        LoadTexture("dogica-font");
        LoadTexture("gradient-set");
        LoadTexture("white");
        LoadTexture("cracks");
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
        LoadTexture("cloud-background");
        LoadTexture("land-background");
        LoadTexture("grapple-icon");
        LoadTexture("oizus-bg");
        LoadTexture("launch-light");
        LoadTexture("launch-background");
        LoadTexture("launch-background-repeating");
        LoadTexture("launch-pad-front");
        LoadTexture("launch-pad-rear");
        LoadTexture("mountains-nice");
        LoadTexture("mountains-nice-mask");
        LoadTexture("mountains-nice-tiled");
        LoadTexture("oizus-rocks-under");
        LoadTexture("rig-room");
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var gamePadState = GamePad.GetState(PlayerIndex.One);

        var activeMiningControls = _miningControlMapper.GetActiveControls(keyboardState, gamePadState);

        State.Update(activeMiningControls, gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        FrameCounter.Update(deltaTime);


        GraphicsDevice.Clear(Color.Black);

        _renderer.Render(_spriteBatch);

        base.Draw(gameTime);
    }
}