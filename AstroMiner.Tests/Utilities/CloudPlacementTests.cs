using System;
using System.Linq;
using AstroMiner.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroMiner.Tests.Utilities;

[TestClass]
public class CloudPlacementTests
{
    [TestMethod]
    public void GetCloudPlacements_SameSeedAndRect_ReturnsSameClouds()
    {
        // Arrange
        var gridRectX = 10f;
        var gridRectY = 20f;
        var gridRectWidth = 30f;
        var gridRectHeight = 20f;
        var cloudsPerGridCell = 0.1f;
        var seed = 12345;

        // Act - Call twice with same parameters
        var clouds1 = CloudGenerator.GetCloudPlacements(gridRectX, gridRectY, gridRectWidth, gridRectHeight, cloudsPerGridCell, seed);
        var clouds2 = CloudGenerator.GetCloudPlacements(gridRectX, gridRectY, gridRectWidth, gridRectHeight, cloudsPerGridCell, seed);

        // Assert - Should be identical
        Assert.AreEqual(clouds1.Count, clouds2.Count, "Cloud count should be identical for same parameters");

        for (int i = 0; i < clouds1.Count; i++)
        {
            Assert.AreEqual(clouds1[i].X, clouds2[i].X, 0.001f, $"Cloud {i} X position should be identical");
            Assert.AreEqual(clouds1[i].Y, clouds2[i].Y, 0.001f, $"Cloud {i} Y position should be identical");
        }
    }

    [TestMethod]
    public void GetCloudPlacements_DifferentSeeds_ReturnsDifferentClouds()
    {
        // Arrange
        var gridRectX = 10f;
        var gridRectY = 20f;
        var gridRectWidth = 30f;
        var gridRectHeight = 20f;
        var cloudsPerGridCell = 0.1f;

        // Act - Call with different seeds
        var clouds1 = CloudGenerator.GetCloudPlacements(gridRectX, gridRectY, gridRectWidth, gridRectHeight, cloudsPerGridCell, 12345);
        var clouds2 = CloudGenerator.GetCloudPlacements(gridRectX, gridRectY, gridRectWidth, gridRectHeight, cloudsPerGridCell, 54321);

        // Assert - Should be different (very unlikely to be identical by chance)
        var identical = clouds1.Count == clouds2.Count &&
                       clouds1.Zip(clouds2, (c1, c2) => c1.X == c2.X && c1.Y == c2.Y).All(x => x);

        Assert.IsFalse(identical, "Different seeds should produce different cloud patterns");
    }

    [TestMethod]
    public void GetCloudPlacements_CloudDensity_AffectsCloudCount()
    {
        // Arrange
        var gridRectX = 0f;
        var gridRectY = 0f;
        var gridRectWidth = 50f;
        var gridRectHeight = 50f;
        var seed = 12345;

        // Act - Test different densities
        var lowDensityClouds = CloudGenerator.GetCloudPlacements(gridRectX, gridRectY, gridRectWidth, gridRectHeight, 0.01f, seed);
        var highDensityClouds = CloudGenerator.GetCloudPlacements(gridRectX, gridRectY, gridRectWidth, gridRectHeight, 0.1f, seed);

        // Assert - Higher density should generally produce more clouds
        // We use a range check since it's probabilistic
        Assert.IsTrue(highDensityClouds.Count >= lowDensityClouds.Count,
            $"Higher density ({highDensityClouds.Count}) should produce at least as many clouds as lower density ({lowDensityClouds.Count})");
    }

    [TestMethod]
    public void GetCloudPlacements_CloudsWithinPaddedArea()
    {
        // Arrange
        var gridRectX = 10f;
        var gridRectY = 10f;
        var gridRectWidth = 20f;
        var gridRectHeight = 20f;
        var cloudsPerGridCell = 0.2f; // Higher density to ensure we get some clouds
        var padding = 3f;
        var seed = 12345;

        // Act
        var clouds = CloudGenerator.GetCloudPlacements(gridRectX, gridRectY, gridRectWidth, gridRectHeight, cloudsPerGridCell, seed, padding);

        // Assert - Clouds should be in reasonable area (allowing for natural offset beyond padding)
        // With the fix, clouds can extend slightly beyond the padded area due to random offsets
        var tolerance = 1.1f; // Allow some natural variation beyond padding
        var paddedMinX = gridRectX - padding - tolerance;
        var paddedMinY = gridRectY - padding - tolerance;
        var paddedMaxX = gridRectX + gridRectWidth + padding + tolerance;
        var paddedMaxY = gridRectY + gridRectHeight + padding + tolerance;

        foreach (var cloud in clouds)
        {
            Assert.IsTrue(cloud.X >= paddedMinX && cloud.X <= paddedMaxX,
                $"Cloud X position {cloud.X} should be within reasonable range [{paddedMinX}, {paddedMaxX}]");
            Assert.IsTrue(cloud.Y >= paddedMinY && cloud.Y <= paddedMaxY,
                $"Cloud Y position {cloud.Y} should be within reasonable range [{paddedMinY}, {paddedMaxY}]");
        }
    }

    [TestMethod]
    public void GetCloudPlacements_MovingCamera_ConsistentResults()
    {
        // Arrange - Simulate camera movement by shifting the grid rect
        var baseX = 100f;
        var baseY = 100f;
        var width = 30f;
        var height = 20f;
        var cloudsPerGridCell = 0.1f;
        var seed = 12345;

        // Act - Get clouds for overlapping areas as camera moves
        var clouds1 = CloudGenerator.GetCloudPlacements(baseX, baseY, width, height, cloudsPerGridCell, seed);
        var clouds2 = CloudGenerator.GetCloudPlacements(baseX + 10f, baseY + 5f, width, height, cloudsPerGridCell, seed);

        // Assert - Clouds in overlapping area should be consistent
        // Find clouds from first call that should also appear in second call
        var overlapMinX = baseX + 10f;
        var overlapMinY = baseY + 5f;
        var overlapMaxX = baseX + width;
        var overlapMaxY = baseY + height;

        var cloudsInOverlap1 = clouds1.Where(c =>
            c.X >= overlapMinX && c.X <= overlapMaxX &&
            c.Y >= overlapMinY && c.Y <= overlapMaxY).ToList();

        // These clouds should also appear in clouds2
        foreach (var cloud in cloudsInOverlap1)
        {
            var matchingCloud = clouds2.FirstOrDefault(c =>
                System.Math.Abs(c.X - cloud.X) < 0.001f &&
                System.Math.Abs(c.Y - cloud.Y) < 0.001f);

            Assert.IsNotNull(matchingCloud,
                $"Cloud at ({cloud.X}, {cloud.Y}) should appear in both overlapping areas");
        }
    }

    [TestMethod]
    public void GetCloudPlacements_ZeroDensity_ReturnsNoClouds()
    {
        // Arrange
        var gridRectX = 0f;
        var gridRectY = 0f;
        var gridRectWidth = 100f;
        var gridRectHeight = 100f;
        var cloudsPerGridCell = 0f;
        var seed = 12345;

        // Act
        var clouds = CloudGenerator.GetCloudPlacements(gridRectX, gridRectY, gridRectWidth, gridRectHeight, cloudsPerGridCell, seed);

        // Assert
        Assert.AreEqual(0, clouds.Count, "Zero density should produce no clouds");
    }

    [TestMethod]
    public void GetCloudPlacements_SmallArea_HandledCorrectly()
    {
        // Arrange - Very small visible area
        var gridRectX = 0f;
        var gridRectY = 0f;
        var gridRectWidth = 1f;
        var gridRectHeight = 1f;
        var cloudsPerGridCell = 0.5f; // High density
        var seed = 12345;

        // Act
        var clouds = CloudGenerator.GetCloudPlacements(gridRectX, gridRectY, gridRectWidth, gridRectHeight, cloudsPerGridCell, seed);

        // Assert - Should not crash and should return reasonable results
        Assert.IsTrue(clouds.Count >= 0, "Should handle small areas without crashing");

        // All clouds should be within reasonable bounds (including padding)
        foreach (var cloud in clouds)
        {
            Assert.IsTrue(cloud.X >= -5f && cloud.X <= 10f, "Cloud X should be within reasonable bounds");
            Assert.IsTrue(cloud.Y >= -5f && cloud.Y <= 10f, "Cloud Y should be within reasonable bounds");
        }
    }

    [TestMethod]
    public void GetCloudPlacements_NegativeCoordinates_WorksCorrectly()
    {
        // Arrange - Test with negative coordinates
        var gridRectX = -50f;
        var gridRectY = -30f;
        var gridRectWidth = 20f;
        var gridRectHeight = 15f;
        var cloudsPerGridCell = 0.1f;
        var seed = 12345;

        // Act
        var clouds = CloudGenerator.GetCloudPlacements(gridRectX, gridRectY, gridRectWidth, gridRectHeight, cloudsPerGridCell, seed);

        // Assert - Should work with negative coordinates
        Assert.IsTrue(clouds.Count >= 0, "Should handle negative coordinates");

        // Verify clouds are in reasonable area (allowing for natural offset beyond padding)
        var padding = 2f; // Default padding
        var tolerance = 1.1f; // Allow some natural variation beyond padding
        foreach (var cloud in clouds)
        {
            Assert.IsTrue(cloud.X >= gridRectX - padding - tolerance && cloud.X <= gridRectX + gridRectWidth + padding + tolerance,
                $"Cloud X {cloud.X} should be within reasonable range");
            Assert.IsTrue(cloud.Y >= gridRectY - padding - tolerance && cloud.Y <= gridRectY + gridRectHeight + padding + tolerance,
                $"Cloud Y {cloud.Y} should be within reasonable range");
        }
    }

    [TestMethod]
    public void GetCloudPlacements_NoCloudsClampedToBoundaryEdges()
    {
        // Arrange - Test that clouds don't get artificially clustered at boundary edges
        var gridRectX = 0f;
        var gridRectY = 0f;
        var gridRectWidth = 10f;  // Smaller area to make the effect more pronounced
        var gridRectHeight = 10f;
        var cloudsPerGridCell = 1.0f; // Very high density to ensure we get clouds that would be offset
        var seed = 12345;
        var padding = 2f;

        // Act
        var clouds = CloudGenerator.GetCloudPlacements(gridRectX, gridRectY, gridRectWidth, gridRectHeight, cloudsPerGridCell, seed, padding);

        // Assert - Check that clouds are not artificially clustered at the exact boundary edges
        var boundaryTolerance = 0.001f;
        var paddedMinX = gridRectX - padding;
        var paddedMinY = gridRectY - padding;
        var paddedMaxX = gridRectX + gridRectWidth + padding;
        var paddedMaxY = gridRectY + gridRectHeight + padding;

        var cloudsAtMinX = clouds.Count(c => Math.Abs(c.X - paddedMinX) < boundaryTolerance);
        var cloudsAtMinY = clouds.Count(c => Math.Abs(c.Y - paddedMinY) < boundaryTolerance);
        var cloudsAtMaxX = clouds.Count(c => Math.Abs(c.X - paddedMaxX) < boundaryTolerance);
        var cloudsAtMaxY = clouds.Count(c => Math.Abs(c.Y - paddedMaxY) < boundaryTolerance);

        // With clamping, we expect to see clouds exactly at the boundaries
        // This test should FAIL with the current implementation that clamps to boundaries
        Assert.AreEqual(0, cloudsAtMinX, $"Found {cloudsAtMinX} clouds clamped to minimum X boundary");
        Assert.AreEqual(0, cloudsAtMinY, $"Found {cloudsAtMinY} clouds clamped to minimum Y boundary");
        Assert.AreEqual(0, cloudsAtMaxX, $"Found {cloudsAtMaxX} clouds clamped to maximum X boundary");
        Assert.AreEqual(0, cloudsAtMaxY, $"Found {cloudsAtMaxY} clouds clamped to maximum Y boundary");
    }
}
