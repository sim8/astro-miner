namespace AstroMiner.Definitions;

public record ScrollingBackgroundConfig
{
    // Land configuration
    public int LandTextureWidthPx { get; init; } = 1600;
    public int LandTextureHeightPx { get; init; } = 1600;
    public float LandTextureScale { get; init; } = 0.5f;
    public float LandSpeed { get; init; } = 0.5f;
    public float LandParallaxFactorX { get; init; } = 0.4f;
    public float LandParallaxFactorY { get; init; } = 0.2f;

    // Cloud configuration
    public int CloudTextureSizePx { get; init; } = 128;
    public float CloudsPerGridCell { get; init; } = 0.1f;
    public int CloudSeed { get; init; } = 42;
    public float CloudSpeed { get; init; } = 1f;
    public float CloudParallaxFactorX { get; init; } = 0.6f;
    public float CloudParallaxFactorY { get; init; } = 0.3f;
}

public static class ScrollingBackgrounds
{
    public static readonly ScrollingBackgroundConfig OizusAsteroid = new()
    {
        LandTextureWidthPx = 1600,
        LandTextureHeightPx = 1600,
        LandTextureScale = 0.5f,
        LandSpeed = 0.5f,
        LandParallaxFactorX = 0.4f,
        LandParallaxFactorY = 0.2f,
        CloudTextureSizePx = 128,
        CloudsPerGridCell = 0.1f,
        CloudSeed = 42,
        CloudSpeed = 1f,
        CloudParallaxFactorX = 0.6f,
        CloudParallaxFactorY = 0.3f
    };
}