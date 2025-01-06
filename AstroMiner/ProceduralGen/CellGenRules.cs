#nullable enable
using System;
using AstroMiner.Definitions;

namespace AstroMiner.ProceduralGen;

public static class CellGenRules
{
    public static CellType EvaluateRules(float distancePerc, float noise1Value, float noise2Value)
    {
        foreach (var rule in GameConfig.AsteroidGen.OrderedRules)
            if (rule.Matches(distancePerc, noise1Value, noise2Value))
                return rule.CellType;

        return CellType.Empty;
    }
}

public class RuleOptions
{
    public (float start, float end) DistanceRange { get; set; }
    public (float start, float end)? Noise1Range { get; set; }

    public (float start, float end)? Noise2Range { get; set; }
    public Func<float, (float start, float end)>? GetNoise1Range { get; set; }
}

public class Rule(
    CellType cellType,
    RuleOptions p)
{
    public CellType CellType => cellType;

    public bool Matches(float distancePerc, float noise1Value, float noise2Value)
    {
        var (startDistance, endDistance) = p.DistanceRange;

        // Early return if distance out of bounds of current rule
        if (distancePerc < startDistance || distancePerc > endDistance) return false;

        var noiseRange1 = p.Noise1Range;

        if (p.GetNoise1Range != null)
        {
            var layerSize = endDistance - startDistance;
            var distanceFromStartOfRuleLayer = distancePerc - startDistance;
            var distanceFromStartOfRuleLayerPercentage = distanceFromStartOfRuleLayer / layerSize;
            // Apply modifier to allowance
            noiseRange1 = p.GetNoise1Range(distanceFromStartOfRuleLayerPercentage);
        }

        if (noiseRange1 is var (start1, end1) && (noise1Value < start1 || noise1Value > end1)) return false;
        if (p.Noise2Range is var (start2, end2) && (noise2Value < start2 || noise2Value > end2)) return false;

        return true;
    }
}