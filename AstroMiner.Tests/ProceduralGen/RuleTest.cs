using AstroMiner.ProceduralGen;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroMiner.Tests.ProceduralGen;

[TestClass]
[TestSubject(typeof(CellGenRules.Rule))]
public class RuleTest
{
    [TestMethod]
    public void Rule_BasicNoise1Rule()
    {
        var rule = new CellGenRules.Rule(
            new CellGenRules.RuleParams
            {
                StartDistance = 0.3f,
                EndDistance = 0.6f,
                Noise1Target = 0.5f,
                BaseAllowance = 0.05f
            }
        );

        var result = rule.Matches(0.47f, 0.5f, 0.1f);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Rule_AllowanceIsSharedAcrossNoiseLayers()
    {
        var rule = new CellGenRules.Rule(
            new CellGenRules.RuleParams
            {
                StartDistance = 0.3f,
                EndDistance = 0.6f,
                Noise1Target = 0.5f,
                Noise2Target = 0.8f,
                BaseAllowance = 0.05f
            }
        );

        var negativeResult = rule.Matches(0.46f, 0.46f, 0.84f);
        Assert.IsFalse(negativeResult);

        var positiveResult = rule.Matches(0.46f, 0.48f, 0.82f);
        Assert.IsTrue(positiveResult);
    }

    [TestMethod]
    public void Rule_AllowanceModifierWorks()
    {
        var rule = new CellGenRules.Rule(
            new CellGenRules.RuleParams
            {
                StartDistance = 0.1f,
                EndDistance = 0.2f,
                Noise1Target = 0.5f,
                BaseAllowance = 0f,
                AllowanceModifier = (allowance, distanceFromStartOfRuleLayerPercentage) =>
                    distanceFromStartOfRuleLayerPercentage > 0.8 ? 1f : 0f
            }
        );

        var negativeResult = rule.Matches(0.17f, 0.48f, 0.1f);
        Assert.IsFalse(negativeResult);

        var positiveResult = rule.Matches(0.19f, 0.48f, 0.1f);
        Assert.IsTrue(positiveResult);
    }
}