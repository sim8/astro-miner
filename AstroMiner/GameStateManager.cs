using System.Collections.Generic;
using AstroMiner.AsteroidWorld;
using AstroMiner.Definitions;
using AstroMiner.ECS;
using AstroMiner.StaticWorld;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public enum Direction
{
    Top,
    Right,
    Bottom,
    Left
}

public class GameStateManager(BaseGame game)
{
    private static readonly HashSet<MiningControls> EmptyControls = new();

    private NewGameManager _newGameManager;
    public AsteroidWorldState AsteroidWorld;
    public CameraState Camera;
    public Inventory Inventory;
    public StaticWorldState StaticWorld;
    public TransitionManager TransitionManager;
    public UI.UI Ui;

    public Ecs Ecs { get; private set; }
    public GameTime GameTime { get; private set; } = new();


    public BaseWorldState ActiveWorldState => GetWorldState(game.Model.ActiveWorld);

    public bool IsInGame => !Ui.State.IsInMainMenu && !Ui.State.IsInPauseMenu;

    public BaseWorldState GetWorldState(World world)
    {
        return world switch
        {
            World.Asteroid => AsteroidWorld,
            _ => StaticWorld
        };
    }

    public void SetActiveWorldAndInitialize(World world)
    {
        game.Model.ActiveWorld = world;
        if (world == World.Asteroid)
            AsteroidWorld.Initialize();
        else
            StaticWorld.Initialize();
    }

    public void Initialize()
    {
        Inventory = new Inventory(game);
        Camera = new CameraState(game);
        AsteroidWorld = new AsteroidWorldState(game);
        StaticWorld = new StaticWorldState(game);
        TransitionManager = new TransitionManager(game);
        Ecs = new Ecs(game);
        Ui = new UI.UI(game);
        _newGameManager = new NewGameManager(game);
    }

    public void LoadGame()
    {
        game.Model = game.GameStateStorage.LoadState();
        InitializeInGame();
    }

    public void NewGame()
    {
        _newGameManager.SetUpNewGame();
        InitializeInGame();
    }

    private void InitializeInGame()
    {
        SetActiveWorldAndInitialize(game.Model.ActiveWorld);
        game.StateManager.Ui.State.IsInMainMenu = false;
    }

    public void SaveGame()
    {
        game.GameStateStorage.SaveState(game.Model);
    }

    public long GetTotalPlayTime()
    {
        // TODO doesn't take menu time into account. Revert to manually counting?
        return game.Model.SavedTotalPlaytimeMs + (long)GameTime.TotalGameTime.TotalMilliseconds;
    }

    private void UpdateInGame(ActiveControls activeControls, GameTime gameTime)
    {
        ActiveWorldState.Update(activeControls, gameTime); // TODO only utilized by AsteroidWorld

        Camera.Update(gameTime, activeControls);
        TransitionManager.Update(gameTime);
        Ecs.Update(gameTime, activeControls);
    }

    public void Update(ActiveControls activeControls, GameTime gameTime)
    {
        GameTime = gameTime;

        if (IsInGame) UpdateInGame(activeControls, gameTime);

        Ui.Update(gameTime, activeControls);
    }
}