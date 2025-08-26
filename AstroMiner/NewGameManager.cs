using AstroMiner.Definitions;
using AstroMiner.Model;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class NewGameManager(BaseGame game)
{
    public void SetUpNewGame()
    {
        game.Model = GameModelHelpers.CreateNewGameModel();

        game.StateManager.Ecs.Factories.CreateMinerEntity();
        var KrevikStartingPos = new Vector2(1.5f, 1.5f);
        var playerEntityId = game.StateManager.Ecs.Factories.CreatePlayerEntity(KrevikStartingPos);

        game.StateManager.Ecs.SetActiveControllableEntity(playerEntityId);

        SetUpNpcs();
        SetUpStaticEntitiesTEMP();
    }

    private void SetUpNpcs()
    {
        game.StateManager.Ecs.Factories.CreateMinExMerchantEntity();
        game.StateManager.Ecs.Factories.CreateRikusEntity();
    }

    // TODO feels a weird pattern. Do they need to be in the model?
    private void SetUpStaticEntitiesTEMP()
    {
        // Bedroom
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(16.5f, 1f));
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(18.95f, 1f));
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(20.95f, 1f));
        game.StateManager.Ecs.Factories.CreateSlidingDoorEntity(World.ShipDownstairs, new Vector2(7f, 7.82f));
        game.StateManager.Ecs.Factories.CreateSlidingDoorEntity(World.ShipDownstairs, new Vector2(11f, 7.82f));
        game.StateManager.Ecs.Factories.CreateSlidingDoorEntity(World.ShipDownstairs, new Vector2(20f, 7.82f));

        // Corridor
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(24f, 10f));
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(18f, 10f));
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(12f, 10f));

        // Hangar
        game.StateManager.Ecs.Factories.CreateCeilingLightSourceEntity(World.ShipDownstairs, new Vector2(33.5f, 6f));

        game.StateManager.Ecs.Factories.CreateLaunchConsoleEntity();

        // Krevik
        game.StateManager.Ecs.Factories.CreateShopEntity();
        game.StateManager.Ecs.Factories.CreateShipEntity(new Vector2(Coordinates.Grid.KrevikToShipDownstairsPortal.x, Coordinates.Grid.KrevikToShipDownstairsPortal.y));
        
    }
}