#nullable enable
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

public class RuleParams
{
    public CellType CellType { get; set; }
    public (float start, float end) DistanceRange { get; set; }
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
        var (startDistance, endDistance) = p.DistanceRange;

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