using AstroMiner.Definitions;
using AstroMiner.ProceduralGen;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroMiner.Tests.ProceduralGen;

[TestClass]
[TestSubject(typeof(Rule))]
public class RuleTest
{
    [TestMethod]
    public void Rule_BasicNoise1Rule()
    {
        var rule = new Rule(
            CellType.Diamond,
            new RuleOptions
            {
                DistanceRange = (0.3f, 0.6f),
                Noise1Range = (0.45f, 0.55f)
            }
        );

        var result = rule.Matches(0.47f, 0.5f, 0.1f);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Rule_AllowanceModifierWorks()
    {
        var rule = new Rule(
            CellType.Diamond,
            new RuleOptions
            {
                DistanceRange = (0.1f, 0.2f),
                GetNoise1Range = distanceFromStartOfRuleLayerPercentage =>
                    distanceFromStartOfRuleLayerPercentage > 0.8 ? (0f, 1f) : (0f, 0f)
            }
        );

        var negativeResult = rule.Matches(0.17f, 0.48f, 0.1f);
        Assert.IsFalse(negativeResult);

        var positiveResult = rule.Matches(0.19f, 0.48f, 0.1f);
        Assert.IsTrue(positiveResult);
    }
}