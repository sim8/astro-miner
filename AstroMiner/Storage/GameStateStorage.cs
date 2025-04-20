using System;
using System.IO;
using System.Text.Json;
using AstroMiner.Model;

namespace AstroMiner.Storage;

public class GameStateStorage
{
    private readonly string SaveFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        "game_save_2.json");

    public void SaveState(GameModel model)
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(model);
            File.WriteAllText(SaveFilePath, jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving game state: {ex.Message}");
        }
    }

    public GameModel LoadState()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                var jsonString = File.ReadAllText(SaveFilePath);
                var gameState = JsonSerializer.Deserialize<GameModel>(jsonString);
                return gameState ?? GameModelHelpers.CreateNewGameModel();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading game state: {ex.Message}");
        }

        // Return a new game state if loading fails or file doesn't exist
        return GameModelHelpers.CreateNewGameModel();
    }
}