using System.Collections.Generic;
using AstroMiner.AsteroidWorld;
using AstroMiner.Entities;
using AstroMiner.HomeWorld;
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
    NewGame // TODO factor out
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
    public AsteroidState Asteroid;
    public CameraState Camera;
    public CloudManager CloudManager;
    public HomeWorldState HomeWorld;
    public Inventory Inventory;
    public bool IsOnAsteroid;

    public GameState(
        GraphicsDeviceManager graphics)
    {
        Graphics = graphics;
        Initialize();
        // InitializeAsteroid();
        HomeWorld.Initialize();
    }

    public long MsSinceStart { get; private set; }

    // TODO improve this
    public MiningControllableEntity ActiveControllableEntity =>
        !IsOnAsteroid ? HomeWorld.Player : Asteroid.IsInMiner ? Asteroid.Miner : Asteroid.Player;

    public void InitializeAsteroid()
    {
        IsOnAsteroid = true;
        Asteroid.Initialize();
    }

    public void Initialize()
    {
        Inventory = new Inventory();
        Camera = new CameraState(this);
        MsSinceStart = 0;
        Asteroid = new AsteroidState(this);
        HomeWorld = new HomeWorldState(this);
        CloudManager = new CloudManager(this);
        IsOnAsteroid = false;
    }

    public void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (IsOnAsteroid)
            Asteroid.Update(activeMiningControls, gameTime);
        else
            HomeWorld.Update(activeMiningControls, gameTime);

        Camera.Update(gameTime, activeMiningControls);

        CloudManager.Update(gameTime);
    }
}