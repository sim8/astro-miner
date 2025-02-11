using System.Collections.Generic;
using AstroMiner.AsteroidWorld;
using AstroMiner.Definitions;
using AstroMiner.Entities;
using AstroMiner.HomeWorld;
using AstroMiner.ECS;
using Microsoft.Xna.Framework;
using System;

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

    CycleZoom,
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
    public readonly GraphicsDeviceManager Graphics;
    public AsteroidWorldState AsteroidWorld;
    public CameraState Camera;
    public CloudManager CloudManager;
    public HomeWorldState HomeWorld;
    public Inventory Inventory;
    public bool IsOnAsteroid;
    public Ecs Ecs { get; private set; }

    public GameState(
        GraphicsDeviceManager graphics)
    {
        Graphics = graphics;
        Initialize();
        InitializeAsteroid();
        // HomeWorld.Initialize();
    }

    public long MsSinceStart { get; private set; }

    public World ActiveWorld { get; private set; }


    public BaseWorldState ActiveWorldState =>
        ActiveWorld switch
        {
            World.Asteroid => AsteroidWorld,
            World.Home => HomeWorld,
            _ => throw new Exception("Invalid world")
        };

    public void InitializeAsteroid()
    {
        ActiveWorld = World.Asteroid;
        AsteroidWorld.Initialize();
    }

    public void InitializeHome()
    {
        ActiveWorld = World.Home;
    }

    public void Initialize()
    {
        Inventory = new Inventory();
        Camera = new CameraState(this);
        MsSinceStart = 0;
        AsteroidWorld = new AsteroidWorldState(this);
        HomeWorld = new HomeWorldState(this);
        CloudManager = new CloudManager(this);
        Ecs = new Ecs(this);
    }

    public void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        ActiveWorldState.Update(activeMiningControls, gameTime);

        Camera.Update(gameTime, activeMiningControls);
        CloudManager.Update(gameTime);
        Ecs.Update(gameTime, activeMiningControls);
    }
}