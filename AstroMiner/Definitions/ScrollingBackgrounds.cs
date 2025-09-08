using System.Collections.Generic;

namespace AstroMiner.Definitions;

public record ScrollingBackgroundLayer
{
    public string TextureName { get; init; } = Tx.Backgrounds.Oizus;
    public int TextureWidthPx { get; init; } = 1600;
    public int TextureHeightPx { get; init; } = 1600;
    public float Speed { get; init; } = 0.5f;
    public float ParallaxFactorX { get; init; } = 0.4f;
    public float ParallaxFactorY { get; init; } = 0.2f;
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
                TextureName = Tx.Backgrounds.Oizus,
                TextureWidthPx = 640,
                TextureHeightPx = 640,
                Speed = 5f,
                ParallaxFactorX = 0.4f,
                ParallaxFactorY = 0.2f,
            },
            new()
            {
                TextureName = Tx.Backgrounds.OizusClouds1,
                TextureWidthPx = 640,
                TextureHeightPx = 640,
                Speed = 6f,
                ParallaxFactorX = 0.5f,
                ParallaxFactorY = 0.25f,
            },
            new()
            {
                TextureName = Tx.Backgrounds.OizusClouds2,
                TextureWidthPx = 640,
                TextureHeightPx = 640,
                Speed = 10f,
                ParallaxFactorX = 1f,
                ParallaxFactorY = 0.5f,
            }
        }
    };

    private static readonly ScrollingBackgroundConfig OizusOrbit = new()
    {
        Layers = new List<ScrollingBackgroundLayer>
        {
            new()
            {
                TextureWidthPx = 640,
                TextureHeightPx = 640,
                Speed = 5f,
                ParallaxFactorX = 0.4f,
                ParallaxFactorY = 0.2f,
                TextureName = Tx.Backgrounds.Oizus,
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