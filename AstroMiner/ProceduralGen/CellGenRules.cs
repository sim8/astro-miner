#nullable enable
using System;
using System.Collections.Generic;
using AstroMiner.Definitions;

namespace AstroMiner.ProceduralGen;

public static class CellGenRules
{
    public static readonly List<Rule> _rules = new();

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
        public bool Matches(float distance, float noise1Value, float noise2Value)
        {
            var startDistance = p.StartDistance ?? 0f;
            var endDistance = p.EndDistance ?? 1f;

            // Early return if distance out of bounds of current rule
            if (distance < startDistance || distance > endDistance) return false;

            var allowance = p.BaseAllowance;
            if (p.AllowanceModifier != null)
            {
                var layerSize = endDistance - startDistance;
                var distanceFromStartOfRuleLayer = distance - startDistance;
                var distanceFromStartOfRuleLayerPercentage = distanceFromStartOfRuleLayer / layerSize;
                // Apply modifier to allowance
                allowance = p.AllowanceModifier(allowance, distanceFromStartOfRuleLayerPercentage);
            }

            var totalDistanceFromTargetAcrossAllNoiseLayers = 0f;


            if (p.Noise1Target.HasValue)
                totalDistanceFromTargetAcrossAllNoiseLayers += Math.Abs(noise1Value - p.Noise1Target.Value);

            if (p.Noise2Target.HasValue)
                totalDistanceFromTargetAcrossAllNoiseLayers += Math.Abs(noise2Value - p.Noise2Target.Value);

            return totalDistanceFromTargetAcrossAllNoiseLayers < allowance;
        }
    }
}