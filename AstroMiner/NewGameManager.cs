using AstroMiner.Definitions;
using AstroMiner.Model;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class NewGameManager(BaseGame game)
{
    public void SetUpNewGame()
    {
        game.Model = GameModelHelpers.CreateNewGameModel();

        game.Model.ActiveWorld = World.Krevik;

        game.Model.Launch.LaunchPadFrontEntityId =
            game.StateManager.Ecs.Factories.CreateLaunchPadFrontEntity(new Vector2());
        game.Model.Launch.LaunchPadRearEntityId =
            game.StateManager.Ecs.Factories.CreateLaunchPadRearEntity(new Vector2());

        game.StateManager.Ecs.Factories.CreateMinerEntity(new Vector2());
        var KrevikStartingPos = new Vector2(3, 3);
        game.StateManager.Ecs.Factories.CreatePlayerEntity(KrevikStartingPos);

        SetUpNpcs();


        game.StateManager.HomeWorld.ResetEntities();
    }

    private void SetUpNpcs()
    {
        game.StateManager.Ecs.Factories.CreateMinExMerchantEntity();
        game.StateManager.Ecs.Factories.CreateRikusEntity();
    }
}