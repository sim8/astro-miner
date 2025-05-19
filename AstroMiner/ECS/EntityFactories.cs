using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS;

public class EntityFactories
{
    private readonly Ecs _ecs;
    private readonly BaseGame _game;

    public EntityFactories(Ecs ecs, BaseGame game)
    {
        _game = game;
        _ecs = ecs;
    }

    public int CreatePlayerEntity(Vector2 centerPosition)
    {
        var entityId = _ecs.CreateEntity();

        // Add position component
        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.SetCenterPosition(centerPosition);
        positionComponent.World = _game.Model.ActiveWorld;
        positionComponent.WidthPx = GameConfig.PlayerBoxSizePx;
        positionComponent.HeightPx = GameConfig.PlayerBoxSizePx;
        positionComponent.IsCollideable = true;

        // Add direction component
        _ecs.AddComponent<DirectionComponent>(entityId);

        // Add movement component
        var movementComponent = _ecs.AddComponent<MovementComponent>(entityId);
        movementComponent.MaxSpeed = GameConfig.Speeds.Running;
        movementComponent.TimeToReachMaxSpeedMs = 0; // From ControllableEntity default
        movementComponent.TimeToStopMs = 0; // From ControllableEntity default

        // Add tag component for identification
        _ecs.AddComponent<PlayerTag>(entityId);

        // Store the player entity ID
        _ecs.SetPlayerEntityId(entityId);

        return entityId;
    }

    public int CreateMinerEntity()
    {
        var entityId = _ecs.CreateEntity();

        // Add position component
        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = World.ShipDownstairs;
        positionComponent.WidthPx = GameConfig.MinerBoxSizePx;
        positionComponent.HeightPx = GameConfig.MinerBoxSizePx;
        positionComponent.SetCenterPosition(new Vector2(34f, 6f));
        positionComponent.IsCollideable = true;

        // Add direction component
        var directionComponent = _ecs.AddComponent<DirectionComponent>(entityId);
        directionComponent.Direction = Direction.Right;

        // Add tag component for identification
        _ecs.AddComponent<MinerTag>(entityId);

        // Store the miner entity ID
        _ecs.SetMinerEntityId(entityId);

        // Add interactive component
        var interactiveComponent = _ecs.AddComponent<InteractiveComponent>(entityId);
        interactiveComponent.InteractableDistance = 1.5f;

        return entityId;
    }

    public int CreateMinExMerchantEntity()
    {
        var entityId = _ecs.CreateEntity();

        // Add position component
        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = World.MinEx;
        positionComponent.SetCenterPosition(new Vector2(4.5f, 3.5f));
        positionComponent.WidthPx = GameConfig.PlayerBoxSizePx;
        positionComponent.HeightPx = GameConfig.PlayerBoxSizePx;
        positionComponent.IsCollideable = true;

        // Add direction component
        var directionComponent = _ecs.AddComponent<DirectionComponent>(entityId);
        directionComponent.Direction = Direction.Bottom;

        // Add tag component for identification
        _ecs.AddComponent<NpcComponent>(entityId);
        _ecs.GetComponent<NpcComponent>(entityId).Npc = Npc.MinExMerchant;

        // Add interactive component
        _ecs.AddComponent<InteractiveComponent>(entityId);

        return entityId;
    }

    public int CreateRikusEntity()
    {
        var entityId = _ecs.CreateEntity();

        // Add position component
        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = World.Krevik;
        positionComponent.SetCenterPosition(new Vector2(4.5f, 3.5f));
        positionComponent.WidthPx = GameConfig.PlayerBoxSizePx;
        positionComponent.HeightPx = GameConfig.PlayerBoxSizePx;
        positionComponent.IsCollideable = true;

        // Add direction component
        var directionComponent = _ecs.AddComponent<DirectionComponent>(entityId);
        directionComponent.Direction = Direction.Bottom;

        // Add tag component for identification
        _ecs.AddComponent<NpcComponent>(entityId);
        _ecs.GetComponent<NpcComponent>(entityId).Npc = Npc.Rikus;

        // Add interactive component
        _ecs.AddComponent<InteractiveComponent>(entityId);

        return entityId;
    }

    public int CreateDynamiteEntity(Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = _game.Model.ActiveWorld;
        positionComponent.Position = position;
        positionComponent.WidthPx = 4; // From DynamiteSystem
        positionComponent.HeightPx = 4; // From DynamiteSystem
        positionComponent.IsCollideable = false;

        var fuseComponent = _ecs.AddComponent<FuseComponent>(entityId);
        fuseComponent.MaxFuseTimeMs = 4000; // From DynamiteSystem
        fuseComponent.TimeToExplodeMs = 4000;

        // Add tag component for identification
        _ecs.AddComponent<DynamiteTag>(entityId);

        return entityId;
    }

    public int CreateExplosionEntity(Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = _game.Model.ActiveWorld;
        positionComponent.Position = position;
        positionComponent.WidthPx = 1; // From ExplosionSystem
        positionComponent.HeightPx = 1; // From ExplosionSystem
        positionComponent.IsCollideable = false;

        _ecs.AddComponent<ExplosionComponent>(entityId);
        _ecs.AddComponent<ExplosionTag>(entityId);

        return entityId;
    }

    public int CreateWindowLightSourceEntity(World world, Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = world;
        positionComponent.Position = position;
        positionComponent.WidthPx = 1;
        positionComponent.HeightPx = 1;
        positionComponent.IsCollideable = false;

        var radialLightSource = _ecs.AddComponent<RadialLightSourceComponent>(entityId);
        radialLightSource.Tint = Color.White;
        radialLightSource.SizePx = 350;
        radialLightSource.Opacity = 0.7f;

        return entityId;
    }

    public int CreateCeilingLightSourceEntity(World world, Vector2 position)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = world;
        positionComponent.Position = position;
        positionComponent.WidthPx = 1;
        positionComponent.HeightPx = 1;
        positionComponent.IsCollideable = false;

        var radialLightSource = _ecs.AddComponent<RadialLightSourceComponent>(entityId);
        radialLightSource.Tint = new Color(255, 251, 220);
        radialLightSource.SizePx = 256;
        radialLightSource.Opacity = 1f;

        return entityId;
    }
}