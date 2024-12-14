namespace AstroMiner;

public static class GameConfig
{
    public const int MinerBoxSizePx = 38;
    public const int PlayerBoxSizePx = 18;
    public const int ScaleMultiplier = 3;
    public const int CellTextureSizePx = 32;
    public const int GridSize = 100;
    public const float MinEmbarkingDistance = 1.2f;

    // Derived consts
    public const float MinerSize = (float)MinerBoxSizePx / CellTextureSizePx;
    public const int CellDisplayedSizePx = CellTextureSizePx * ScaleMultiplier;
}