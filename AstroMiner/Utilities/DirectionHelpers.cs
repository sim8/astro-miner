using System;
using Microsoft.Xna.Framework;

namespace AstroMiner.Utilities;

public static class DirectionHelpers
{
    public static Vector2 GetDirectionalVector(float distance, Direction direction)
    {
        return direction switch
        {
            Direction.Top => new Vector2(0, -distance),
            Direction.Right => new Vector2(distance, 0),
            Direction.Bottom => new Vector2(0, distance),
            Direction.Left => new Vector2(-distance, 0),
            _ => Vector2.Zero
        };
    }

    public static Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Top => Direction.Bottom,
            Direction.Right => Direction.Left,
            Direction.Bottom => Direction.Top,
            Direction.Left => Direction.Right,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}