using System.Collections.Generic;
using AstroMiner.ProceduralGen;

namespace AstroMiner.Definitions;

public static class GameConfig
{
    public const int MinerBoxSizePx = 38;
    public const int PlayerBoxSizePx = 18;
    public const float ZoomLevelMiner = 2f;
    public const float ZoomLevelPlayer = 3f;
    public const int CellTextureSizePx = 32;
    public const int GridSize = 200;
    public const float MinEmbarkingDistance = 1.2f;

    // e.g. for a 7x7 block of rock, the center cell should be invisible
    public const int MaxUnexploredCellsVisible = 3;

    public const float PlayerMaxHealth = 100;
    public const float MinerMaxHealth = 200;

    public const int LavaDamagePerSecond = 50;

    public const int DamageAnimationTimeMs = 1000;

    // Damage ramps down to 0 based on distance from center + explosion radius
    public const int ExplosionMaxDamage = 180;

    // Derived consts
    public const float MinerSize = (float)MinerBoxSizePx / CellTextureSizePx;

    public static class AsteroidGen
    {
        public const float MantleRadius = 0.7f;
        public const float CoreRadius = 0.27f;

        // TODO consts for "borders" between two cell types
        public static readonly List<Rule> OrderedRules = new()
        {
            //----------------------------------------------
            // CORE RULES
            //----------------------------------------------
            new Rule(new RuleParams
            {
                CellType = CellType.Diamond,
                EndDistance = CoreRadius,
                Noise1Range = (0.7f, 1f)
            }),
            new Rule(new RuleParams
            {
                CellType = CellType.SolidRock,
                EndDistance = CoreRadius,
                Noise1Range = (0.56f, 1f)
            }),
            new Rule(new RuleParams
            {
                CellType = CellType.ExplosiveRock,
                EndDistance = CoreRadius,
                Noise1Range = (0.38f, 0.39f)
            }),
            //----------------------------------------------
            // MANTLE RULES
            //----------------------------------------------
            new Rule(new RuleParams
            {
                CellType = CellType.Floor,
                StartDistance = CoreRadius,
                EndDistance = MantleRadius,
                Noise1Range = (0.4f, 0.6f),
                Noise2Range = (0.42f, 0.55f)
            }),
            new Rule(new RuleParams
            {
                CellType = CellType.Lava,
                StartDistance = CoreRadius,
                EndDistance = MantleRadius,
                Noise2Range = (0.55f, 1f)
            }),
            new Rule(new RuleParams
            {
                CellType = CellType.SolidRock,
                StartDistance = CoreRadius,
                EndDistance = MantleRadius,
                Noise1Range = (0.65f, 1f)
            }),
            new Rule(new RuleParams
            {
                CellType = CellType.Gold,
                StartDistance = CoreRadius - 0.1f,
                EndDistance = CoreRadius + 0.2f,
                Noise1Range = (0.42f, 0.43f),
                Noise2Range = (0.42f, 0.55f)
            }),
            //----------------------------------------------
            // CRUST RULES
            //----------------------------------------------
            //----------------------------------------------
            // ALL LAYERS RULES
            //----------------------------------------------
            new Rule(new RuleParams
            {
                CellType = CellType.Ruby,
                Noise1Range = (0.410f, 0.415f)
            }),
            new Rule(new RuleParams
            {
                CellType = CellType.Rock
            })
        };
    }
}