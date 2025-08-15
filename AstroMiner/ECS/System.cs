using Microsoft.Xna.Framework;

namespace AstroMiner.ECS;

/// <summary>
///     Base class for all ECS systems. Systems contain the logic for processing components.
/// </summary>
public abstract class System
{
    protected readonly Ecs Ecs;
    protected readonly BaseGame game;

    protected System(Ecs ecs, BaseGame _game)
    {
        Ecs = ecs;
        game = _game;
    }

    public abstract void Update(GameTime gameTime, ActiveControls activeControls);
}