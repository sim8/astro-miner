using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS;

public class EntityFactories
{
    private readonly Ecs _ecs;
    private readonly BaseGame _game;

    public readonly float SlidingDoorOffset = .75f;

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
        positionComponent.SetCenterPosition(new Vector2(33.5f, 6f));
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

    public int CreateLaunchConsoleEntity()
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = World.ShipUpstairs;
        positionComponent.Position = new Vector2(25, 7);
        positionComponent.WidthPx = 64;
        positionComponent.HeightPx = 64;
        positionComponent.IsCollideable = true;

        var radialLightSource = _ecs.AddComponent<RadialLightSourceComponent>(entityId);
        radialLightSource.Tint = new Color(192, 210, 216);
        radialLightSource.SizePx = 128;
        radialLightSource.Opacity = 1f;

        var interactiveComponent = _ecs.AddComponent<InteractiveComponent>(entityId);
        interactiveComponent.InteractiveType = InteractiveType.LaunchConsole;
        interactiveComponent.InteractableDistance = 1.5f;

        var textureComponent = _ecs.AddComponent<TextureComponent>(entityId);
        textureComponent.TextureName = Tx.LaunchConsole;
        textureComponent.TopPaddingPx = 10;

        return entityId;
    }

    public int CreateShopEntity()
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = World.Krevik;
        positionComponent.Position = new Vector2(10.5f, 4);
        positionComponent.WidthPx = 64;
        positionComponent.HeightPx = 16;
        positionComponent.IsCollideable = true;

        var interactiveComponent = _ecs.AddComponent<InteractiveComponent>(entityId);
        interactiveComponent.InteractiveType = InteractiveType.Shop;
        interactiveComponent.InteractableDistance = 1.5f;

        return entityId;
    }

    public int CreateMerchantEntity(World world, Vector2 position, int widthPx, int heightPx, MerchantType merchantType)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = world;
        positionComponent.Position = position;
        positionComponent.WidthPx = widthPx;
        positionComponent.HeightPx = heightPx;
        positionComponent.IsCollideable = true;

        var interactiveComponent = _ecs.AddComponent<InteractiveComponent>(entityId);
        interactiveComponent.InteractiveType = InteractiveType.Merchant;
        interactiveComponent.InteractableDistance = 1.5f;
        interactiveComponent.MerchantType = merchantType;

        return entityId;
    }

    public int CreateSlidingDoorEntity(World world, Vector2 position, bool isElevator = false)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = world;
        positionComponent.Position = position;
        positionComponent.WidthPx = 32;
        positionComponent.HeightPx = 4;
        positionComponent.IsCollideable = !isElevator;

        var slidingDoorComponent = _ecs.AddComponent<SlidingDoorComponent>(entityId);
        slidingDoorComponent.OpenPercent = 0f;
        slidingDoorComponent.OpenSpeed = 1f;
        slidingDoorComponent.IsElevator = isElevator;

        var textureComponent = _ecs.AddComponent<TextureComponent>(entityId);
        textureComponent.TextureName = Tx.Door;
        textureComponent.TopPaddingPx = 60;

        textureComponent.TextureOffsetYPx = isElevator ? 64 : 0;

        textureComponent.totalFrames = 24;

        return entityId;
    }

    public int CreateShipEntity(Vector2 entrancePosition)
    {
        var entityId = _ecs.CreateEntity();

        var positionComponent = _ecs.AddComponent<PositionComponent>(entityId);
        positionComponent.World = World.Krevik;
        positionComponent.Position = new Vector2(entrancePosition.X - 25, entrancePosition.Y + 4);
        positionComponent.WidthPx = 1024;
        positionComponent.HeightPx = 256;
        positionComponent.IsCollideable = true;

        var textureComponent = _ecs.AddComponent<TextureComponent>(entityId);
        textureComponent.TextureName = Tx.Ship;
        textureComponent.TopPaddingPx = 160;
        textureComponent.LeftPaddingPx = 64;
        textureComponent.BottomPaddingPx = 32;

        textureComponent.TextureOffsetXPx = 64;
        textureComponent.TextureOffsetYPx = 800;

        return entityId;
    }
}