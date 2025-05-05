using System.Collections.Generic;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class InteractionSystem : System
{
    public InteractionSystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }

    public int InteractableEntityId { get; set; } = -1;

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        CalculateInteractableEntityId();
    }

    private void CalculateInteractableEntityId()
    {
        InteractableEntityId = -1;
        var shortestDistance = float.MaxValue;
        foreach (var interactiveComponent in Ecs.GetAllComponentsInActiveWorld<InteractiveComponent>())
        {
            var positionComponent = Ecs.GetComponent<PositionComponent>(interactiveComponent.EntityId);

            var distance = EntityHelpers.GetDistanceBetween(Ecs, Ecs.PlayerEntityId.Value,
                interactiveComponent.EntityId);

            if (distance < interactiveComponent.InteractableDistance && distance < shortestDistance)
            {
                InteractableEntityId = interactiveComponent.EntityId;
                shortestDistance = distance;
            }
        }
    }
}