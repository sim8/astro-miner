namespace AstroMiner;

public static class GameConfig
{
    public const int MinerBoxSizePx = 38;
    public const int PlayerBoxSizePx = 18;
    public const float ZoomLevelMiner = 2f;
    public const float ZoomLevelPlayer = 3f;
    public const int CellTextureSizePx = 32;
    public const int GridSize = 180;
    public const float MinEmbarkingDistance = 1.2f;

    // e.g. for a 7x7 block of rock, the center cell should be invisible
    public const int MaxUnexploredCellsVisible = 3;

    public const float PlayerMaxHealth = 100;
    public const float MinerMaxHealth = 200;

    public const int LavaDamagePerSecond = 30;

    public const int DamageAnimationTimeMs = 1000;

    // Damage ramps down to 0 based on distance from center + explosion radius
    public const int ExplosionMaxDamage = 120;

    // Derived consts
    public const float MinerSize = (float)MinerBoxSizePx / CellTextureSizePx;
}