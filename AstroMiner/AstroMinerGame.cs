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
        LoadTexture("krevik-docks");
        LoadTexture("ship");
        LoadTexture("ship-shadow-map");
        LoadTexture("launch-console");
        LoadTexture("star");
        LoadTexture("lava");
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