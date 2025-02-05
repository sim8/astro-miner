using System.Collections.Generic;
using AstroMiner.AsteroidWorld;
using AstroMiner.Definitions;
using AstroMiner.Entities;
using AstroMiner.HomeWorld;
using AstroMiner.ECS;
using AstroMiner.ECS.Systems;
using AstroMiner.ECS.Components;
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
    public World EcsWorld { get; private set; }
    public DynamiteSystem DynamiteSystem { get; private set; }
    public ExplosionSystem ExplosionSystem { get; private set; }
    public MovementSystem MovementSystem { get; private set; }

    public GameState(
        GraphicsDeviceManager graphics)
    {
        Graphics = graphics;
        Initialize();
        InitializeAsteroid();
        // HomeWorld.Initialize();
    }

    public long MsSinceStart { get; private set; }

    public MiningControllableEntity ActiveControllableEntity =>
        ActiveWorld.ActiveControllableEntity;

    public BaseWorldState ActiveWorld => IsOnAsteroid ? AsteroidWorld : HomeWorld;

    public void InitializeAsteroid()
    {
        IsOnAsteroid = true;
        AsteroidWorld.Initialize();
    }

    public void InitializeHome()
    {
        IsOnAsteroid = false;
    }

    public void Initialize()
    {
        Inventory = new Inventory();
        Camera = new CameraState(this);
        MsSinceStart = 0;
        AsteroidWorld = new AsteroidWorldState(this);
        HomeWorld = new HomeWorldState(this);
        CloudManager = new CloudManager(this);
        IsOnAsteroid = false;
        EcsWorld = new World(this);
        DynamiteSystem = new DynamiteSystem(EcsWorld, this);
        ExplosionSystem = new ExplosionSystem(EcsWorld, this);
        MovementSystem = new MovementSystem(EcsWorld, this);
    }

    public void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (IsOnAsteroid)
            AsteroidWorld.Update(activeMiningControls, gameTime);
        else
            HomeWorld.Update(activeMiningControls, gameTime);

        Camera.Update(gameTime, activeMiningControls);

        CloudManager.Update(gameTime);
        
        // Update ECS systems
        DynamiteSystem.Update(gameTime, activeMiningControls);
        ExplosionSystem.Update(gameTime, activeMiningControls);
        MovementSystem.Update(gameTime, activeMiningControls);
    }
}