using System.Collections.Generic;
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
        // Arrange
        var dialog = "";

        // Act
        var result = DialogHelpers.BreakDialogIntoVisibleLines(dialog);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void BreakDialogIntoVisibleLines_ShortText_ReturnsSingleLine()
    {
        // Arrange
        var dialog = "Short text";  // 10 characters * 5px = 50px (less than DialogBoxWidth)

        // Act
        var result = DialogHelpers.BreakDialogIntoVisibleLines(dialog);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Short text", result[0]);
    }

    [TestMethod]
    public void BreakDialogIntoVisibleLines_LongSentence_BreaksIntoMultipleLines()
    {
        // Arrange
        // Each character is 5px wide, and DialogBoxWidth is 100px
        // So we can fit 20 characters per line
        var dialog = "This is a long sentence that should be broken into multiple lines based on the dialog box width";

        // Act
        var result = DialogHelpers.BreakDialogIntoVisibleLines(dialog);

        // Assert
        Assert.IsTrue(result.Count > 1);
        // Verify that each line is not too long
        foreach (var line in result)
        {
            // 100px / 5px per character = 20 characters maximum
            Assert.IsTrue(line.Length <= 20, $"Line too long: {line}");
        }
    }

    [TestMethod]
    public void BreakDialogIntoVisibleLines_LongWordExceedingWidth_HandlesCorrectly()
    {
        // Arrange
        // Create a string with a very long word
        var dialog = "Normal SuperLongWordThatExceedsDialogBoxWidth normal";

        // Act
        var result = DialogHelpers.BreakDialogIntoVisibleLines(dialog);

        // Assert
        Assert.IsTrue(result.Count >= 2, "Should break the long word into at least two lines");
        // The long word should be on its own line or broken across lines
        bool containsLongWord = false;
        foreach (var line in result)
        {
            if (line.Contains("SuperLongWordThatExceedsDialogBoxWidth"))
            {
                containsLongWord = true;
                break;
            }
        }
        Assert.IsTrue(containsLongWord, "The long word should be included in the output");
    }

    [TestMethod]
    public void BreakDialogIntoVisibleLines_MultipleSentences_BreaksCorrectly()
    {
        // Arrange
        var dialog = "This is the first sentence. This is the second sentence. And this is the third.";

        // Act
        var result = DialogHelpers.BreakDialogIntoVisibleLines(dialog);

        // Assert
        Assert.IsTrue(result.Count > 1);

        // Verify that all content from the original string is preserved in the broken lines
        // We don't check for exact string matching since our implementation preserves spacing differently
        string combinedResult = string.Join("", result);
        string normalizedDialog = dialog.Replace(" ", "");
        string normalizedResult = combinedResult.Replace(" ", "");

        Assert.AreEqual(normalizedDialog, normalizedResult,
            "All text content should be preserved when breaking into lines (ignoring spaces)");

        // Also check that we didn't lose any sentences
        Assert.IsTrue(combinedResult.Contains("first"), "First sentence should be preserved");
        Assert.IsTrue(combinedResult.Contains("second"), "Second sentence should be preserved");
        Assert.IsTrue(combinedResult.Contains("third"), "Third sentence should be preserved");
    }
}