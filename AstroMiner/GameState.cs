using System.Collections.Generic;
using AstroMiner.Asteroid;
using AstroMiner.Entities;
using AstroMiner.World;
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
    public Inventory Inventory;
    public bool IsOnAsteroid;
    public WorldState World;

    public GameState(
        GraphicsDeviceManager graphics)
    {
        Graphics = graphics;
        Initialize();
        // InitializeAsteroid();
        World.Initialize();
    }

    public long MsSinceStart { get; private set; }

    // TODO improve this
    public MiningControllableEntity ActiveControllableEntity =>
        !IsOnAsteroid ? World.Player : Asteroid.IsInMiner ? Asteroid.Miner : Asteroid.Player;

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
        World = new WorldState(this);
        CloudManager = new CloudManager(this);
        IsOnAsteroid = false;
    }

    public void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
        if (IsOnAsteroid)
            Asteroid.Update(activeMiningControls, gameTime);
        else
            World.Update(activeMiningControls, gameTime);

        Camera.Update(gameTime, activeMiningControls);

        CloudManager.Update(gameTime);
    }
}