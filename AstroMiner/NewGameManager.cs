using AstroMiner.Model;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class NewGameManager(BaseGame game)
{
    public void SetUpNewGame()
    {
        game.Model = GameModelHelpers.CreateNewGameModel();

        game.Model.Launch.LaunchPadFrontEntityId =
            game.StateManager.Ecs.Factories.CreateLaunchPadFrontEntity(new Vector2());
        game.Model.Launch.LaunchPadRearEntityId =
            game.StateManager.Ecs.Factories.CreateLaunchPadRearEntity(new Vector2());

        game.StateManager.Ecs.Factories.CreateMinerEntity(new Vector2());
        game.StateManager.Ecs.Factories.CreatePlayerEntity(new Vector2());

        SetUpNpcs();

        game.StateManager.HomeWorld.ResetEntities();
    }

    private void SetUpNpcs()
    {
        game.StateManager.Ecs.Factories.CreateMinExMerchantEntity();
    }
}