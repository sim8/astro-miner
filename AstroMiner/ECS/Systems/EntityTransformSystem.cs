using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class EntityTransformSystem : System
{
    public EntityTransformSystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }


    public override void Update(GameTime gameTime, ActiveControls activeControls)
    {
    }

    public void MoveMinerAndPlayerToAsteroid()
    {
        MoveMinerToAsteroid(game.Model.Asteroid.MinerStartingPos);
        MovePlayerToAsteroid(game.Model.Asteroid.MinerStartingPos);
    }

    public void MoveMinerAndPlayerToShipDownstairs()
    {
        MoveMinerToShipDownstairs();
        MovePlayerToShipDownstairs();
    }

    private void MoveMinerToAsteroid(Vector2 minerCenterPos)
    {
        var entityId = game.Model.Ecs.MinerEntityId.Value;

        var minerPosition = game.StateManager.Ecs.GetComponent<PositionComponent>(entityId);
        minerPosition.SetCenterPosition(minerCenterPos);
        minerPosition.World = World.Asteroid;

        // Add movement component with miner-specific values
        var movementComponent = game.StateManager.Ecs.AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = GameConfig.Speeds.Running;
        movementComponent.TimeToReachMaxSpeedMs = 600; // From MinerEntity
        movementComponent.TimeToStopMs = 400; // From MinerEntity

        // Add health component
        var healthComponent = game.StateManager.Ecs.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.MinerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.MinerMaxHealth;

        // Add mining component
        var miningComponent = game.StateManager.Ecs.AddComponent<MiningComponent>(entityId);
        miningComponent.DrillingWidth = 0.9f;

        // Add grapple component
        game.StateManager.Ecs.AddComponent<GrappleComponent>(entityId);

        // Add directional light source component
        var directionalLightSourceComponent =
            game.StateManager.Ecs.AddComponent<DirectionalLightSourceComponent>(entityId);
        directionalLightSourceComponent.TopOffset = new Vector2(1.06f, 0.34f);
        directionalLightSourceComponent.RightOffset = new Vector2(0.70f, 0.66f);
        directionalLightSourceComponent.BottomOffset = new Vector2(0.12f, 0.58f);
        directionalLightSourceComponent.LeftOffset = new Vector2(0.48f, -0.28f);

        // TODO needed?
        game.StateManager.Ecs.SetActiveControllableEntity(game.Model.Ecs.MinerEntityId.Value);
    }

    private void MovePlayerToAsteroid(Vector2 playerCenterPos)
    {
        var entityId = game.Model.Ecs.PlayerEntityId.Value;

        var playerPosition = game.StateManager.Ecs.GetComponent<PositionComponent>(entityId);
        playerPosition.SetCenterPosition(playerCenterPos);
        playerPosition.World = World.Asteroid;

        // Add health component
        var healthComponent = game.StateManager.Ecs.AddComponent<HealthComponent>(entityId);
        healthComponent.MaxHealth = GameConfig.PlayerMaxHealth;
        healthComponent.CurrentHealth = GameConfig.PlayerMaxHealth;

        // Add mining component
        game.StateManager.Ecs.AddComponent<MiningComponent>(entityId);

        // Add directional light source component
        var directionalLightSourceComponent =
            game.StateManager.Ecs.AddComponent<DirectionalLightSourceComponent>(entityId);
        directionalLightSourceComponent.TopOffset = new Vector2(0.28f, -0.30f);
        directionalLightSourceComponent.RightOffset = new Vector2(0.32f, -0.30f);
        directionalLightSourceComponent.BottomOffset = new Vector2(0.24f, -0.30f);
        directionalLightSourceComponent.LeftOffset = new Vector2(0.26f, -0.28f);
    }

    private void MoveMinerToShipDownstairs()
    {
        var minerEntityId = game.StateManager.Ecs.MinerEntityId.Value;

        var minerPosition = game.StateManager.Ecs.GetComponent<PositionComponent>(minerEntityId);
        minerPosition.SetCenterPosition(new Vector2(34f, 6f));
        minerPosition.IsOffAsteroid = false;
        minerPosition.World = World.ShipDownstairs;
        var minerDirection = game.StateManager.Ecs.GetComponent<DirectionComponent>(minerEntityId);
        minerDirection.Direction = Direction.Right;

        // Remove components that were added in AsteroidWorld
        game.StateManager.Ecs.RemoveComponent<MovementComponent>(minerEntityId);
        game.StateManager.Ecs.RemoveComponent<HealthComponent>(minerEntityId);
        game.StateManager.Ecs.RemoveComponent<MiningComponent>(minerEntityId);
        game.StateManager.Ecs.RemoveComponent<GrappleComponent>(minerEntityId);
        game.StateManager.Ecs.RemoveComponent<DirectionalLightSourceComponent>(minerEntityId);
    }

    private void MovePlayerToShipDownstairs()
    {
        var playerEntityId = game.StateManager.Ecs.PlayerEntityId.Value;

        var playerPosition = game.StateManager.Ecs.GetComponent<PositionComponent>(playerEntityId);
        playerPosition.SetCenterPosition(new Vector2(34f, 7.5f));
        playerPosition.IsOffAsteroid = false;
        playerPosition.World = World.ShipDownstairs;

        var direction = game.StateManager.Ecs.GetComponent<DirectionComponent>(playerEntityId);
        direction.Direction = Direction.Bottom;

        game.StateManager.Ecs.SetActiveControllableEntity(playerEntityId);

        // Remove components that were added in AsteroidWorld
        game.StateManager.Ecs.RemoveComponent<HealthComponent>(playerEntityId);
        game.StateManager.Ecs.RemoveComponent<MiningComponent>(playerEntityId);
        game.StateManager.Ecs.RemoveComponent<DirectionalLightSourceComponent>(playerEntityId);
    }
}