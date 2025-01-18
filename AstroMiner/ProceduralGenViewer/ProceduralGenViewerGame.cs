using System.Collections.Generic;
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

public class ProceduralGenViewerGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly ProceduralGenViewerState _proceduralGenViewerState = new();

    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly ControlMapper<ViewerControls> _viewerControlsMapper = new();
    private GameState _gameState;
    private ProceduralGenViewerRenderer _renderer;
    private SpriteBatch _spriteBatch;


    public ProceduralGenViewerGame()
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
        _gameState = new GameState(_graphics);
        _renderer = new ProceduralGenViewerRenderer(_textures, _gameState, _proceduralGenViewerState);
        InitializeControls();
        base.Initialize();
    }

    private void InitializeControls()
    {
        _viewerControlsMapper.AddMapping(ViewerControls.NewAsteroid, Keys.N, false);
        _viewerControlsMapper.AddMapping(ViewerControls.ToggleLayers, Keys.L, false);
        _viewerControlsMapper.AddMapping(ViewerControls.ToggleWalls, Keys.W, false);
    }

    private void LoadTexture(string name)
    {
        _textures[name] = Content.Load<Texture2D>($"img/{name}");
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        LoadTexture("white");
        LoadTexture("dogica-font");
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var activeControls = _viewerControlsMapper.GetActiveControls(keyboardState);

        if (activeControls.Contains(ViewerControls.NewAsteroid)) _gameState.Initialize();
        if (activeControls.Contains(ViewerControls.ToggleWalls))
            _proceduralGenViewerState.showWalls = !_proceduralGenViewerState.showWalls;
        if (activeControls.Contains(ViewerControls.ToggleLayers))
            _proceduralGenViewerState.showLayers = !_proceduralGenViewerState.showLayers;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _renderer.Render(_spriteBatch);

        base.Draw(gameTime);
    }
}