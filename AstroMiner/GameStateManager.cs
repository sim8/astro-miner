using System;
using System.Collections.Generic;
using AstroMiner.AsteroidWorld;
using AstroMiner.Definitions;
using AstroMiner.ECS;
using AstroMiner.HomeWorld;
using AstroMiner.StaticWorld;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public enum MiningControls
{
    // Movement
    MoveUp,
    MoveRight,
    MoveDown,
    MoveLeft,

    Drill,
    ExitVehicle,

    // Player-only
    UseItem,
    Interact,

    // Miner-only
    UseGrapple,

    ToggleInventory,
    NewGameOrReturnToBase, // TODO factor out
    SaveGame // TEMP
}

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
    public UI.UI Ui;

    public bool FreezeControls { get; set; }

    public Ecs Ecs { get; private set; }
    public GameTime GameTime { get; private set; }


    public BaseWorldState ActiveWorldState =>
        game.Model.ActiveWorld switch
        {
            World.Asteroid => AsteroidWorld,
            World.Home => HomeWorld,
            World.RigRoom => StaticWorld,
            World.Krevik => StaticWorld,
            World.MinEx => StaticWorld,
            _ => throw new Exception("Invalid world")
        };

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

    public void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        var controlsToUse = FreezeControls ? EmptyControls : activeMiningControls;
        GameTime = gameTime;

        if (!Ui.State.IsInMainMenu)
        {
            ActiveWorldState.Update(controlsToUse, gameTime);
            Camera.Update(gameTime, controlsToUse);
            CloudManager.Update(gameTime);
            Ecs.Update(gameTime, controlsToUse);

            if (activeMiningControls.Contains(MiningControls.ToggleInventory))
                Ui.State.IsInventoryOpen = !Ui.State.IsInventoryOpen;
        }


        Ui.Update(gameTime);

        if (activeMiningControls.Contains(MiningControls.SaveGame)) SaveGameTEMP();
    }
}