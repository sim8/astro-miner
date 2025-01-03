namespace AstroMiner.Utilities;

public enum Corner
{
    TopLeft = 3, // Bit 3
    TopRight = 2, // Bit 2
    BottomLeft = 1, // Bit 1
    BottomRight = 0 // Bit 0
}

// Create or modify a bit-based key representing the up/down state of each corner of a cell
public static class RampKeys
{
    public static readonly int Empty = CreateKey();
    public static readonly int Right = CreateKey(Corner.TopRight, Corner.BottomRight);
    public static readonly int Down = CreateKey(Corner.BottomLeft, Corner.BottomRight);
    public static readonly int Left = CreateKey(Corner.TopLeft, Corner.BottomLeft);
    public static readonly int Up = CreateKey(Corner.TopRight, Corner.TopLeft);
    public static readonly int UpRight = CreateKey(Corner.TopRight);
    public static readonly int DownRight = CreateKey(Corner.BottomRight);
    public static readonly int UpLeft = CreateKey(Corner.TopLeft);
    public static readonly int DownLeft = CreateKey(Corner.BottomLeft);
    public static readonly int UpRightWide = CreateKey(Corner.TopRight, Corner.BottomRight, Corner.TopLeft);
    public static readonly int DownRightWide = CreateKey(Corner.TopRight, Corner.BottomRight, Corner.BottomLeft);
    public static readonly int UpLeftWide = CreateKey(Corner.TopRight, Corner.BottomLeft, Corner.TopLeft);
    public static readonly int DownLeftWide = CreateKey(Corner.TopLeft, Corner.BottomLeft, Corner.BottomRight);
    public static readonly int UpLeftToBottomRight = CreateKey(Corner.TopLeft, Corner.BottomRight);
    public static readonly int UpRightToBottomLeft = CreateKey(Corner.TopRight, Corner.BottomLeft);

    // TODO shouldn't happen in gradients
    public static readonly int
        Solid = CreateKey(Corner.TopRight, Corner.BottomLeft, Corner.BottomRight, Corner.TopLeft);

    public static int CreateKey(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
    {
        var key = Empty;
        if (topLeft) key |= 1 << (int)Corner.TopLeft;
        if (topRight) key |= 1 << (int)Corner.TopRight;
        if (bottomLeft) key |= 1 << (int)Corner.BottomLeft;
        if (bottomRight) key |= 1 << (int)Corner.BottomRight;
        return key;
    }


    public static int CreateKey(params Corner[] corners)
    {
        var key = Empty;
        foreach (var corner in corners) key |= 1 << (int)corner;
        return key;
    }
}