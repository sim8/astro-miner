using System;
using System.Collections.Generic;
using System.Linq;

namespace AstroMiner;

internal enum Neighbours
{
    Above,
    AboveRight,
    Right,
    BelowRight,
    Below,
    BelowLeft,
    Left,
    AboveLeft
}

public static class Tilesets
{
    private static readonly Dictionary<Neighbours, (int, int)> Offsets;
    private static readonly Neighbours[] AllNeighbours;
    public static readonly Dictionary<CellState, string> TilesetTextureNames;

    static Tilesets()
    {
        Offsets = new Dictionary<Neighbours, (int, int)>
        {
            { Neighbours.Above, (0, -1) },
            { Neighbours.AboveRight, (1, -1) },
            { Neighbours.Right, (1, 0) },
            { Neighbours.BelowRight, (1, 1) },
            { Neighbours.Below, (0, 1) },
            { Neighbours.BelowLeft, (-1, 1) },
            { Neighbours.Left, (-1, 0) },
            { Neighbours.AboveLeft, (-1, -1) }
        };
        TilesetTextureNames = new Dictionary<CellState, string>
        {
            { CellState.Rock, "rock-tileset" },
            { CellState.SolidRock, "solid-rock-tileset" },
            { CellState.Diamond, "diamond-tileset" },
            { CellState.Ruby, "ruby-tileset" }
        };
        AllNeighbours = (Neighbours[])Enum.GetValues(typeof(Neighbours));
    }


    public static (int, int) GetTileCoords(MiningState state, int col, int row)
    {
        // Dark
        if (AllFilledExcept(state, col, row)) return (1, 2);

        // Concave
        if (AllFilledExcept(state, col, row, Neighbours.AboveRight))
            return (2, 2);
        if (AllFilledExcept(state, col, row, Neighbours.BelowRight))
            return (2, 0);
        if (AllFilledExcept(state, col, row, Neighbours.BelowLeft))
            return (4, 0);
        if (AllFilledExcept(state, col, row, Neighbours.AboveLeft))
            return (4, 2);

        // Double concave
        if (AllFilledExcept(state, col, row, Neighbours.AboveRight, Neighbours.BelowLeft))
            return (5, 0);
        if (AllFilledExcept(state, col, row, Neighbours.AboveLeft, Neighbours.BelowRight))
            return (5, 1);

        // Flat edges
        if (AllFilledExcept([Neighbours.AboveLeft, Neighbours.Above, Neighbours.AboveRight], state, col, row))
            return (3, 2);
        if (AllFilledExcept([Neighbours.AboveRight, Neighbours.Right, Neighbours.BelowRight], state, col, row))
            return (2, 1);
        if (AllFilledExcept([Neighbours.BelowLeft, Neighbours.Below, Neighbours.BelowRight], state, col, row))
            return (3, 0);
        if (AllFilledExcept([Neighbours.AboveLeft, Neighbours.Left, Neighbours.BelowLeft], state, col, row))
            return (4, 1);

        // Convex
        if (AllFilledExcept(
            [
                Neighbours.AboveLeft, Neighbours.Above, Neighbours.AboveRight, Neighbours.Right, Neighbours.BelowRight
            ], state, col, row))
            return (1, 0);
        if (AllFilledExcept(
                [
                    Neighbours.AboveRight, Neighbours.Right, Neighbours.BelowRight, Neighbours.Below,
                    Neighbours.BelowLeft
                ], state, col, row,
                Neighbours.Right, Neighbours.Below))
            return (1, 1);
        if (AllFilledExcept(
                [Neighbours.BelowRight, Neighbours.Below, Neighbours.BelowLeft, Neighbours.Left, Neighbours.AboveLeft],
                state, col, row,
                Neighbours.Below, Neighbours.Left))
            return (0, 1);
        if (AllFilledExcept(
                [Neighbours.BelowLeft, Neighbours.Left, Neighbours.AboveLeft, Neighbours.Above, Neighbours.AboveRight],
                state, col, row,
                Neighbours.Left, Neighbours.Above))
            return (0, 0);

        // Island
        return (0, 2);
    }


    private static bool AllFilledExcept(MiningState state, int col, int row,
        params Neighbours[] shouldBeEmptyNeighbours)
    {
        return AllFilledExcept([], state, col, row, shouldBeEmptyNeighbours);
    }

    private static bool AllFilledExcept(Neighbours[] disregardNeighbours, MiningState state, int col, int row,
        params Neighbours[] shouldBeEmptyNeighbours)
    {
        foreach (var neighbour in AllNeighbours)
        {
            if (disregardNeighbours.Contains(neighbour)) continue;

            var offset = Offsets[neighbour];
            var cellState = state.GetCellState(col + offset.Item1, row + offset.Item2);
            var isFilled = cellState != CellState.Empty && cellState != CellState.Floor;
            var shouldBeFilled = !shouldBeEmptyNeighbours.Contains(neighbour);

            if (isFilled != shouldBeFilled) return false;
        }

        return true;
    }
}