namespace AstroMiner.ECS;

/// <summary>
///     Base class for all ECS components. Components are pure data containers.
/// </summary>
public abstract class Component
{
    public int EntityId { get; init; }
}