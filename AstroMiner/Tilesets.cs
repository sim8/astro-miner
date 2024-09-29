using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AstroMiner;

[Flags]
internal enum Neighbours
{
    None = 0,
    Above = 1 << 0, // 1
    AboveRight = 1 << 1, // 2
    Right = 1 << 2, // 4
    BelowRight = 1 << 3, // 8
    Below = 1 << 4, // 16
    BelowLeft = 1 << 5, // 32
    Left = 1 << 6, // 64
    AboveLeft = 1 << 7 // 128
}

public static class Tilesets
{
    private static readonly (int, int)[] Offsets;
    public static readonly Dictionary<CellState, string> TilesetTextureNames;

    static Tilesets()
    {
        // Initialize the offsets for each neighbour, indexed by bit position (0-7)
        Offsets = new (int, int)[8]
        {
            (0, -1), // Above (bit position 0)
            (1, -1), // AboveRight (bit position 1)
            (1, 0), // Right (bit position 2)
            (1, 1), // BelowRight (bit position 3)
            (0, 1), // Below (bit position 4)
            (-1, 1), // BelowLeft (bit position 5)
            (-1, 0), // Left (bit position 6)
            (-1, -1) // AboveLeft (bit position 7)
        };

        TilesetTextureNames = new Dictionary<CellState, string>
        {
            { CellState.Rock, "rock-tileset" },
            { CellState.SolidRock, "solid-rock-tileset" },
            { CellState.Diamond, "diamond-tileset" },
            { CellState.Ruby, "ruby-tileset" }
        };
    }

    public static (int, int) GetTileCoords(GameState state, int col, int row)
    {
        // Dark
        if (AllFilledExcept(state, col, row))
            return (1, 2);

        // Concave corners
        if (AllFilledExcept(state, col, row, Neighbours.AboveRight))
            return (2, 2);
        if (AllFilledExcept(state, col, row, Neighbours.BelowRight))
            return (2, 0);
        if (AllFilledExcept(state, col, row, Neighbours.BelowLeft))
            return (4, 0);
        if (AllFilledExcept(state, col, row, Neighbours.AboveLeft))
            return (4, 2);

        // Double concave corners
        if (AllFilledExcept(state, col, row, Neighbours.AboveRight | Neighbours.BelowLeft))
            return (5, 0);
        if (AllFilledExcept(state, col, row, Neighbours.AboveLeft | Neighbours.BelowRight))
            return (5, 1);

        // Flat edges
        if (AllFilledExcept(state, col, row, Neighbours.None,
                Neighbours.AboveLeft | Neighbours.Above | Neighbours.AboveRight))
            return (3, 2);
        if (AllFilledExcept(state, col, row, Neighbours.None,
                Neighbours.AboveRight | Neighbours.Right | Neighbours.BelowRight))
            return (2, 1);
        if (AllFilledExcept(state, col, row, Neighbours.None,
                Neighbours.BelowLeft | Neighbours.Below | Neighbours.BelowRight))
            return (3, 0);
        if (AllFilledExcept(state, col, row, Neighbours.None,
                Neighbours.AboveLeft | Neighbours.Left | Neighbours.BelowLeft))
            return (4, 1);

        // Convex corners
        if (AllFilledExcept(state, col, row, Neighbours.None,
                Neighbours.AboveLeft | Neighbours.Above | Neighbours.AboveRight | Neighbours.Right |
                Neighbours.BelowRight))
            return (1, 0);
        if (AllFilledExcept(state, col, row, Neighbours.Right | Neighbours.Below,
                Neighbours.AboveRight | Neighbours.Right | Neighbours.BelowRight | Neighbours.Below |
                Neighbours.BelowLeft))
            return (1, 1);
        if (AllFilledExcept(state, col, row, Neighbours.Below | Neighbours.Left,
                Neighbours.BelowRight | Neighbours.Below | Neighbours.BelowLeft | Neighbours.Left |
                Neighbours.AboveLeft))
            return (0, 1);
        if (AllFilledExcept(state, col, row, Neighbours.Left | Neighbours.Above,
                Neighbours.BelowLeft | Neighbours.Left | Neighbours.AboveLeft | Neighbours.Above |
                Neighbours.AboveRight))
            return (0, 0);

        // Island
        return (0, 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AllFilledExcept(
        GameState state,
        int col,
        int row,
        Neighbours shouldBeEmptyNeighbours = Neighbours.None,
        Neighbours disregardNeighbours = Neighbours.None)
    {
        for (var i = 0; i < 8; i++)
        {
            var neighbour = (Neighbours)(1 << i);

            if ((disregardNeighbours & neighbour) != Neighbours.None)
                continue;

            var offset = Offsets[i];
            var cellState = state.Grid.GetCellState(col + offset.Item1, row + offset.Item2);
            var isFilled = cellState != CellState.Empty && cellState != CellState.Floor;
            var shouldBeFilled = (shouldBeEmptyNeighbours & neighbour) == Neighbours.None;

            if (isFilled != shouldBeFilled)
                return false;
        }

        return true;
    }
}