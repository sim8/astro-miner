using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AstroMiner.ProceduralGenViewer;

public class ProceduralGenViewerState
{
    public bool showLayers;
    public bool showWalls = true;
}

public enum ViewerControls
{
    NewAsteroid,
    ToggleLayers,
    ToggleWalls
}

public class ProceduralGenViewerGame : BaseGame
{
    private readonly ProceduralGenViewerState _proceduralGenViewerState = new();
    private readonly ControlMapper<ViewerControls> _viewerControlsMapper = new();
    private ProceduralGenViewerRenderer _renderer;

    protected override void Initialize()
    {
        StateManager = new GameStateManager(this);
        StateManager.Initialize();
        InitializeAsteroidForViewing();
        _renderer = new ProceduralGenViewerRenderer(Textures, StateManager, _proceduralGenViewerState);
        InitializeControls();
        base.Initialize();
    }

    private void InitializeAsteroidForViewing()
    {
        StateManager.Initialize();
        StateManager.SetActiveWorldAndInitialize(World.Asteroid);
    }

    protected override void InitializeControls()
    {
        _viewerControlsMapper.AddMapping(ViewerControls.NewAsteroid, Keys.N, Buttons.A, false);
        _viewerControlsMapper.AddMapping(ViewerControls.ToggleLayers, Keys.L, Buttons.RightShoulder, false);
        _viewerControlsMapper.AddMapping(ViewerControls.ToggleWalls, Keys.W, Buttons.LeftShoulder, false);
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        LoadTexture("white");
        LoadTexture("dogica-font");
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var gamePadState = GamePad.GetState(PlayerIndex.One);
        var activeControls = _viewerControlsMapper.GetActiveControls(keyboardState, gamePadState);

        if (activeControls.Contains(ViewerControls.NewAsteroid))
            InitializeAsteroidForViewing();
        if (activeControls.Contains(ViewerControls.ToggleWalls))
            _proceduralGenViewerState.showWalls = !_proceduralGenViewerState.showWalls;
        if (activeControls.Contains(ViewerControls.ToggleLayers))
            _proceduralGenViewerState.showLayers = !_proceduralGenViewerState.showLayers;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _renderer.Render(SpriteBatch);
        base.Draw(gameTime);
    }
}