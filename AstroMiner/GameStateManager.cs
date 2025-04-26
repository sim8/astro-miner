using System;
using System.Collections.Generic;
using AstroMiner.AsteroidWorld;
using AstroMiner.Definitions;
using AstroMiner.ECS;
using AstroMiner.HomeWorld;
using AstroMiner.InteriorsWorld;
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
    EnterOrExit,

    // Player-only
    UseItem,

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
    public AsteroidWorldState AsteroidWorld;
    public CameraState Camera;
    public CloudManager CloudManager;
    public HomeWorldState HomeWorld;
    public InteriorsWorldState InteriorsWorld;
    public Inventory Inventory;
    public UI.UI Ui;

    public bool FreezeControls { get; set; }

    public Ecs Ecs { get; private set; }
    public GameTime GameTime { get; private set; }

    public long MsSinceStart { get; private set; }


    public BaseWorldState ActiveWorldState =>
        game.Model.ActiveWorld switch
        {
            World.Asteroid => AsteroidWorld,
            World.Home => HomeWorld,
            World.RigRoom => InteriorsWorld,
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
            InteriorsWorld.Initialize();
    }

    // TODO probably lots?
    private void SetUpNewGame()
    {
        HomeWorld.InitializeOrResetEntities();
    }

    public void Initialize()
    {
        // game.Model = GameModelHelpers.CreateNewGameModel();
        game.Model = game.GameStateStorage.LoadState();
        Inventory = new Inventory(game);
        Camera = new CameraState(game);
        MsSinceStart = 0;
        AsteroidWorld = new AsteroidWorldState(game);
        HomeWorld = new HomeWorldState(game);
        InteriorsWorld = new InteriorsWorldState(game);
        CloudManager = new CloudManager(game);
        Ecs = new Ecs(game);
        Ui = new UI.UI(game);

        if (!game.StateManager.Ecs.PlayerEntityId.HasValue) SetUpNewGame();

        SetActiveWorldAndInitialize(game.Model.ActiveWorld);
    }

    private void SaveGameTEMP()
    {
        game.GameStateStorage.SaveState(game.Model);
        Console.WriteLine("saved!");
    }

    public void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        var controlsToUse = FreezeControls ? EmptyControls : activeMiningControls;
        GameTime = gameTime;
        ActiveWorldState.Update(controlsToUse, gameTime);

        Camera.Update(gameTime, controlsToUse);
        CloudManager.Update(gameTime);
        Ecs.Update(gameTime, controlsToUse);
        Ui.Update(gameTime);

        if (activeMiningControls.Contains(MiningControls.SaveGame)) SaveGameTEMP();
        if (activeMiningControls.Contains(MiningControls.ToggleInventory))
            Ui.State.IsInventoryOpen = !Ui.State.IsInventoryOpen;
    }
}