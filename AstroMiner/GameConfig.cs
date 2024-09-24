namespace AstroMiner;

public static class GameConfig
{
    public const int MinerBoxSizePx = 38;
    public const int PlayerBoxSizePx = 18;
    public const int ScaleMultiplier = 3;
    public const int CellTextureSizePx = 64;
    public const int GridSize = 40;
    public const float MinEmbarkingDistance = 0.7f;

    // Derived consts
    public const float MinerSize = (float)MinerBoxSizePx / CellTextureSizePx;
    public const int CellDisplayedSizePx = CellTextureSizePx * ScaleMultiplier;
    public const int MinerVisibleRadius = MinerBoxSizePx * ScaleMultiplier / 2;
}