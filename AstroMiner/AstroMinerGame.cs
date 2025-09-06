using AstroMiner.Definitions;
using AstroMiner.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner;

public class AstroMinerGame : BaseGame
{
    private ControlManager _controlManager;
    private MouseState _prevMouseState;

    private Renderer _renderer;

    protected override void Initialize()
    {
        _controlManager = new ControlManager(this);
        StateManager = new GameStateManager(this);
        StateManager.Initialize();
        _renderer = new Renderer(this);
        Window.ClientSizeChanged += _renderer.HandleWindowResize;
        base.Initialize();
    }

    protected override void InitializeControls()
    {
        // Control initialization moved to ControlManager
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        foreach (var texture in Tx.AllTextures)
        {
            LoadTexture(texture);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var gamePadState = GamePad.GetState(PlayerIndex.One);

        var activeControls = _controlManager.Update(keyboardState, gamePadState);

        StateManager.Update(activeControls, gameTime);

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


        GraphicsDevice.Clear(Colors.VeryDarkBlue);

        _renderer.Render(SpriteBatch);

        base.Draw(gameTime);
    }
}