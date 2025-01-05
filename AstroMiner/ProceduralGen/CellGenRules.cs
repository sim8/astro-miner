#nullable enable
using System.Collections.Generic;
using AstroMiner.Definitions;

namespace AstroMiner.ProceduralGen;

public static class CellGenRules
{
    // TODO consts for "borders" between two cell types
    private static readonly List<Rule> _orderedRules = new()
    {
        //----------------------------------------------
        // CORE RULES
        //----------------------------------------------
        new Rule(new RuleParams
        {
            CellType = CellType.Diamond,
            EndDistance = GameConfig.AsteroidGen.CoreRadius,
            Noise1Range = (0.7f, 1f)
        }),
        new Rule(new RuleParams
        {
            CellType = CellType.SolidRock,
            EndDistance = GameConfig.AsteroidGen.CoreRadius,
            Noise1Range = (0.56f, 1f)
        }),
        new Rule(new RuleParams
        {
            CellType = CellType.ExplosiveRock,
            EndDistance = GameConfig.AsteroidGen.CoreRadius,
            Noise1Range = (0.38f, 0.39f)
        }),
        //----------------------------------------------
        // MANTLE RULES
        //----------------------------------------------
        new Rule(new RuleParams
        {
            CellType = CellType.Floor,
            StartDistance = GameConfig.AsteroidGen.CoreRadius,
            EndDistance = GameConfig.AsteroidGen.MantleRadius,
            Noise1Range = (0.4f, 0.6f),
            Noise2Range = (0.42f, 0.55f)
        }),
        new Rule(new RuleParams
        {
            CellType = CellType.Lava,
            StartDistance = GameConfig.AsteroidGen.CoreRadius,
            EndDistance = GameConfig.AsteroidGen.MantleRadius,
            Noise2Range = (0.55f, 1f)
        }),
        new Rule(new RuleParams
        {
            CellType = CellType.SolidRock,
            StartDistance = GameConfig.AsteroidGen.CoreRadius,
            EndDistance = GameConfig.AsteroidGen.MantleRadius,
            Noise1Range = (0.65f, 1f)
        }),
        new Rule(new RuleParams
        {
            CellType = CellType.Gold,
            StartDistance = GameConfig.AsteroidGen.CoreRadius - 0.1f,
            EndDistance = GameConfig.AsteroidGen.CoreRadius + 0.2f,
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

    public static CellType EvaluateRules(float distancePerc, float noise1Value, float noise2Value)
    {
        foreach (var rule in _orderedRules)
            if (rule.Matches(distancePerc, noise1Value, noise2Value))
                return rule.CellType;

        return CellType.Empty;
    }

    public class RuleParams
    {
        public CellType CellType { get; set; }
        public float? StartDistance { get; set; }
        public float? EndDistance { get; set; }
        public (float start, float end)? Noise1Range { get; set; }

        public (float start, float end)? Noise2Range { get; set; }
        // public Func<float, float, bool>? CustomRangeCheck { get; set; }
    }

    public class Rule(
        RuleParams p)
    {
        public CellType CellType => p.CellType;

        public bool Matches(float distancePerc, float noise1Value, float noise2Value)
        {
            var startDistance = p.StartDistance ?? 0f;
            var endDistance = p.EndDistance ?? 1f;

            // Early return if distance out of bounds of current rule
            if (distancePerc < startDistance || distancePerc > endDistance) return false;

            // if (p.CustomRangeCheck != null)
            // {
            //     var layerSize = endDistance - startDistance;
            //     var distanceFromStartOfRuleLayer = distancePerc - startDistance;
            //     var distanceFromStartOfRuleLayerPercentage = distanceFromStartOfRuleLayer / layerSize;
            //     // Apply modifier to allowance
            //     allowance = p.AllowanceModifier(allowance, distanceFromStartOfRuleLayerPercentage);
            // }
            if (p.Noise1Range is var (start1, end1) && (noise1Value < start1 || noise1Value > end1)) return false;
            if (p.Noise2Range is var (start2, end2) && (noise2Value < start2 || noise2Value > end2)) return false;

            return true;
        }
    }
}