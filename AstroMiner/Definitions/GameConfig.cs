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
    public const int MaxGrappleLength = 5;
    public const int GrappleCooldownMs = 3000;

    public const int AsteroidExplodeTimeMs = 1000 * 60 * 5 + 999;

    // e.g. for a 7x7 block of rock, the center cell should be invisible
    public const int MaxUnexploredCellsVisible = 3;

    // Distance threshold for showing fog of war gradients
    public const int ShowGradientsAtDistance = 4;

    public const float PlayerMaxHealth = 100;
    public const float MinerMaxHealth = 200;

    public const int LavaDamagePerSecond = 50;

    public const int LavaDamageDelayMs = 1000;

    public const int DamageAnimationTimeMs = 1000;

    public const int CollapsingFloorSpreadTime = 700;

    // Damage ramps down to 0 based on distance from center + explosion radius
    public const int ExplosionMaxDamage = 180;

    // Derived consts
    public const float MinerSize = (float)MinerBoxSizePx / CellTextureSizePx;
    public const float PlayerSize = (float)PlayerBoxSizePx / CellTextureSizePx;

    public static class Launch
    {
        // When crossing this Y threshold, switch to asteroid
        // public const int HomeToAsteroidPointY = -110;
        public const int HomeToAsteroidPointY = -300;
        // Miner starts slightly off the asteroid


        public const int AsteroidStartYOffset = 0;

        public const float AsteroidSpeed = 60f;

        public static readonly (int x, int y) MinerHomeStartPosCenter = (2, 4);
    }


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
        ///     Rules are iterated until both WallType and FloorType matched.
        /// </summary>
        public static readonly List<Rule> OrderedRules = new()
        {
            //----------------------------------------------
            // CORE RULES
            //----------------------------------------------
            new WallRule(WallType.Diamond, new RuleOptions
            {
                DistanceRange = (0f, 0.1f),
                Noise1Range = (0.7f, 1f)
            }),
            new WallRule(WallType.SolidRock, new RuleOptions
            {
                DistanceRange = (0f, CoreRadius),
                Noise1Range = (0.53f, 1f)
            }),
            new WallRule(WallType.ExplosiveRock, new RuleOptions
            {
                DistanceRange = (0f, CoreRadius),
                Noise1Range = (0.38f, 0.39f)
            }),
            //----------------------------------------------
            // MANTLE RULES
            //----------------------------------------------
            new WallAndFloorRule(WallType.Empty, FloorType.Lava, new RuleOptions
            {
                DistanceRange = (CoreRadius, MantleRadius - 0.03f),
                Noise2Range = (0.55f, 1f)
            }),
            new FloorRule(FloorType.LavaCracks, new RuleOptions
            {
                DistanceRange = (CoreRadius, MantleRadius - 0.03f),
                Noise1Range = (0.5f, 0.9f),
                Noise2Range = (0.4f, 1f) // Overlap with lava range so it borders
            }),
            new WallAndFloorRule(WallType.Empty, FloorType.Floor, new RuleOptions
            {
                DistanceRange = (CoreRadius, MantleRadius),
                GetNoise1Range = distancePercentage => // Taper off towards center
                    (0f, 0.65f - (1f - distancePercentage) / 4),
                Noise2Range = (0.42f, 1f) // Overlap with lava range so it borders
            }),
            new WallRule(WallType.SolidRock, new RuleOptions
            {
                DistanceRange = (CoreRadius, MantleRadius),
                Noise1Range = (0.59f, 0.65f)
            }),
            new WallRule(WallType.Gold, new RuleOptions
            {
                DistanceRange = (CoreRadius - 0.1f, CoreRadius + 0.2f),
                Noise1Range = (0.42f, 0.43f),
                Noise2Range = (0.42f, 0.55f)
            }),
            new WallRule(WallType.LooseRock, new RuleOptions
            {
                DistanceRange = (CoreRadius, MantleRadius),
                Noise1Range = (0.3f, 0.45f)
            }),
            //----------------------------------------------
            // CRUST RULES
            //----------------------------------------------
            new WallAndFloorRule(WallType.Empty, FloorType.Floor, new RuleOptions
            {
                DistanceRange = (MantleRadius, 1f),
                GetNoise1Range = distancePercentage =>
                {
                    var distanceToStartFadingToFloor = 0.3f;
                    var amountToWidenBy = Math.Max(distancePercentage - distanceToStartFadingToFloor, 0f) / 3;
                    return (0.49f - amountToWidenBy, 0.51f + amountToWidenBy);
                }
            }),
            new WallRule(WallType.Nickel, new RuleOptions
            {
                DistanceRange = (MantleRadius, 1f),
                Noise1Range = (0f, 0.25f)
            }),
            new WallRule(WallType.LooseRock, new RuleOptions
            {
                DistanceRange = (MantleRadius, 1f),
                Noise2Range = (0.4f, 0.55f)
            }),
            //----------------------------------------------
            // ALL LAYERS RULES
            //----------------------------------------------
            new WallRule(WallType.Ruby, new RuleOptions
            {
                DistanceRange = (0f, 1f),
                Noise1Range = (0.410f, 0.415f)
            }),
            new WallAndFloorRule(WallType.Rock, FloorType.Floor, new RuleOptions
            {
                DistanceRange = (0f, 1f) // Fill any remaining
            })
        };
    }
}