using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.UI;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class InteractionSystem : System
{
    public InteractionSystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }

    public int InteractableEntityId { get; private set; } = -1;

    public override void Update(GameTime gameTime, ActiveControls activeControls)
    {
        CalculateInteractableEntityId();
        if (activeControls.Mining.Contains(MiningControls.Interact))
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
        if (InteractableEntityId == Ecs.MinerEntityId)
        {
            Ecs.SetActiveControllableEntity(Ecs.MinerEntityId.Value);
            return;
        }

        var interactiveType = Ecs.GetComponent<InteractiveComponent>(InteractableEntityId).InteractiveType;

        if (interactiveType == InteractiveType.LaunchConsole)
        {
            game.StateManager.Ui.State.ActiveScreen = Screen.LaunchConsole;
            return;
        }

        if (interactiveType == InteractiveType.Shop)
        {
            game.StateManager.Ui.State.ActiveScreen = Screen.SaleMenu;
            return;
        }

        if (interactiveType == InteractiveType.Merchant)
        {
            game.StateManager.Ui.State.ActiveScreen = Screen.MerchantMenu;
            game.StateManager.Ui.State.ActiveMerchantType = Ecs.GetComponent<InteractiveComponent>(InteractableEntityId).MerchantType;
            return;
        }

        var npc = Ecs.GetComponent<NpcComponent>(InteractableEntityId);

        if (npc is { Npc: Npc.MinExMerchant }) game.StateManager.Ui.State.IsInDialog = true;
    }

    // TODO activeWorld hardcoded - change if NPCs use this
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
        var minerDirection = game.StateManager.Ecs.GetComponent<DirectionComponent>(Ecs.MinerEntityId.Value);

        var playerPosition = Ecs.GetComponent<PositionComponent>(Ecs.PlayerEntityId.Value);
        var playerDirection = game.StateManager.Ecs.GetComponent<DirectionComponent>(Ecs.PlayerEntityId.Value);

        // Prefer getting out left side of vehicle.
        // If vehicle facing right, get out right side so player faces camera
        Direction[] exitDirectionPriority = minerDirection.Direction == Direction.Right
            ? [Direction.Right, Direction.Left, Direction.Bottom]
            : [Direction.Left, Direction.Right, Direction.Bottom];

        foreach (var dir in exitDirectionPriority)
        {
            var exitSuccess = Ecs.MovementSystem.SetPositionRelativeToDirectionalEntity(playerPosition,
                Ecs.MinerEntityId.Value,
                dir, false, true);
            if (exitSuccess)
            {
                // Face away from vehicle
                playerDirection.Direction = MovementSystem.GetRotatedDirection(dir, minerDirection.Direction);
                return;
            }
        }

        // Fallback in case of collisions on all sides
        playerPosition.Position = minerPosition;
        playerDirection.Direction = Direction.Bottom;
    }
}