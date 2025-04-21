using System.Drawing;
using System.Text.Json;
using AstroMiner.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace AstroMiner.Tests.Storage;

[TestClass]
public class JsonConverterTests
{
    private JsonSerializerOptions _options;

    [TestInitialize]
    public void Setup()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new Vector2Converter(),
                new AstroMiner.Storage.ColorConverter(),
                new RectangleFConverter()
            }
        };
    }

    [TestMethod]
    public void Vector2Converter_CanSerializeAndDeserialize()
    {
        // Arrange
        var originalVector = new Vector2(10.5f, 20.75f);

        // Act
        string json = JsonSerializer.Serialize(originalVector, _options);
        var deserializedVector = JsonSerializer.Deserialize<Vector2>(json, _options);

        // Assert
        Assert.AreEqual(originalVector.X, deserializedVector.X);
        Assert.AreEqual(originalVector.Y, deserializedVector.Y);
    }

    [TestMethod]
    public void ColorConverter_CanSerializeAndDeserialize()
    {
        // Arrange
        var originalColor = new Color(128, 64, 255, 200);

        // Act
        string json = JsonSerializer.Serialize(originalColor, _options);
        var deserializedColor = JsonSerializer.Deserialize<Color>(json, _options);

        // Assert
        Assert.AreEqual(originalColor.R, deserializedColor.R);
        Assert.AreEqual(originalColor.G, deserializedColor.G);
        Assert.AreEqual(originalColor.B, deserializedColor.B);
        Assert.AreEqual(originalColor.A, deserializedColor.A);
    }

    [TestMethod]
    public void RectangleFConverter_CanSerializeAndDeserialize()
    {
        // Arrange
        var originalRect = new RectangleF(5.25f, 10.75f, 100.5f, 200.25f);

        // Act
        string json = JsonSerializer.Serialize(originalRect, _options);
        var deserializedRect = JsonSerializer.Deserialize<RectangleF>(json, _options);

        // Assert
        Assert.AreEqual(originalRect.X, deserializedRect.X);
        Assert.AreEqual(originalRect.Y, deserializedRect.Y);
        Assert.AreEqual(originalRect.Width, deserializedRect.Width);
        Assert.AreEqual(originalRect.Height, deserializedRect.Height);
    }

    [TestMethod]
    public void ComplexObject_WithConverterTypes_CanSerializeAndDeserialize()
    {
        // Arrange
        var testObj = new TestSerializableObject
        {
            Position = new Vector2(15.5f, 30.25f),
            Color = new Color(50, 100, 150, 200),
            Rectangle = new RectangleF(5.5f, 10.5f, 100.25f, 200.75f),
            Name = "Test Object"
        };

        // Act
        string json = JsonSerializer.Serialize(testObj, _options);
        var deserializedObj = JsonSerializer.Deserialize<TestSerializableObject>(json, _options);

        // Assert
        Assert.AreEqual(testObj.Position.X, deserializedObj.Position.X);
        Assert.AreEqual(testObj.Position.Y, deserializedObj.Position.Y);
        Assert.AreEqual(testObj.Color.R, deserializedObj.Color.R);
        Assert.AreEqual(testObj.Color.G, deserializedObj.Color.G);
        Assert.AreEqual(testObj.Color.B, deserializedObj.Color.B);
        Assert.AreEqual(testObj.Color.A, deserializedObj.Color.A);
        Assert.AreEqual(testObj.Rectangle.X, deserializedObj.Rectangle.X);
        Assert.AreEqual(testObj.Rectangle.Y, deserializedObj.Rectangle.Y);
        Assert.AreEqual(testObj.Rectangle.Width, deserializedObj.Rectangle.Width);
        Assert.AreEqual(testObj.Rectangle.Height, deserializedObj.Rectangle.Height);
        Assert.AreEqual(testObj.Name, deserializedObj.Name);
    }

    // A test class that contains all types that need custom converters
    private class TestSerializableObject
    {
        public Vector2 Position { get; set; }
        public Color Color { get; set; }
        public RectangleF Rectangle { get; set; }
        public string Name { get; set; }
    }
}