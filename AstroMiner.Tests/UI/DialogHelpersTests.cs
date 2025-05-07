using System.Collections.Generic;
using System.Linq;
using AstroMiner.UI;
using AstroMiner.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroMiner.Tests.UI;

[TestClass]
public class DialogHelpersTests
{
    [TestInitialize]
    public void Setup()
    {
        // Set up a mock character size for consistent testing
        FontHelpers.MockCharacterSizeForTest = (x: 0, y: 0, width: 5, height: 8);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Reset the mock after tests
        FontHelpers.MockCharacterSizeForTest = null;
    }

    [TestMethod]
    public void BreakDialogIntoVisibleLines_EmptyString_ReturnsEmptyList()
    {
        var result = DialogHelpers.BreakDialogIntoVisibleLines("");
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void BreakDialogIntoVisibleLines_SingleShortLine_ReturnsSingleLine()
    {
        var text = "Hello world";
        var result = DialogHelpers.BreakDialogIntoVisibleLines(text);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Hello world", result[0]);
    }

    [TestMethod]
    public void BreakDialogIntoVisibleLines_LongLine_WrapsWords()
    {
        // Each character is 5 units wide, space is 5 units
        // DialogBoxWidth is 100 units, so we can fit about 20 characters per line
        var text = "This is a very long line that should be wrapped across multiple lines";
        var result = DialogHelpers.BreakDialogIntoVisibleLines(text);

        Assert.IsTrue(result.Count > 1);
        foreach (var line in result)
        {
            var lineWidth = FontHelpers.TransformString(line).Sum(r => r.width);
            Assert.IsTrue(lineWidth <= DialogHelpers.DialogBoxWidth);
        }
    }

    [TestMethod]
    public void BreakDialogIntoVisibleLines_LongWord_HyphenatesWord()
    {
        var text = "supercalifragilisticexpialidocious";
        var result = DialogHelpers.BreakDialogIntoVisibleLines(text);

        Assert.IsTrue(result.Count > 1);
        Assert.IsTrue(result[0].EndsWith("-"));

        foreach (var line in result)
        {
            var lineWidth = FontHelpers.TransformString(line).Sum(r => r.width);
            Assert.IsTrue(lineWidth <= DialogHelpers.DialogBoxWidth);
        }
    }

    [TestMethod]
    public void BreakDialogIntoVisibleLines_MixedContent_HandlesCorrectly()
    {
        var text = "This is a normal sentence with a supercalifragilisticexpialidocious word in it";
        var result = DialogHelpers.BreakDialogIntoVisibleLines(text);

        Assert.IsTrue(result.Count > 2);
        Assert.IsTrue(result.Any(line => line.EndsWith("-")));

        foreach (var line in result)
        {
            var lineWidth = FontHelpers.TransformString(line).Sum(r => r.width);
            Assert.IsTrue(lineWidth <= DialogHelpers.DialogBoxWidth);
        }
    }
}