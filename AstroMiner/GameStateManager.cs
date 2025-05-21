using System;
using System.Collections.Generic;
using AstroMiner.AsteroidWorld;
using AstroMiner.Definitions;
using AstroMiner.ECS;
using AstroMiner.HomeWorld;
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
    public CloudManager CloudManager;
    public HomeWorldState HomeWorld;
    public Inventory Inventory;
    public StaticWorldState StaticWorld;
    public TransitionManager TransitionManager;
    public UI.UI Ui;

    public Ecs Ecs { get; private set; }
    public GameTime GameTime { get; private set; } = new();


    public BaseWorldState ActiveWorldState => GetWorldState(game.Model.ActiveWorld);

    public BaseWorldState GetWorldState(World world)
    {
        return world switch
        {
            World.Asteroid => AsteroidWorld,
            World.Home => HomeWorld, // TODO deprecate
            _ => StaticWorld
        };
    }

    public void SetActiveWorldAndInitialize(World world)
    {
        game.Model.ActiveWorld = world;
        if (world == World.Asteroid)
            AsteroidWorld.Initialize();
        else if (world == World.Home)
            HomeWorld.Initialize();
        else
            StaticWorld.Initialize();
    }

    public void Initialize()
    {
        Inventory = new Inventory(game);
        Camera = new CameraState(game);
        AsteroidWorld = new AsteroidWorldState(game);
        HomeWorld = new HomeWorldState(game);
        StaticWorld = new StaticWorldState(game);
        CloudManager = new CloudManager(game);
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

    private void SaveGameTEMP()
    {
        game.GameStateStorage.SaveState(game.Model);
        Console.WriteLine("saved!");
    }

    public long GetTotalPlayTime()
    {
        // TODO doesn't take menu time into account. Revert to manually counting?
        return game.Model.SavedTotalPlaytimeMs + (long)GameTime.TotalGameTime.TotalMilliseconds;
    }

    public void Update(ActiveControls activeControls, GameTime gameTime)
    {
        GameTime = gameTime;

        if (!Ui.State.IsInMainMenu)
        {
            ActiveWorldState.Update(activeControls, gameTime); // TODO only utilized by AsteroidWorld

            Camera.Update(gameTime, activeControls);
            CloudManager.Update(gameTime);
            TransitionManager.Update(gameTime);
            Ecs.Update(gameTime, activeControls);

            // TODO move into UIState Updater?
            if (activeControls.Global.Contains(GlobalControls.ToggleMenu))
                Ui.State.IsInventoryOpen = !Ui.State.IsInventoryOpen;
        }


        Ui.Update(gameTime);

        if (activeControls.Mining.Contains(MiningControls.SaveGame)) SaveGameTEMP();
    }
}