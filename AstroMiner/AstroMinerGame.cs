using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public enum CellState
{
    Empty,
    Floor,
    Rock,
    SolidRock,
    Diamond,
    Ruby
}

public class AstroMinerGame : Game
{
    // TODO these should live elsewhere but needed by a few places
    private readonly HashSet<MiningControls> _activeMiningControls = new();

    private readonly GraphicsDeviceManager _graphics;

    private readonly Dictionary<string, Texture2D> _textures = new();
    private MiningState _miningState;
    private Renderer _renderer;
    private SpriteBatch _spriteBatch;


    public AstroMinerGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 1024;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _miningState = new MiningState();
        _renderer = new Renderer(_graphics, _textures, _miningState);
        base.Initialize();
    }

    private void LoadTexture(string name)
    {
        _textures[name] = Content.Load<Texture2D>($"img/{name}");
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        LoadTexture("floor");
        LoadTexture("rock-tileset");
        LoadTexture("solid-rock-tileset");
        LoadTexture("ruby-tileset");
        LoadTexture("diamond-tileset");
        LoadTexture("miner-no-tracks");
        LoadTexture("tracks-0");
        LoadTexture("tracks-1");
        LoadTexture("tracks-2");
        LoadTexture("dark-screen");
        LoadTexture("light-mask");
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
        if (Keyboard.GetState().IsKeyDown(Keys.Space))
            _activeMiningControls.Add(MiningControls.Drill);

        _miningState.Update(_activeMiningControls, gameTime.ElapsedGameTime.Milliseconds);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _renderer.Render(_spriteBatch);

        base.Draw(gameTime);
    }
}