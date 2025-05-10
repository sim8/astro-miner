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

    public int InteractableEntityId { get; private set; } = -1;

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        CalculateInteractableEntityId();
        if (activeControls.Contains(MiningControls.Interact))
        {
            if (InteractableEntityId != -1)
                HandleInteraction();
            else if (game.StateManager.AsteroidWorld.IsInMiner)
                // Kind of unrelated to interactions but handled here for simplicity
                // (uses same control and shouldn't fire if just boarded)
                ExitVehicle();
        }
    }

    private void HandleInteraction()
    {
        if (InteractableEntityId == Ecs.MinerEntityId) Ecs.SetActiveControllableEntity(Ecs.MinerEntityId.Value);

        var npc = Ecs.GetComponent<NpcComponent>(InteractableEntityId);

        if (npc is { Npc: Npc.MinExMerchant }) game.StateManager.Ui.State.IsInDialog = true;
    }

    private void CalculateInteractableEntityId()
    {
        InteractableEntityId = -1;
        if (game.StateManager.Ecs.ActiveControllableEntityId != game.StateManager.Ecs.PlayerEntityId) return;
        var shortestDistance = float.MaxValue;

        foreach (var interactiveComponent in Ecs.GetAllComponentsInActiveWorld<InteractiveComponent>())
        {
            if (interactiveComponent.EntityId == Ecs.ActiveControllableEntityId) continue;

            var distance = EntityHelpers.GetDistanceBetween(Ecs, Ecs.PlayerEntityId.Value,
                interactiveComponent.EntityId);

            if (distance < interactiveComponent.InteractableDistance && distance < shortestDistance)
            {
                InteractableEntityId = interactiveComponent.EntityId;
                shortestDistance = distance;
            }
        }
    }

    private void ExitVehicle()
    {
        // Set player as active controllable entity
        Ecs.SetActiveControllableEntity(Ecs.PlayerEntityId.Value);

        var minerPosition = Ecs.GetComponent<PositionComponent>(Ecs.MinerEntityId.Value).Position;

        // Set player position to miner position
        var playerPosition = Ecs.GetComponent<PositionComponent>(Ecs.PlayerEntityId.Value);
        playerPosition.Position = minerPosition;
    }
}