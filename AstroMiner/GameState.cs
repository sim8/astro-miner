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
    PlaceDynamite,

    // Miner-only
    UseGrapple,

    NewGameOrReturnToBase // TODO factor out
}

public enum Direction
{
    Top,
    Right,
    Bottom,
    Left
}

public class GameState
{
    private static readonly HashSet<MiningControls> EmptyControls = new();
    public readonly GraphicsDeviceManager Graphics;
    public AsteroidWorldState AsteroidWorld;
    public CameraState Camera;
    public CloudManager CloudManager;
    public HomeWorldState HomeWorld;
    public InteriorsWorldState InteriorsWorld;
    public Inventory Inventory;

    public GameState(
        GraphicsDeviceManager graphics)
    {
        Graphics = graphics;
        Initialize();
    }

    public bool FreezeControls { get; set; }

    public Ecs Ecs { get; private set; }
    public GameTime GameTime { get; private set; }

    public long MsSinceStart { get; private set; }

    public World ActiveWorld { get; private set; }


    public BaseWorldState ActiveWorldState =>
        ActiveWorld switch
        {
            World.Asteroid => AsteroidWorld,
            World.Home => HomeWorld,
            World.RigRoom => InteriorsWorld,
            _ => throw new Exception("Invalid world")
        };

    public void SetActiveWorldAndInitialize(World world)
    {
        ActiveWorld = world;
        if (world == World.Asteroid)
            AsteroidWorld.Initialize();
        else if (world == World.Home)
            HomeWorld.Initialize();
        else
            InteriorsWorld.Initialize();
    }

    public void Initialize()
    {
        Inventory = new Inventory();
        Camera = new CameraState(this);
        MsSinceStart = 0;
        AsteroidWorld = new AsteroidWorldState(this);
        HomeWorld = new HomeWorldState(this);
        InteriorsWorld = new InteriorsWorldState(this);
        CloudManager = new CloudManager(this);
        Ecs = new Ecs(this);

        SetActiveWorldAndInitialize(World.Home);
        HomeWorld.InitializeOrResetEntities();
    }

    public void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        var controlsToUse = FreezeControls ? EmptyControls : activeMiningControls;
        GameTime = gameTime;
        ActiveWorldState.Update(controlsToUse, gameTime);

        Camera.Update(gameTime, controlsToUse);
        CloudManager.Update(gameTime);
        Ecs.Update(gameTime, controlsToUse);
    }
}