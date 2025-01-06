using System;
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

        public const int AverageRadius = 80;
        public const int MaxDeviation = 12; // Adjusted for larger imperfections
        public const double MaxDelta = 9; // Adjusted for smoother transitions
        public const int AngleSegments = 140; // Adjusted for larger-scale variations


        // Two different layers of Perlin noise
        // Layer 1 - Default layer, more granular
        // Layer 2 - Lower frequency - used to define larger areas
        //   - Lava lakes + adjoining floor
        //   - Gold zones (near lava)
        public const float Perlin1NoiseScale = 0.22f;
        public const float Perlin2NoiseScale = 0.14f;

        /// <summary>
        ///     Defines the ordered list of cell generation rules when populating the grid.
        ///     Inputs to each rule are:
        ///     - DistanceRange - values can be from 0f (center) to 1f (edge)
        ///     - Noise1Range/Noise2Range - the range of noise values required to pass the
        ///     rule. If both are present, both are required.
        ///     - GetNoise1Range - can be specified instead of Noise1Range. Is passed the
        ///     percentage distance from the start to end of current DistanceRange
        /// </summary>
        public static readonly List<Rule> OrderedRules = new()
        {
            //----------------------------------------------
            // CORE RULES
            //----------------------------------------------
            new Rule(CellType.Diamond, new RuleOptions
            {
                DistanceRange = (0f, CoreRadius),
                Noise1Range = (0.7f, 1f)
            }),
            new Rule(CellType.SolidRock, new RuleOptions
            {
                DistanceRange = (0f, CoreRadius),
                Noise1Range = (0.56f, 1f)
            }),
            new Rule(CellType.ExplosiveRock, new RuleOptions
            {
                DistanceRange = (0f, CoreRadius),
                Noise1Range = (0.38f, 0.39f)
            }),
            //----------------------------------------------
            // MANTLE RULES
            //----------------------------------------------
            new Rule(CellType.Lava, new RuleOptions
            {
                DistanceRange = (CoreRadius, MantleRadius - 0.03f),
                Noise2Range = (0.55f, 1f)
            }),
            new Rule(CellType.Floor, new RuleOptions
            {
                DistanceRange = (CoreRadius, MantleRadius),
                GetNoise1Range = distancePercentage => // Taper off towards center
                    (0f, 0.65f - (1f - distancePercentage) / 4),
                Noise2Range = (0.42f, 1f) // Overlap with lava range so it borders
            }),
            new Rule(CellType.SolidRock, new RuleOptions
            {
                DistanceRange = (CoreRadius, MantleRadius),
                Noise1Range = (0.67f, 1f)
            }),
            new Rule(CellType.Gold, new RuleOptions
            {
                DistanceRange = (CoreRadius - 0.1f, CoreRadius + 0.2f),
                Noise1Range = (0.42f, 0.43f),
                Noise2Range = (0.42f, 0.55f)
            }),
            //----------------------------------------------
            // CRUST RULES
            //----------------------------------------------
            new Rule(CellType.Floor, new RuleOptions
            {
                DistanceRange = (MantleRadius, 1f),
                GetNoise1Range = distancePercentage =>
                {
                    var distanceToStartFadingToFloor = 0.3f;
                    var amountToWidenBy = Math.Max(distancePercentage - distanceToStartFadingToFloor, 0f) / 3;
                    return (0.49f - amountToWidenBy, 0.51f + amountToWidenBy);
                }
            }),
            new Rule(CellType.Nickel, new RuleOptions
            {
                DistanceRange = (MantleRadius, 1f),
                Noise1Range = (0f, 0.25f)
            }),
            //----------------------------------------------
            // ALL LAYERS RULES
            //----------------------------------------------
            new Rule(CellType.Ruby, new RuleOptions
            {
                DistanceRange = (0f, 1f),
                Noise1Range = (0.410f, 0.415f)
            }),
            new Rule(CellType.Rock, new RuleOptions
            {
                DistanceRange = (0f, 1f) // Fill any remaining
            })
        };
    }
}