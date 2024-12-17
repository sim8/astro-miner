namespace AstroMiner;

// Enum for corners (mapping bits for clarity)
public enum Corner
{
    TopLeft = 3, // Bit 3
    TopRight = 2, // Bit 2
    BottomLeft = 1, // Bit 1
    BottomRight = 0 // Bit 0
}

public static class GradientKeyHelpers
{
    public const int InitialKey = 0;


    public static int CreateKey(params Corner[] corners)
    {
        var key = InitialKey;
        foreach (var corner in corners) key |= 1 << (int)corner;
        return key;
    }

    public static int SetCorner(int key, Corner corner, bool value)
    {
        if (value)
            // Set the corner to true
            return key | (1 << (int)corner);

        // Set the corner to false
        return key & ~(1 << (int)corner);
    }

    public static int SetCorners(int key, Corner corner1, Corner corner2, bool value)
    {
        if (value)
            // Set both corners to true
            return key | (1 << (int)corner1) | (1 << (int)corner2);

        // Set both corners to false
        return key & ~(1 << (int)corner1) & ~(1 << (int)corner2);
    }
}