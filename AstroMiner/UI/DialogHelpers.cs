using System.Collections.Generic;
using AstroMiner.Utilities;

namespace AstroMiner.UI;

public static class DialogHelpers
{
    public const int DialogBoxWidth = 100;
    public const int DialogBoxHeight = 64;

    /// <summary>
    /// Breaks a dialog string into visible lines based on the DialogBoxWidth.
    /// </summary>
    /// <param name="dialog">The input dialog text</param>
    /// <returns>A list of string lines that will fit within the dialog box width</returns>
    public static List<string> BreakDialogIntoVisibleLines(string dialog)
    {
        var lines = new List<string>();

        // Handle empty string case
        if (string.IsNullOrEmpty(dialog))
        {
            return lines;
        }

        var trimmed = dialog.Trim();
        // Also handle case where string is only whitespace
        if (string.IsNullOrEmpty(trimmed))
        {
            return lines;
        }

        var stringSourceRects = FontHelpers.TransformString(trimmed);
        var currentLineWidthPx = 0;
        var currentWordWidthPx = 0;
        var lineStartIndex = 0;
        var wordStartIndex = 0;

        // Process each character in the dialog
        for (var i = 0; i < trimmed.Length; i++)
        {
            if (trimmed[i] != ' ')
            {
                // Add character width to current word
                currentWordWidthPx += stringSourceRects[i].width;

                // If this is the last character, handle the final word
                if (i == trimmed.Length - 1)
                {
                    // Check if current line + current word fits
                    if (currentLineWidthPx + currentWordWidthPx <= DialogBoxWidth)
                    {
                        // Add the whole line including the last word
                        lines.Add(trimmed.Substring(lineStartIndex));
                    }
                    else
                    {
                        // Add the current line without the last word if not empty
                        if (wordStartIndex > lineStartIndex)
                        {
                            lines.Add(trimmed.Substring(lineStartIndex, wordStartIndex - lineStartIndex));
                        }

                        // Add the last word as a separate line
                        lines.Add(trimmed.Substring(wordStartIndex));
                    }
                }
            }
            else
            {
                // Space character - end of a word
                int spaceWidth = stringSourceRects[i].width;

                // Check if current line + current word + space fits
                if (currentLineWidthPx + currentWordWidthPx + spaceWidth <= DialogBoxWidth)
                {
                    // Word fits on current line, add its width and the space
                    currentLineWidthPx += currentWordWidthPx + spaceWidth;
                }
                else
                {
                    // Word doesn't fit, create a new line

                    // If there's content before this word on the current line
                    if (wordStartIndex > lineStartIndex)
                    {
                        // Add the current line without the current word
                        lines.Add(trimmed.Substring(lineStartIndex, wordStartIndex - lineStartIndex));
                    }

                    // Handle the case where a single word is longer than DialogBoxWidth
                    if (currentWordWidthPx > DialogBoxWidth)
                    {
                        // Future improvement: break the long word into multiple lines
                        // For now, just put it on its own line
                        lines.Add(trimmed.Substring(wordStartIndex, i - wordStartIndex));
                    }
                    else if (i > wordStartIndex)
                    {
                        // Start a new line with the current word
                        currentLineWidthPx = currentWordWidthPx + spaceWidth;
                    }

                    lineStartIndex = wordStartIndex;
                }

                // Reset word tracking for the next word
                currentWordWidthPx = 0;
                wordStartIndex = i + 1;
            }
        }

        // Handle case where we have content that hasn't been added to a line yet
        // This happens if the last line didn't exceed the width
        if (lineStartIndex < trimmed.Length && lines.Count == 0)
        {
            lines.Add(trimmed.Substring(lineStartIndex));
        }

        return lines;
    }
}