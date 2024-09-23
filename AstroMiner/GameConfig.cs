namespace AstroMiner;

public static class GameConfig
{
    public const int MinerTextureSizePx = 38;
    public const int ScaleMultiplier = 3;
    public const int CellTextureSizePx = 64;
    public const int GridSize = 40;

    // Derived consts
    public const float MinerSize = (float)MinerTextureSizePx / CellTextureSizePx;
    public const int CellDisplayedSizePx = CellTextureSizePx * ScaleMultiplier;
    public const int MinerVisibleRadius = MinerTextureSizePx * ScaleMultiplier / 2;
}