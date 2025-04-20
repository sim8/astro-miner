using System;
using System.IO;
using System.Text.Json;

namespace AstroMiner.Storage;

public class GameStateStorage
{
    private readonly string SaveFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        "game_save.json");

    public void SaveState(GameState gameState)
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(gameState);
            File.WriteAllText(SaveFilePath, jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving game state: {ex.Message}");
        }
    }

    public GameState LoadState(BaseGame game)
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                var jsonString = File.ReadAllText(SaveFilePath);
                var gameState = JsonSerializer.Deserialize<GameState>(jsonString);
                return gameState ?? new GameState(game);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading game state: {ex.Message}");
        }

        // Return a new game state if loading fails or file doesn't exist
        return new GameState(game);
    }
}