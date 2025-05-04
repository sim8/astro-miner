using System;
using System.Collections.Generic;

namespace AstroMiner.Definitions;

public enum WorldCellType
{
    Empty,
    Filled,
    Portal
}

public class PortalConfig(World targetWorld, (int, int) coordinates, Direction direction)
{
    public World TargetWorld { get; } = targetWorld;
    public (int, int) Coordinates { get; } = coordinates;
    public Direction Direction { get; } = direction;
}

public static class WorldGrid
{
    // -------------------------------0-------------------1-------------------2------------
    // -------------------------------0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6
    private const string OizusWorld = """
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,-,-,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,-,-,-,-,X,X,X,X,-,-,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,-,X,X,X,X,X,-,X,X,X,X,-,-,X,X,X,X,X,X,X,X
                                      X,-,-,X,X,X,X,-,X,X,X,X,X,-,X,X,X,X,-,-,X,X,X,X,X,X,X,X
                                      X,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X,-,-,X,X,X,X,X,X,X,X
                                      X,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X,-,-,X,@,X,X,X,X,X,X
                                      X,-,-,X,X,X,X,X,X,X,X,X,X,X,-,-,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,@,X,-,-,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,X,X,X,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,X,X,X,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,X,X,X,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      """;

    // ---------------------------------0,1,2,3,4,5,6,7
    private const string RigRoomWorld = """
                                        X,X,X,X,X,X,X,X
                                        X,X,X,X,X,X,X,X
                                        X,X,X,X,X,X,X,X
                                        X,-,-,-,-,-,-,X
                                        X,-,-,-,-,-,-,X
                                        X,-,-,-,-,-,-,X
                                        X,-,-,-,-,-,-,X
                                        X,X,X,X,@,X,X,X
                                        X,X,X,X,X,X,X,X
                                        """;

    private const string MinExWorld = """
                                        X,X,X,X,X,X,X,X
                                        X,X,X,X,X,X,X,X
                                        X,X,X,X,X,X,X,X
                                        X,-,-,-,-,-,-,X
                                        X,-,-,-,-,-,-,X
                                        X,-,-,-,-,-,-,X
                                        X,-,-,-,-,-,-,X
                                        X,X,X,@,X,X,X,X
                                        X,X,X,X,X,X,X,X
                                        """;

    private static readonly IReadOnlyDictionary<(World world, (int x, int y)), PortalConfig> PortalsConfig =
        new Dictionary<(World world, (int x, int y)), PortalConfig>
        {
            {
                (World.Home, Coordinates.Grid.HomeToRigRoomPortal),
                new PortalConfig(World.RigRoom, Coordinates.Grid.RigToomToHomePortal, Direction.Top)
            },
            {
                (World.RigRoom, Coordinates.Grid.RigToomToHomePortal),
                new PortalConfig(World.Home, Coordinates.Grid.HomeToRigRoomPortal, Direction.Bottom)
            },
            {
                (World.Home, Coordinates.Grid.HomeToMinExPortal),
                new PortalConfig(World.MinEx, Coordinates.Grid.MinExToHomePortal, Direction.Top)
            },
            {
                (World.MinEx, Coordinates.Grid.MinExToHomePortal),
                new PortalConfig(World.Home, Coordinates.Grid.HomeToMinExPortal, Direction.Bottom)
            }
        };

    private static WorldCellType[,] ParseWorld(string world)
    {
        var rows = world.Trim().Split('\n');

        var rowCount = rows.Length;
        var colCount = rows[0].Replace(",", "").Length;

        var grid = new WorldCellType[rowCount, colCount];

        for (var row = 0; row < rowCount; row++)
        {
            var cells = rows[row].Replace(",", "").Trim().ToCharArray();

            for (var col = 0; col < colCount; col++)
                grid[row, col] = cells[col] switch
                {
                    'X' => WorldCellType.Filled,
                    '-' => WorldCellType.Empty,
                    '@' => WorldCellType.Portal,
                    _ => throw new ArgumentOutOfRangeException()
                };
        }

        return grid;
    }

    public static WorldCellType[,] GetWorldGrid(World world)
    {
        return world switch
        {
            World.Home => ParseWorld(OizusWorld),
            World.RigRoom => ParseWorld(RigRoomWorld),
            World.MinEx => ParseWorld(MinExWorld),
            _ => throw new ArgumentOutOfRangeException(nameof(world), world, null)
        };
    }

    public static PortalConfig GetPortalConfig(World world, (int, int) coordinates)
    {
        return PortalsConfig.GetValueOrDefault((world, coordinates));
    }
}