using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public class AstroMinerGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private MiningState _miningState;
    private Renderer _renderer;
    private SpriteBatch _spriteBatch;

    private readonly Dictionary<string, Texture2D> _textures = new();


    public AstroMinerGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _miningState = new MiningState();
        _renderer = new Renderer(_graphics, _textures);
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
            _miningState.AttemptMove(Direction.Top, gameTime.ElapsedGameTime.Milliseconds);
        else if (Keyboard.GetState().IsKeyDown(Keys.D))
            _miningState.AttemptMove(Direction.Right, gameTime.ElapsedGameTime.Milliseconds);
        else if (Keyboard.GetState().IsKeyDown(Keys.S))
            _miningState.AttemptMove(Direction.Bottom, gameTime.ElapsedGameTime.Milliseconds);
        else if (Keyboard.GetState().IsKeyDown(Keys.A))
            _miningState.AttemptMove(Direction.Left, gameTime.ElapsedGameTime.Milliseconds);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

        _renderer.Render(_spriteBatch, _miningState);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}