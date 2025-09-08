using System.Collections.Generic;

namespace AstroMiner.Definitions;

public record CloudConfig
{
    public int CloudTextureSizePx { get; init; } = 128;
    public float CloudTextureScale { get; init; } = 1.0f;
    public float CloudsPerGridCell { get; init; } = 0.1f;
    public int CloudSeed { get; init; } = 42;
    public float CloudSpeed { get; init; } = 1f;
    public float CloudParallaxFactorX { get; init; } = 0.6f;
    public float CloudParallaxFactorY { get; init; } = 0.3f;
    public string TextureName { get; init; } = "cloud-background";
}

public record ScrollingBackgroundConfig
{
    // Land configuration
    public int LandTextureWidthPx { get; init; } = 1600;
    public int LandTextureHeightPx { get; init; } = 1600;
    public float LandTextureScale { get; init; } = 0.5f;
    public float LandSpeed { get; init; } = 0.5f;
    public float LandParallaxFactorX { get; init; } = 0.4f;
    public float LandParallaxFactorY { get; init; } = 0.2f;
    public string LandTextureName { get; init; } = "mountains-nice-tiled";

    // Cloud configuration
    public List<CloudConfig> CloudLayers { get; init; } = new();
}

public static class ScrollingBackgrounds
{
    private static readonly ScrollingBackgroundConfig OizusAsteroid = new()
    {
        LandTextureWidthPx = 640,
        LandTextureHeightPx = 640,
        LandTextureScale = 1f,
        LandSpeed = 5f,
        LandParallaxFactorX = 0.4f,
        LandParallaxFactorY = 0.2f,
        LandTextureName = Tx.Backgrounds.Oizus,
        CloudLayers =
        [
            // new CloudConfig
            // {
            //     CloudTextureScale = 0.5f,
            //     CloudsPerGridCell = 0.1f,
            //     CloudSeed = 42,
            //     CloudSpeed = 1f,
            //     CloudParallaxFactorX = 0.6f,
            //     CloudParallaxFactorY = 0.3f
            // },
            // // Foreground cloud layer

            // new CloudConfig
            // {
            //     CloudTextureScale = 1f,
            //     CloudsPerGridCell = 0.05f,
            //     CloudSeed = 123,
            //     CloudSpeed = 1.5f,
            //     CloudParallaxFactorX = 0.8f,
            //     CloudParallaxFactorY = 0.4f
            // }
        ]
    };

    private static readonly ScrollingBackgroundConfig OizusOrbit = new()
    {
        LandTextureWidthPx = 1600,
        LandTextureHeightPx = 1600,
        LandTextureScale = 0.0625f,
        LandSpeed = 0.2f,
        LandParallaxFactorX = 0.4f,
        LandParallaxFactorY = 0.2f,
        LandTextureName = Tx.Backgrounds.Oizus,
        CloudLayers =
        [

        ]
    };

    public static ScrollingBackgroundConfig? GetScrollingBackgroundConfig(World activeWorld)
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