using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AstroMiner.Model;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace AstroMiner.Storage;

public class GameStateStorage
{
    private readonly BaseGame _game;

    private readonly JsonSerializerOptions _serializerOptions;

    private readonly string SaveFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        "game_save_2.json");

    public GameStateStorage(BaseGame game)
    {
        _game = game;
        _serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new Vector2Converter(),
                new ColorConverter(),
                new RectangleFConverter()
            }
        };
    }

    // Method to expose serializer options for testing
    public JsonSerializerOptions GetSerializerOptions()
    {
        return _serializerOptions;
    }

    public void SaveState(GameModel model)
    {
        try
        {
            model.SavedTotalPlaytimeMs = _game.StateManager.GetTotalPlayTime();
            var jsonString = JsonSerializer.Serialize(model, _serializerOptions);
            File.WriteAllText(SaveFilePath, jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving game state: {ex.Message}");
        }
    }

    public GameModel LoadState()
    {
        if (File.Exists(SaveFilePath))
        {
            var jsonString = File.ReadAllText(SaveFilePath);
            var gameState = JsonSerializer.Deserialize<GameModel>(jsonString, _serializerOptions);
            if (gameState == null)
            {
                throw new Exception("Failed to deserialize game state");
            }
            return gameState;
        }

        throw new Exception("Save file not found");
    }
}

// Custom JSON converters for XNA and System.Drawing types
public class Vector2Converter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of object");

        float x = 0, y = 0;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "X":
                        x = reader.GetSingle();
                        break;
                    case "Y":
                        y = reader.GetSingle();
                        break;
                }
            }

        return new Vector2(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}

public class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of object");

        byte r = 0, g = 0, b = 0, a = 255;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "R":
                        r = reader.GetByte();
                        break;
                    case "G":
                        g = reader.GetByte();
                        break;
                    case "B":
                        b = reader.GetByte();
                        break;
                    case "A":
                        a = reader.GetByte();
                        break;
                }
            }

        return new Color(r, g, b, a);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("R", value.R);
        writer.WriteNumber("G", value.G);
        writer.WriteNumber("B", value.B);
        writer.WriteNumber("A", value.A);
        writer.WriteEndObject();
    }
}

public class RectangleFConverter : JsonConverter<RectangleF>
{
    public override RectangleF Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of object");

        float x = 0, y = 0, width = 0, height = 0;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "X":
                        x = reader.GetSingle();
                        break;
                    case "Y":
                        y = reader.GetSingle();
                        break;
                    case "Width":
                        width = reader.GetSingle();
                        break;
                    case "Height":
                        height = reader.GetSingle();
                        break;
                }
            }

        return new RectangleF(x, y, width, height);
    }

    public override void Write(Utf8JsonWriter writer, RectangleF value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Width", value.Width);
        writer.WriteNumber("Height", value.Height);
        writer.WriteEndObject();
    }
}