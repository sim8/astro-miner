using System.Collections.Generic;
using AstroMiner.Renderers;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public class AstroMinerGame : Game
{
    private readonly FrameCounter _frameCounter = new();

    private readonly GraphicsDeviceManager _graphics;
    private readonly ControlMapper<MiningControls> _miningControlMapper = new();

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
        _gameState = new GameState(_graphics);
        _renderer = new Renderer(_graphics, _textures, _gameState, _frameCounter);
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
        _miningControlMapper.AddMapping(MiningControls.NewGame, Keys.N, Buttons.Start, false);
        _miningControlMapper.AddMapping(MiningControls.CycleZoom, Keys.Z, Buttons.Back, false);
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
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var gamePadState = GamePad.GetState(PlayerIndex.One);

        var activeMiningControls = _miningControlMapper.GetActiveControls(keyboardState, gamePadState);

        _gameState.Update(activeMiningControls, gameTime);

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