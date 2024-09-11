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
        int minerPosXPx = (int)_miningState.MinerPos.X * CellSizePx;
        int minerPosYPx = (int)_miningState.MinerPos.Y * CellSizePx;
        Rectangle sourceRectangle = new Rectangle(BaseMinerSizePx, BaseMinerSizePx, BaseMinerSizePx, BaseMinerSizePx);
        Rectangle destinationRectangle = new Rectangle(minerPosXPx, minerPosYPx, MinerSizePx, MinerSizePx);
        _spriteBatch.Draw(_miner, destinationRectangle, sourceRectangle, Color.White);
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
}