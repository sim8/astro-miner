﻿using AstroMiner.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public class AstroMinerGame : BaseGame
{
    private readonly ControlMapper<MiningControls> _miningControlMapper = new();
    private MouseState _prevMouseState;

    private Renderer _renderer;

    protected override void Initialize()
    {
        StateManager = new GameStateManager(this);
        StateManager.Initialize();
        _renderer = new Renderer(this);
        Window.ClientSizeChanged += _renderer.HandleWindowResize;
        InitializeControls();
        base.Initialize();
    }

    protected override void InitializeControls()
    {
        _miningControlMapper.AddMapping(MiningControls.MoveUp, Keys.W, Buttons.LeftThumbstickUp, true);
        _miningControlMapper.AddMapping(MiningControls.MoveRight, Keys.D, Buttons.LeftThumbstickRight, true);
        _miningControlMapper.AddMapping(MiningControls.MoveDown, Keys.S, Buttons.LeftThumbstickDown, true);
        _miningControlMapper.AddMapping(MiningControls.MoveLeft, Keys.A, Buttons.LeftThumbstickLeft, true);
        _miningControlMapper.AddMapping(MiningControls.Drill, Keys.Space, Buttons.RightTrigger, true);
        _miningControlMapper.AddMapping(MiningControls.UseItem, Keys.Space, Buttons.RightTrigger, true);
        _miningControlMapper.AddMapping(MiningControls.Interact, Keys.E, Buttons.Y, false);
        _miningControlMapper.AddMapping(MiningControls.ExitVehicle, Keys.E, Buttons.Y, false);
        _miningControlMapper.AddMapping(MiningControls.UseGrapple, Keys.G, Buttons.LeftTrigger, true);
        _miningControlMapper.AddMapping(MiningControls.NewGameOrReturnToBase, Keys.N, Buttons.Start, false);
        _miningControlMapper.AddMapping(MiningControls.SaveGame, Keys.B, Buttons.Back, false); // TEMP
        _miningControlMapper.AddMapping(MiningControls.ToggleInventory, Keys.Tab, Buttons.Back, false);
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
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
        LoadTexture("min-ex");
        LoadTexture("icons");
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var gamePadState = GamePad.GetState(PlayerIndex.One);

        var activeMiningControls = _miningControlMapper.GetActiveControls(keyboardState, gamePadState);

        StateManager.Update(activeMiningControls, gameTime);

        var mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released)
            StateManager.Ui.OnMouseClick(mouseState.X, mouseState.Y);

        _prevMouseState = mouseState;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        FrameCounter.Update(deltaTime);


        GraphicsDevice.Clear(Color.Black);

        _renderer.Render(SpriteBatch);

        base.Draw(gameTime);
    }
}