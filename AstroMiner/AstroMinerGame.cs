using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public class AstroMinerGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private MiningState _miningState;
    private const int BaseCellSizePx = 64;
    private const int BaseMinerSizePx = 38;
    private const int SizeMultiplier = 4;
    private const int CellSizePx = BaseCellSizePx * SizeMultiplier;
    private const int MinerSizePx = BaseMinerSizePx * SizeMultiplier;
    
    private Texture2D _floor;
    private Texture2D _rock;
    private Texture2D _miner;

    public AstroMinerGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _miningState = new MiningState();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _floor = Content.Load<Texture2D>("img/floor");
        _rock = Content.Load<Texture2D>("img/rock");
        _miner = Content.Load<Texture2D>("img/miner");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            _miningState.AttemptMove(Direction.Top, gameTime.ElapsedGameTime.Milliseconds);
        }
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            _miningState.AttemptMove(Direction.Right, gameTime.ElapsedGameTime.Milliseconds);
        }
        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            _miningState.AttemptMove(Direction.Bottom, gameTime.ElapsedGameTime.Milliseconds);
        }
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            _miningState.AttemptMove(Direction.Left, gameTime.ElapsedGameTime.Milliseconds);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

        for (int row = 0; row < MiningState.Rows; row++)
        {
            for (int col = 0; col < MiningState.Columns; col++)
            {
                bool isRock = _miningState.GetCellState(row, col) == CellState.Rock;
                _spriteBatch.Draw(isRock ? _rock : _floor, new Rectangle(row * CellSizePx, col * CellSizePx, CellSizePx, CellSizePx), Color.White);
            }
        }
        int minerPosXPx = getScreenPxFromGridCoordinate(_miningState.MinerPos.X);
        int minerPosYPx = getScreenPxFromGridCoordinate(_miningState.MinerPos.Y);
        Rectangle sourceRectangle = new Rectangle(
            _miningState.MinerDirection == Direction.Top ||_miningState.MinerDirection == Direction.Left ? 0 : BaseMinerSizePx,
            _miningState.MinerDirection == Direction.Top ||_miningState.MinerDirection == Direction.Right ? 0 : BaseMinerSizePx,
            BaseMinerSizePx,
            BaseMinerSizePx);
        Rectangle destinationRectangle = new Rectangle(minerPosXPx, minerPosYPx, MinerSizePx, MinerSizePx);
        _spriteBatch.Draw(_miner, destinationRectangle, sourceRectangle, Color.White);
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }

    private int getScreenPxFromGridCoordinate(float gridCoordinate)
    {
        int basePxCoordinate = (int)(BaseCellSizePx * gridCoordinate);
        return basePxCoordinate * SizeMultiplier;
    }
}