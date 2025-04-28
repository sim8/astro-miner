using System;
using System.Text.Json;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Model;
using AstroMiner.Storage;
using AstroMiner.Tests.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace AstroMiner.Tests.Storage;

[TestClass]
public class GameModelSerializationTests
{
    private GameStateStorage _gameStateStorage;
    private MockBaseGame _mockBaseGame;

    [TestInitialize]
    public void Setup()
    {
        _mockBaseGame = new MockBaseGame(null);
        _gameStateStorage = new GameStateStorage(_mockBaseGame);
    }

    [TestMethod]
    public void GameModel_WithBasicComponents_SerializesAndDeserializes()
    {
        // Arrange - Create a game model with some component data
        var model = GameModelHelpers.CreateNewGameModel();

        // Add a player entity with some components
        var entityId = 1;
        model.Ecs.NextEntityId = 2; // Set next entity ID

        // Position component with Vector2
        var positionComponent = new PositionComponent
        {
            Position = new Vector2(10.5f, 20.25f),
            WidthPx = 32,
            HeightPx = 32,
            IsCollideable = true,
            World = World.Home
        };
        model.Ecs.ComponentsByEntityId.Position[entityId] = positionComponent;

        // Direction component
        var directionComponent = new DirectionComponent
        {
            Direction = Direction.Right
        };
        model.Ecs.ComponentsByEntityId.Direction[entityId] = directionComponent;

        // Texture component
        var textureComponent = new TextureComponent
        {
            TextureName = "player"
        };
        model.Ecs.ComponentsByEntityId.Texture[entityId] = textureComponent;

        // Light source component with Color
        var lightComponent = new RadialLightSourceComponent
        {
            SizePx = 256,
            Opacity = 0.8f,
            Tint = new Color(255, 200, 100)
        };
        model.Ecs.ComponentsByEntityId.RadialLightSource[entityId] = lightComponent;

        // Set player entity
        model.Ecs.PlayerEntityId = entityId;
        model.Ecs.ActiveControllableEntityId = entityId;

        // Act - Serialize and then deserialize
        var jsonString = JsonSerializer.Serialize(model, _gameStateStorage.GetSerializerOptions());
        Console.WriteLine(
            $"Serialized JSON (excerpt): {jsonString.Substring(0, Math.Min(1000, jsonString.Length))}...");

        var deserializedModel =
            JsonSerializer.Deserialize<GameModel>(jsonString, _gameStateStorage.GetSerializerOptions());

        // Print deserialized values for debugging
        var deserializedPos = deserializedModel.Ecs.ComponentsByEntityId.Position[entityId].Position;
        Console.WriteLine($"Original Position: {positionComponent.Position}");
        Console.WriteLine($"Deserialized Position: {deserializedPos}");

        // Assert - Verify the model properties are preserved
        Assert.IsNotNull(deserializedModel);
        Assert.AreEqual(model.ActiveWorld, deserializedModel.ActiveWorld);
        Assert.AreEqual(model.SavedTotalPlaytimeMs, deserializedModel.SavedTotalPlaytimeMs);

        // Verify entity references
        Assert.AreEqual(model.Ecs.PlayerEntityId, deserializedModel.Ecs.PlayerEntityId);
        Assert.AreEqual(model.Ecs.ActiveControllableEntityId, deserializedModel.Ecs.ActiveControllableEntityId);

        // Verify component data
        var deserializedPosition = deserializedModel.Ecs.ComponentsByEntityId.Position[entityId];
        Assert.AreEqual(positionComponent.Position.X, deserializedPosition.Position.X);
        Assert.AreEqual(positionComponent.Position.Y, deserializedPosition.Position.Y);
        Assert.AreEqual(positionComponent.WidthPx, deserializedPosition.WidthPx);
        Assert.AreEqual(positionComponent.World, deserializedPosition.World);

        var deserializedDirection = deserializedModel.Ecs.ComponentsByEntityId.Direction[entityId];
        Assert.AreEqual(directionComponent.Direction, deserializedDirection.Direction);

        var deserializedTexture = deserializedModel.Ecs.ComponentsByEntityId.Texture[entityId];
        Assert.AreEqual(textureComponent.TextureName, deserializedTexture.TextureName);

        var deserializedLight = deserializedModel.Ecs.ComponentsByEntityId.RadialLightSource[entityId];
        Assert.AreEqual(lightComponent.SizePx, deserializedLight.SizePx);
        Assert.AreEqual(lightComponent.Opacity, deserializedLight.Opacity);
        Assert.AreEqual(lightComponent.Tint.R, deserializedLight.Tint.R);
        Assert.AreEqual(lightComponent.Tint.G, deserializedLight.Tint.G);
        Assert.AreEqual(lightComponent.Tint.B, deserializedLight.Tint.B);
    }
}