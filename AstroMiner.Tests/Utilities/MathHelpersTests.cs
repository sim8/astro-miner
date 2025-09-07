using AstroMiner.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroMiner.Tests.Utilities;

[TestClass]
public class MathHelpersTests
{
    private const float Tolerance = 0.0001f;

    [TestMethod]
    public void GetPercentageBetween_NormalRange_ReturnsCorrectPercentage()
    {
        // Arrange
        var start = 0f;
        var end = 10f;

        // Act & Assert
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(0f, start, end), Tolerance);
        Assert.AreEqual(0.5f, MathHelpers.GetPercentageBetween(5f, start, end), Tolerance);
        Assert.AreEqual(1f, MathHelpers.GetPercentageBetween(10f, start, end), Tolerance);
    }

    [TestMethod]
    public void GetPercentageBetween_ReverseRange_ReturnsCorrectPercentage()
    {
        // Arrange
        var start = 10f;
        var end = 0f;

        // Act & Assert
        // When value is closer to end (0), percentage should be near 1
        Assert.AreEqual(1f, MathHelpers.GetPercentageBetween(0f, start, end), Tolerance);
        Assert.AreEqual(0.5f, MathHelpers.GetPercentageBetween(5f, start, end), Tolerance);
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(10f, start, end), Tolerance);
    }

    [TestMethod]
    public void GetPercentageBetween_ValueBelowRange_ClampsToZero()
    {
        // Arrange
        var start = 5f;
        var end = 15f;

        // Act & Assert
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(-10f, start, end), Tolerance);
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(0f, start, end), Tolerance);
    }

    [TestMethod]
    public void GetPercentageBetween_ValueAboveRange_ClampsToOne()
    {
        // Arrange
        var start = 5f;
        var end = 15f;

        // Act & Assert
        Assert.AreEqual(1f, MathHelpers.GetPercentageBetween(20f, start, end), Tolerance);
        Assert.AreEqual(1f, MathHelpers.GetPercentageBetween(100f, start, end), Tolerance);
    }

    [TestMethod]
    public void GetPercentageBetween_ReverseRangeValueBelowRange_ClampsToZero()
    {
        // Arrange
        var start = 15f;
        var end = 5f;

        // Act & Assert
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(20f, start, end), Tolerance);
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(100f, start, end), Tolerance);
    }

    [TestMethod]
    public void GetPercentageBetween_ReverseRangeValueAboveRange_ClampsToOne()
    {
        // Arrange
        var start = 15f;
        var end = 5f;

        // Act & Assert
        Assert.AreEqual(1f, MathHelpers.GetPercentageBetween(0f, start, end), Tolerance);
        Assert.AreEqual(1f, MathHelpers.GetPercentageBetween(-10f, start, end), Tolerance);
    }

    [TestMethod]
    public void GetPercentageBetween_EqualStartAndEnd_ReturnsZero()
    {
        // Arrange
        var start = 5f;
        var end = 5f;

        // Act & Assert
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(0f, start, end), Tolerance);
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(5f, start, end), Tolerance);
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(10f, start, end), Tolerance);
    }

    [TestMethod]
    public void GetPercentageBetween_NegativeRange_ReturnsCorrectPercentage()
    {
        // Arrange
        var start = -10f;
        var end = -5f;

        // Act & Assert
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(-10f, start, end), Tolerance);
        Assert.AreEqual(0.5f, MathHelpers.GetPercentageBetween(-7.5f, start, end), Tolerance);
        Assert.AreEqual(1f, MathHelpers.GetPercentageBetween(-5f, start, end), Tolerance);
    }

    [TestMethod]
    public void GetPercentageBetween_CrossingZero_ReturnsCorrectPercentage()
    {
        // Arrange
        var start = -5f;
        var end = 5f;

        // Act & Assert
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(-5f, start, end), Tolerance);
        Assert.AreEqual(0.5f, MathHelpers.GetPercentageBetween(0f, start, end), Tolerance);
        Assert.AreEqual(1f, MathHelpers.GetPercentageBetween(5f, start, end), Tolerance);
    }

    [TestMethod]
    public void GetPercentageBetween_VerySmallRange_HandlesFloatPrecision()
    {
        // Arrange
        var start = 1.0001f;
        var end = 1.0002f;

        // Act & Assert
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(1.0001f, start, end), Tolerance);
        // Use a larger tolerance for very small ranges due to floating point precision
        Assert.AreEqual(0.5f, MathHelpers.GetPercentageBetween(1.00015f, start, end), 0.01f);
        Assert.AreEqual(1f, MathHelpers.GetPercentageBetween(1.0002f, start, end), Tolerance);
    }

    [TestMethod]
    public void GetPercentageBetween_ExtremeValues_HandlesCorrectly()
    {
        // Arrange
        var start = float.MinValue / 2f;
        var end = float.MaxValue / 2f;

        // Act & Assert
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(start, start, end), Tolerance);
        Assert.AreEqual(1f, MathHelpers.GetPercentageBetween(end, start, end), Tolerance);
        Assert.AreEqual(0f, MathHelpers.GetPercentageBetween(start - 1000f, start, end), Tolerance);
        Assert.AreEqual(1f, MathHelpers.GetPercentageBetween(end + 1000f, start, end), Tolerance);
    }
}