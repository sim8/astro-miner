using AstroMiner.Definitions;
using AstroMiner.Model;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class NewGameManager(BaseGame game)
{
    public void SetUpNewGame()
    {
        game.Model = GameModelHelpers.CreateNewGameModel();


        game.StateManager.Ecs.Factories.CreateMinerEntity(new Vector2());
        var KrevikStartingPos = new Vector2(1.5f, 1.5f);
        var playerEntityId = game.StateManager.Ecs.Factories.CreatePlayerEntity(KrevikStartingPos);

        game.StateManager.Ecs.SetActiveControllableEntity(playerEntityId);

        SetUpNpcs();
    }

    private void SetUpNpcs()
    {
        game.StateManager.Ecs.Factories.CreateMinExMerchantEntity();
        game.StateManager.Ecs.Factories.CreateRikusEntity();
    }
}