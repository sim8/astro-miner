using AstroMiner.Model;

namespace AstroMiner;

public class NewGameManager(BaseGame game)
{
    public void SetUpNewGame()
    {
        game.Model = GameModelHelpers.CreateNewGameModel();

        game.StateManager.HomeWorld.InitializeOrResetEntities();
    }
}