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
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(13.5f, 1f));
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(15.95f, 1f));
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(17.95f, 1f));

        // Corridor
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(21f, 10f));
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(15f, 10f));
        game.StateManager.Ecs.Factories.CreateWindowLightSourceEntity(World.ShipDownstairs, new Vector2(9f, 10f));

        // Hangar
        game.StateManager.Ecs.Factories.CreateCeilingLightSourceEntity(World.ShipDownstairs, new Vector2(33f, 6f));
    }
}