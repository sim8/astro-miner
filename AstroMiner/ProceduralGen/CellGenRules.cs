#nullable enable
using System;
using AstroMiner.Definitions;

namespace AstroMiner.ProceduralGen;

public static class CellGenRules
{
    public static (WallType?, FloorType?) EvaluateRules(float distancePerc, float noise1Value, float noise2Value)
    {
        WallType? wallType = null;
        var hasWallTypeMatch = false;
        FloorType? floorType = null;
        var hasFloorTypeMatch = false;

        foreach (var rule in GameConfig.AsteroidGen.OrderedRules)
            if (rule.Matches(distancePerc, noise1Value, noise2Value))
            {
                if (!hasWallTypeMatch && rule is IWallRule wallRule)
                {
                    wallType = wallRule.GetWallType();
                    hasWallTypeMatch = true;
                }

                if (!hasFloorTypeMatch && rule is IFloorRule floorRule)
                {
                    floorType = floorRule.GetFloorType();
                    hasFloorTypeMatch = true;
                }
            }

        return (wallType, floorType);
    }
}

public class RuleOptions
{
    public (float start, float end) DistanceRange { get; set; }
    public (float start, float end)? Noise1Range { get; set; }

    public (float start, float end)? Noise2Range { get; set; }
    public Func<float, (float start, float end)>? GetNoise1Range { get; set; }
}

public abstract class Rule(RuleOptions p)
{
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

internal interface IWallRule
{
    public WallType? GetWallType();
}

public class WallRule(WallType wallType, RuleOptions ruleOptions) : Rule(ruleOptions), IWallRule
{
    public WallType? GetWallType()
    {
        return wallType;
    }
}

internal interface IFloorRule
{
    public FloorType? GetFloorType();
}

public class FloorRule(FloorType floorType, RuleOptions ruleOptions) : Rule(ruleOptions), IFloorRule
{
    public FloorType? GetFloorType()
    {
        return floorType;
    }
}

public class WallAndFloorRule(WallType? wallType, FloorType? floorType, RuleOptions ruleOptions)
    : Rule(ruleOptions), IWallRule, IFloorRule
{
    public FloorType? GetFloorType()
    {
        return floorType;
    }

    public WallType? GetWallType()
    {
        return wallType;
    }
}