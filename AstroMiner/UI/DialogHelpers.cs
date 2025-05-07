using System.Collections.Generic;
using System.Linq;
using AstroMiner.Utilities;

namespace AstroMiner.UI;

public static class DialogHelpers
{
    public const int DialogBoxWidth = 256;
    public const int DialogBoxHeight = 64;

    /// <summary>
    /// Breaks a dialog string into visible lines based on the DialogBoxWidth.
    /// </summary>
    /// <param name="dialog">The input dialog text</param>
    /// <returns>A list of string lines that will fit within the dialog box width</returns>
    public static List<string> BreakDialogIntoVisibleLines(string dialog)
    {
        var lines = new List<string>();
        var trimmed = dialog.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return lines;
        }

        var words = trimmed.Split(' ');
        var currentLine = new List<string>();
        var currentLineWidth = 0;

        foreach (var word in words)
        {
            var wordRects = FontHelpers.TransformString(word);
            var wordWidth = wordRects.Sum(r => r.width);

            // If the word itself is longer than the dialog width, hyphenate it
            if (wordWidth > DialogBoxWidth)
            {
                if (currentLine.Any())
                {
                    lines.Add(string.Join(" ", currentLine));
                    currentLine.Clear();
                    currentLineWidth = 0;
                }

                var chars = word.ToCharArray();
                var partialWord = new List<char>();
                var partialWidth = 0;

                foreach (var c in chars)
                {
                    var charRect = FontHelpers.TransformString(c.ToString())[0];
                    if (partialWidth + charRect.width + FontHelpers.TransformString("-")[0].width <= DialogBoxWidth)
                    {
                        partialWord.Add(c);
                        partialWidth += charRect.width;
                    }
                    else
                    {
                        partialWord.Add('-');
                        lines.Add(new string(partialWord.ToArray()));
                        partialWord.Clear();
                        partialWord.Add(c);
                        partialWidth = charRect.width;
                    }
                }

                if (partialWord.Any())
                {
                    lines.Add(new string(partialWord.ToArray()));
                }
                continue;
            }

            // Add space width if this isn't the first word in the line
            var spaceWidth = currentLine.Any() ? FontHelpers.TransformString(" ")[0].width : 0;
            var totalWidth = currentLineWidth + wordWidth + spaceWidth;

            if (totalWidth <= DialogBoxWidth)
            {
                currentLine.Add(word);
                currentLineWidth = totalWidth;
            }
            else
            {
                if (currentLine.Any())
                {
                    lines.Add(string.Join(" ", currentLine));
                    currentLine.Clear();
                }
                currentLine.Add(word);
                currentLineWidth = wordWidth;
            }
        }

        if (currentLine.Any())
        {
            lines.Add(string.Join(" ", currentLine));
        }

        return lines;
    }
}