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
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,-,-,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,-,-,-,-,X,X,X,-,-,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,-,X,X,X,X,-,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,-,-,X,X,X,X,-,X,X,X,X,-,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X,-,-,X,X,X,X,X,X,X,X
                                      X,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X,-,-,X,X,X,X,X,X,X,X
                                      X,-,-,X,X,X,X,X,X,X,X,X,X,-,-,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,@,X,-,-,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
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

    private static readonly IReadOnlyDictionary<(World world, (int x, int y)), PortalConfig> PortalsConfig =
        new Dictionary<(World world, (int x, int y)), PortalConfig>
        {
            { (World.Home, (11, 10)), new PortalConfig(World.RigRoom, (4, 7), Direction.Top) },
            { (World.RigRoom, (4, 7)), new PortalConfig(World.Home, (11, 10), Direction.Bottom) }
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

    public static WorldCellType[,] GetOizusGrid()
    {
        return ParseWorld(OizusWorld);
    }

    public static WorldCellType[,] GetRigRoomGrid()
    {
        return ParseWorld(RigRoomWorld);
    }

    public static PortalConfig GetPortalConfig(World world, (int, int) coordinates)
    {
        return PortalsConfig.GetValueOrDefault((world, coordinates));
    }
}