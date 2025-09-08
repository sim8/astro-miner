using System;

namespace AstroMiner.Utilities;

public static class MathHelpers
{
    public static float GetPercentageBetween(float value, float start, float end)
    {
        // Handle edge case where start equals end
        if (Math.Abs(start - end) < float.Epsilon) return 0f;

        // Calculate the percentage
        var percentage = (value - start) / (end - start);

        // Clamp between 0 and 1
        return Math.Max(0f, Math.Min(1f, percentage));
    }
}