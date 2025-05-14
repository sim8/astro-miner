#nullable enable
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

public class StaticWorldConfig(string texureName, int gridWidth, int gridHeight, string worldStr)
{
    public string TexureName { get; } = texureName;
    public int GridWidth { get; } = gridWidth;
    public int GridHeight { get; } = gridHeight;
    public WorldCellType[,] World { get; } = StaticWorlds.ParseWorld(worldStr);
}

public static class StaticWorlds
{
    public static readonly IReadOnlyDictionary<World, StaticWorldConfig> StaticWorldConfigs =
        new Dictionary<World, StaticWorldConfig>
        {
            {
                // ----------------------------------------------------0,1,2,3,4,5,6,7
                World.RigRoom, new StaticWorldConfig("rig-room", 8, 9, """
                                                                       X,X,X,X,X,X,X,X
                                                                       X,X,X,X,X,X,X,X
                                                                       X,X,X,X,X,X,X,X
                                                                       X,-,-,-,-,-,-,X
                                                                       X,-,-,-,-,-,-,X
                                                                       X,-,-,-,-,-,-,X
                                                                       X,-,-,-,-,-,-,X
                                                                       X,X,X,X,@,X,X,X
                                                                       X,X,X,X,X,X,X,X
                                                                       """)
            },
            {
                // --------------------------------------------------------0,1,2,3,4,5,6,7,8,9,0,1,2,3
                World.Krevik, new StaticWorldConfig("krevik-docks", 14, 8, """
                                                                           X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                                                           X,-,-,X,X,X,X,X,X,X,X,X,X,X
                                                                           X,-,-,X,X,X,X,X,X,X,X,X,X,X
                                                                           X,-,-,-,-,-,-,-,-,-,-,-,-,X
                                                                           X,-,-,-,-,-,-,-,-,-,-,-,-,X
                                                                           X,-,-,-,-,-,-,-,-,-,-,-,-,X
                                                                           X,X,X,X,X,X,X,X,X,X,X,@,X,X
                                                                           X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                                                           """)
            },
            {
                // ------------------------------------------------0,1,2,3,4,5,6,7
                World.MinEx, new StaticWorldConfig("min-ex", 8, 9, """
                                                                   X,X,X,X,X,X,X,X
                                                                   X,X,X,X,X,X,X,X
                                                                   X,X,X,X,X,X,X,X
                                                                   X,-,-,-,-,-,-,X
                                                                   X,-,-,-,-,-,-,X
                                                                   X,-,-,-,-,-,-,X
                                                                   X,-,-,-,-,-,-,X
                                                                   X,X,X,@,X,X,X,X
                                                                   X,X,X,X,X,X,X,X
                                                                   """)
            },

            {
                // -0-------------------1-------------------2--------------
                // -0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7
                World.Home, new StaticWorldConfig("oizus-bg", Coordinates.Grid.OizusWidth, Coordinates.Grid.OizusHeight,
                    """
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
                    """)
            },
            {
                // -----------------------------------------------0-------------------1-------------------2------
                // -----------------------------------------------0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3
                World.Ship, new StaticWorldConfig("ship", 24, 18, """
                                                                  X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                                                  X,-,-,-,-,X,-,-,-,-,X,-,-,-,-,X,X,@,X,X,X,X,X,X
                                                                  X,-,-,-,-,X,-,-,-,-,X,-,-,-,-,X,-,-,-,X,X,X,X,X
                                                                  X,X,X,X,-,X,X,X,X,-,X,X,X,X,-,X,-,-,-,X,X,X,X,X
                                                                  X,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X,X,X
                                                                  X,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X,X,X
                                                                  X,-,-,-,-,-,X,X,X,X,X,X,X,X,X,X,-,-,-,X,-,-,-,X
                                                                  X,-,-,-,-,-,X,-,-,-,-,-,-,-,-,-,-,-,-,X,-,-,-,X
                                                                  X,-,-,-,-,-,X,-,-,-,-,-,-,-,-,-,-,-,-,X,-,-,-,X
                                                                  X,-,-,-,-,-,X,-,-,-,-,-,-,-,-,-,-,-,-,X,-,-,-,X
                                                                  X,-,-,-,-,-,X,-,-,-,-,-,-,-,-,-,-,-,-,X,-,-,-,X
                                                                  X,-,-,-,-,-,X,X,X,X,X,X,X,X,X,X,-,-,-,X,-,-,-,X
                                                                  X,-,-,-,-,-,X,-,-,-,-,-,-,-,-,-,-,-,-,X,X,X,X,X
                                                                  X,X,X,X,X,-,X,-,-,-,-,-,-,-,-,-,-,-,-,X,X,X,X,X
                                                                  X,-,-,-,-,-,X,-,-,-,X,-,-,-,-,-,-,-,-,X,X,X,X,X
                                                                  X,-,-,-,-,-,X,-,-,-,X,-,-,-,-,-,-,-,-,X,X,X,X,X
                                                                  X,-,-,-,-,-,X,-,-,-,X,-,-,-,-,-,-,-,-,X,X,X,X,X
                                                                  X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                                                  """)
            }
        };

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
            },
            {
                (World.Krevik, Coordinates.Grid.KrevikToShipPortal),
                new PortalConfig(World.Ship, Coordinates.Grid.ShipToKrevikPortal, Direction.Bottom)
            },
            {
                (World.Ship, Coordinates.Grid.ShipToKrevikPortal),
                new PortalConfig(World.Krevik, Coordinates.Grid.KrevikToShipPortal, Direction.Top)
            }
        };

    public static WorldCellType[,] ParseWorld(string world)
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

    public static PortalConfig? GetPortalConfig(World world, (int, int) coordinates, bool allowNull = false)
    {
        if (!PortalsConfig.TryGetValue((world, coordinates), out var config) && !allowNull)
            throw new ArgumentException("No portal config found for world " + world + " at coordinates " + coordinates);

        return config;
    }
}