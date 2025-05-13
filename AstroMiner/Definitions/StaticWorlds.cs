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
                // ---------------------------------------------------0,1,2,3,4,5,6,7
                World.Krevik, new StaticWorldConfig("white", 8, 7, """
                                                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                                                      X,-,-,X,X,X,X,X,X,X,X,X,X,X
                                                                      X,-,-,X,X,X,X,X,X,X,X,X,X,X
                                                                      X,-,-,-,-,-,-,-,-,-,-,-,-,X
                                                                      X,-,-,-,-,-,-,-,-,-,-,-,-,X
                                                                      X,-,-,-,-,-,-,-,-,-,-,-,-,X
                                                                      X,X,X,X,X,X,X,X,X,X,X,-,X,X
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
                World.Ship, new StaticWorldConfig("white", 8, 9, """
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

    public static PortalConfig GetPortalConfig(World world, (int, int) coordinates)
    {
        return PortalsConfig.GetValueOrDefault((world, coordinates));
    }
}