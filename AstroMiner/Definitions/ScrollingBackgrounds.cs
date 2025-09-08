using System.Collections.Generic;

namespace AstroMiner.Definitions;

public record ScrollingBackgroundLayer
{
    // Land configuration
    public int LandTextureWidthPx { get; init; } = 1600;
    public int LandTextureHeightPx { get; init; } = 1600;
    public float LandTextureScale { get; init; } = 0.5f;
    public float LandSpeed { get; init; } = 0.5f;
    public float LandParallaxFactorX { get; init; } = 0.4f;
    public float LandParallaxFactorY { get; init; } = 0.2f;
    public string LandTextureName { get; init; } = Tx.Backgrounds.Oizus;
}

public record ScrollingBackgroundConfig
{
    public List<ScrollingBackgroundLayer> Layers { get; init; } = new();
}

public static class ScrollingBackgrounds
{
    private static readonly ScrollingBackgroundConfig OizusAsteroid = new()
    {
        Layers = new List<ScrollingBackgroundLayer>
        {
            new()
            {
                LandTextureWidthPx = 640,
                LandTextureHeightPx = 640,
                LandTextureScale = 1f,
                LandSpeed = 5f,
                LandParallaxFactorX = 0.4f,
                LandParallaxFactorY = 0.2f,
                LandTextureName = Tx.Backgrounds.Oizus,
            },
            new()
            {
                LandTextureWidthPx = 640,
                LandTextureHeightPx = 640,
                LandTextureScale = 1f,
                LandSpeed = 5.5f,
                LandParallaxFactorX = 0.45f,
                LandParallaxFactorY = 0.25f,
                LandTextureName = Tx.Backgrounds.OizusClouds1,
            },
            new()
            {
                LandTextureWidthPx = 640,
                LandTextureHeightPx = 640,
                LandTextureScale = 1f,
                LandSpeed = 10f,
                LandParallaxFactorX = 0.6f,
                LandParallaxFactorY = 0.5f,
                LandTextureName = Tx.Backgrounds.OizusClouds2,
            }
        }
    };

    private static readonly ScrollingBackgroundConfig OizusOrbit = new()
    {
        Layers = new List<ScrollingBackgroundLayer>
        {
            new()
            {
                LandTextureWidthPx = 640,
                LandTextureHeightPx = 640,
                LandTextureScale = 1f,
                LandSpeed = 5f,
                LandParallaxFactorX = 0.4f,
                LandParallaxFactorY = 0.2f,
                LandTextureName = Tx.Backgrounds.Oizus,
            }
        }
    };

    public static ScrollingBackgroundConfig GetScrollingBackgroundConfig(World activeWorld)
    {
        return activeWorld switch
        {
            World.Asteroid => OizusAsteroid,
            World.ShipDownstairs => OizusOrbit,
            World.ShipUpstairs => OizusOrbit,
            _ => null
        };
    }
}