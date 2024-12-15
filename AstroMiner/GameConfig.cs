namespace AstroMiner;

public static class GameConfig
{
    public const int MinerBoxSizePx = 38;
    public const int PlayerBoxSizePx = 18;
    public const float ZoomLevelMiner = 2f;
    public const float ZoomLevelPlayer = 3f;
    public const int CellTextureSizePx = 32;
    public const int GridSize = 100;
    public const float MinEmbarkingDistance = 1.2f;

    // Derived consts
    public const float MinerSize = (float)MinerBoxSizePx / CellTextureSizePx;
}