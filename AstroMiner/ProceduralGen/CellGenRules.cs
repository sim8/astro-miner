#nullable enable
using System;
using System.Collections.Generic;
using AstroMiner.Definitions;

namespace AstroMiner.ProceduralGen;

public static class CellGenRules
{
    private static readonly List<Rule> _orderedRules = new()
    {
        // Lava perimeter
        new Rule(new RuleParams
        {
            CellType = CellType.Floor,
            StartDistance = GameConfig.AsteroidGen.CoreRadius,
            EndDistance = GameConfig.AsteroidGen.MantleRadius,
            Noise1Target = 0.3f,
            Noise2Target = 0.485f,
            BaseAllowance = 0.3f,
            AllowanceModifier = (allowance, distanceFromStartOfRuleLayerPercentage) =>
                allowance * distanceFromStartOfRuleLayerPercentage
        }),
        new Rule(new RuleParams
        {
            CellType = CellType.Lava,
            StartDistance = GameConfig.AsteroidGen.CoreRadius,
            EndDistance = GameConfig.AsteroidGen.MantleRadius,
            Noise2Target = 0.775f,
            BaseAllowance = 0.225f
        }),
        new Rule(new RuleParams
        {
            CellType = CellType.Gold,
            StartDistance = GameConfig.AsteroidGen.CoreRadius - 0.1f,
            EndDistance = GameConfig.AsteroidGen.CoreRadius + 0.2f,
            Noise2Target = 0.77f,
            BaseAllowance = 0.04f
        }),
        new Rule(new RuleParams
        {
            CellType = CellType.Ruby,
            Noise1Target = 0.4125f,
            BaseAllowance = 0.0025f
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
        public float? Noise1Target { get; set; }
        public float? Noise2Target { get; set; }
        public float BaseAllowance { get; set; }
        public Func<float, float, float>? AllowanceModifier { get; set; }
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

            var allowance = p.BaseAllowance;
            if (p.AllowanceModifier != null)
            {
                var layerSize = endDistance - startDistance;
                var distanceFromStartOfRuleLayer = distancePerc - startDistance;
                var distanceFromStartOfRuleLayerPercentage = distanceFromStartOfRuleLayer / layerSize;
                // Apply modifier to allowance
                allowance = p.AllowanceModifier(allowance, distanceFromStartOfRuleLayerPercentage);
            }

            var totalDistanceFromTargetAcrossAllNoiseLayers = 0f;


            if (p.Noise1Target.HasValue)
                totalDistanceFromTargetAcrossAllNoiseLayers += Math.Abs(noise1Value - p.Noise1Target.Value);

            if (p.Noise2Target.HasValue)
                totalDistanceFromTargetAcrossAllNoiseLayers += Math.Abs(noise2Value - p.Noise2Target.Value);

            return totalDistanceFromTargetAcrossAllNoiseLayers <= allowance;
        }
    }
}