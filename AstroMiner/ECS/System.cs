using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS;

/// <summary>
/// Base class for all ECS systems. Systems contain the logic for processing components.
/// </summary>
public abstract class System
{
    protected readonly World World;
    protected readonly GameState GameState;

    protected System(World world, GameState gameState)
    {
        World = world;
        GameState = gameState;
    }

    public abstract void Update(GameTime gameTime, HashSet<MiningControls> activeControls);
} 