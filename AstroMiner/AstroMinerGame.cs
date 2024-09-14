using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public class AstroMinerGame : Game
{
    // TODO these should live elsewhere but needed by a few places
    private const int MinerTextureSizePx = 38;
    private const int ScaleMultiplier = 1;
    private const int CellTextureSizePx = 64;

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
        _miningState = new MiningState(MinerTextureSizePx, CellTextureSizePx);
        _renderer = new Renderer(_graphics, _textures, _miningState, ScaleMultiplier, MinerTextureSizePx,
            CellTextureSizePx);
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
        LoadTexture("rock");
        LoadTexture("miner");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.W))
            _miningState.MoveMiner(Direction.Top, gameTime.ElapsedGameTime.Milliseconds);
        else if (Keyboard.GetState().IsKeyDown(Keys.D))
            _miningState.MoveMiner(Direction.Right, gameTime.ElapsedGameTime.Milliseconds);
        else if (Keyboard.GetState().IsKeyDown(Keys.S))
            _miningState.MoveMiner(Direction.Bottom, gameTime.ElapsedGameTime.Milliseconds);
        else if (Keyboard.GetState().IsKeyDown(Keys.A))
            _miningState.MoveMiner(Direction.Left, gameTime.ElapsedGameTime.Milliseconds);

        if (Keyboard.GetState().IsKeyDown(Keys.Space))
            _miningState.UseDrill(gameTime.ElapsedGameTime.Milliseconds);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

        _renderer.Render(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}