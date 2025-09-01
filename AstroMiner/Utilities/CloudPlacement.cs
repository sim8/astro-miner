using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.Utilities;

/// <summary>
/// Represents a cloud placement with position in grid coordinates
/// </summary>
public record CloudPlacement(float X, float Y);

/// <summary>
/// Stateless cloud placement system that generates consistent cloud positions
/// based on a seed and visible grid rectangle
/// </summary>
public static class CloudGenerator
{

    /// <summary>
    /// Gets cloud placements for a given visible grid rectangle
    /// </summary>
    /// <param name="gridRectX">X position of visible area in grid coordinates</param>
    /// <param name="gridRectY">Y position of visible area in grid coordinates</param>
    /// <param name="gridRectWidth">Width of visible area in grid units</param>
    /// <param name="gridRectHeight">Height of visible area in grid units</param>
    /// <param name="cloudsPerGridCell">Density of clouds (e.g., 0.05 = 5% chance per grid cell)</param>
    /// <param name="seed">Seed for consistent random generation</param>
    /// <param name="textureSizePx">Size of the texture in pixels</param>
    /// <returns>List of cloud placements in grid coordinates</returns>
    public static List<CloudPlacement> GetCloudPlacements(
        float gridRectX,
        float gridRectY,
        float gridRectWidth,
        float gridRectHeight,
        float cloudsPerGridCell = 0.05f,
        int seed = 12345,
        int textureSizePx = 128)
    {
        var clouds = new List<CloudPlacement>();

        float textureSizeGridUnits = textureSizePx / GameConfig.CellTextureSizePx;

        // Expand the area to include padding for partially visible clouds
        // We only pad left+top as CloudPlacements are for their top/left positions
        var paddedStartX = gridRectX - textureSizeGridUnits;
        var paddedStartY = gridRectY - textureSizeGridUnits;
        var endX = gridRectX + gridRectWidth;
        var endY = gridRectY + gridRectHeight;

        // We'll sample at a grid resolution based on cloud size to avoid over-densification
        // Sample every half cloud width to get good coverage without too many overlaps
        var sampleStep = textureSizeGridUnits * 0.5f;

        // Calculate the grid bounds for sampling
        var startSampleX = (float)Math.Floor(paddedStartX / sampleStep) * sampleStep;
        var startSampleY = (float)Math.Floor(paddedStartY / sampleStep) * sampleStep;

        // Iterate through sample points
        for (var sampleX = startSampleX; sampleX < endX; sampleX += sampleStep)
        {
            for (var sampleY = startSampleY; sampleY < endY; sampleY += sampleStep)
            {
                // Create a deterministic hash based on position and seed
                var hash = GetPositionHash((int)(sampleX * 1000), (int)(sampleY * 1000), seed);

                // Convert hash to a 0-1 probability
                var probability = (hash & 0xFFFF) / (float)0xFFFF;

                // Check if we should place a cloud at this sample point
                if (probability < cloudsPerGridCell)
                {
                    // Add some sub-grid randomness to the position using the hash
                    var offsetHash = GetPositionHash((int)(sampleX * 1000) + 1, (int)(sampleY * 1000) + 1, seed);
                    var offsetX = ((offsetHash & 0xFF) / 255f - 0.5f) * sampleStep * 0.8f;
                    var offsetY = (((offsetHash >> 8) & 0xFF) / 255f - 0.5f) * sampleStep * 0.8f;

                    var finalX = sampleX + offsetX;
                    var finalY = sampleY + offsetY;

                    // No clamping - let clouds appear naturally at their calculated positions
                    // This allows for more natural distribution without artificial clustering at boundaries
                    clouds.Add(new CloudPlacement(finalX, finalY));
                }
            }
        }

        return clouds;
    }

    /// <summary>
    /// Creates a deterministic hash from position coordinates and seed
    /// </summary>
    private static int GetPositionHash(int x, int y, int seed)
    {
        // Simple but effective hash function for 2D coordinates
        // Based on common spatial hashing techniques
        var hash = seed;
        hash = hash * 31 + x;
        hash = hash * 31 + y;
        hash = hash * 31 + (x ^ y);

        // Additional mixing to improve distribution
        hash ^= hash >> 16;
        hash *= unchecked((int)0x85ebca6b);
        hash ^= hash >> 13;
        hash *= unchecked((int)0xc2b2ae35);
        hash ^= hash >> 16;

        return hash;
    }
}
