using System;
using System.Collections.Generic;

namespace AstroMiner.Utilities;

public static class FontHelpers
{
    public const int FontHeightPx = 8;

    // For testing purposes only
    public static (int x, int y, int width, int height)? MockCharacterSizeForTest { get; set; } = null;

    private static readonly Dictionary<char, (int x, int y, int width)> Chars = new()
    {
        { 'A', (0, 0, 7) },
        { 'B', (7, 0, 7) },
        { 'C', (14, 0, 7) },
        { 'D', (21, 0, 8) },
        { 'E', (29, 0, 7) },
        { 'F', (36, 0, 7) },
        { 'G', (43, 0, 7) },
        { 'H', (50, 0, 7) },
        { 'I', (57, 0, 5) },
        { 'J', (62, 0, 6) },
        { 'K', (68, 0, 7) },
        { 'L', (75, 0, 6) },
        { 'M', (81, 0, 8) },
        { 'N', (89, 0, 7) },
        { 'O', (96, 0, 7) },
        { 'P', (103, 0, 7) },
        { 'Q', (110, 0, 7) },
        { 'R', (117, 0, 7) },
        { 'S', (124, 0, 7) },
        { 'T', (131, 0, 7) },
        { 'U', (138, 0, 7) },
        { 'V', (145, 0, 7) },
        { 'W', (152, 0, 9) },
        { 'X', (161, 0, 7) },
        { 'Y', (168, 0, 7) },
        { 'Z', (175, 0, 7) },
        { ' ', (182, 0, 5) },
        { '0', (0, 18, 7) },
        { '1', (7, 18, 7) },
        { '2', (14, 18, 7) },
        { '3', (21, 18, 7) },
        { '4', (28, 18, 7) },
        { '5', (35, 18, 7) },
        { '6', (42, 18, 7) },
        { '7', (49, 18, 7) },
        { '8', (56, 18, 7) },
        { '9', (63, 18, 7) }
    };

    public static List<(int x, int y, int width, int height)> TransformString(string text)
    {
        var result = new List<(int x, int y, int width, int height)>();

        // If we're in test mode, return mock data
        if (MockCharacterSizeForTest.HasValue && !string.IsNullOrEmpty(text))
        {
            var mockChar = MockCharacterSizeForTest.Value;
            for (int i = 0; i < text.Length; i++)
            {
                result.Add(mockChar);
            }
            return result;
        }

        foreach (var c in text)
        {
            if (!Chars.ContainsKey(c)) throw new ArgumentException($"Character '{c}' is not in the font data.");
            result.Add((Chars[c].x, Chars[c].y, Chars[c].width, FontHeightPx));
        }

        return result;
    }
}