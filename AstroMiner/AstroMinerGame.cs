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

    private readonly Dictionary<string, Texture2D> _textures = new();
    private GameState _gameState;
    private ControlMapper<MiningControls> _miningControlMapper;
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
        InitializeMiningControls();
        base.Initialize();
    }

    private void InitializeMiningControls()
    {
        _miningControlMapper = new ControlMapper<MiningControls>();

        _miningControlMapper.AddMapping(MiningControls.MoveUp, Keys.W, true);
        _miningControlMapper.AddMapping(MiningControls.MoveRight, Keys.D, true);
        _miningControlMapper.AddMapping(MiningControls.MoveDown, Keys.S, true);
        _miningControlMapper.AddMapping(MiningControls.MoveLeft, Keys.A, true);
        _miningControlMapper.AddMapping(MiningControls.Drill, Keys.Space, true);
        _miningControlMapper.AddMapping(MiningControls.EnterOrExit, Keys.E, false);
        _miningControlMapper.AddMapping(MiningControls.PlaceDynamite, Keys.R, false);
        _miningControlMapper.AddMapping(MiningControls.NewGame, Keys.N, false);
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
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var keyboardState = Keyboard.GetState();

        var activeMiningControls = _miningControlMapper.GetActiveControls(keyboardState);

        _gameState.Update(activeMiningControls, gameTime.ElapsedGameTime.Milliseconds);

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