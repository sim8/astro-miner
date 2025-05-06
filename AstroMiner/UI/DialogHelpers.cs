using System.Collections.Generic;
using AstroMiner.Utilities;

namespace AstroMiner.UI;

public static class DialogHelpers
{
    public const int DialogBoxWidth = 100;
    public const int DialogBoxHeight = 64;

    // TODO either return sourceRect format or cache transformString
    // Also get AI to rewrite this lol
    public static List<string> BreakDialogIntoVisibleLines(string dialog)
    {
        var lines = new List<string>();
        var trimmed = dialog.Trim();
        var stringSourceRects = FontHelpers.TransformString(trimmed);
        var lineLengthPx = 0;
        var wordLengthPx = 0;
        var lineStartIndex = 0;
        var wordStartIndex = 0;


        for (var i = 0; i < dialog.Length; i++)
            if (dialog[i] != ' ')
            {
                wordLengthPx += stringSourceRects[i].width;
            }
            else
            {
                // End of word
                if (lineLengthPx + wordLengthPx + stringSourceRects[i].width <= DialogBoxWidth)
                {
                    lineLengthPx += wordLengthPx + stringSourceRects[i].width;
                }
                else
                {
                    // Assume 1 space between each letter
                    // TODO case where word longer than width
                    lines.Add(dialog.Substring(lineStartIndex, wordStartIndex - 1 - lineStartIndex));
                    lineLengthPx = 0;
                    lineStartIndex = wordStartIndex;
                }

                wordLengthPx = 0;
                wordStartIndex = i + 1; // Assume 1 space between each letter
            }


        // Last word
        if (lineLengthPx + wordLengthPx <= DialogBoxWidth)
        {
            lines.Add(dialog.Substring(lineStartIndex));
        }
        else
        {
            lines.Add(dialog.Substring(lineStartIndex, wordStartIndex - 1));
            lines.Add(dialog.Substring(wordStartIndex)); // Assume can fit on one line
        }

        return lines;
    }
}