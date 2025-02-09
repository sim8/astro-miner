using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS;

/// <summary>
/// Base class for all ECS systems. Systems contain the logic for processing components.
/// </summary>
public abstract class System
{
    protected readonly Ecs Ecs;
    protected readonly GameState GameState;

    protected System(Ecs ecs, GameState gameState)
    {
        Ecs = ecs;
        GameState = gameState;
    }

    public abstract void Update(GameTime gameTime, HashSet<MiningControls> activeControls);
}