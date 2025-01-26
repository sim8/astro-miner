using System.Collections.Generic;
using AstroMiner.Asteroid;
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

    public GameState(
        GraphicsDeviceManager graphics)
    {
        Graphics = graphics;
        Initialize();
        InitializeAsteroid();
    }

    public long MsSinceStart { get; private set; }

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
        CloudManager = new CloudManager(this);
        IsOnAsteroid = false;
    }

    public void Update(HashSet<MiningControls> activeMiningControls, int elapsedMs)
    {
        if (IsOnAsteroid) Asteroid.Update(activeMiningControls, elapsedMs);

        Camera.Update(elapsedMs, activeMiningControls);

        CloudManager.Update(elapsedMs);
    }
}