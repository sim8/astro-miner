namespace AstroMiner.Definitions;

public enum WorldCellType
{
    Empty,
    Filled
}

public static class WorldGrid
{
    private const string OizusWorld = """
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,-,-,X,X,X,X,X,X,X,X,X,X,X,X
                                      X,X,X,X,-,-,-,-,X,X,X,-,-,X,X,X,X,X
                                      X,X,X,X,X,X,X,-,X,X,X,X,-,X,X,X,X,X
                                      X,-,-,X,X,X,X,-,X,X,X,X,-,X,X,X,X,X
                                      X,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X,X
                                      X,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X,X
                                      X,-,-,X,X,X,X,X,X,X,X,X,X,-,-,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,-,X,-,-,X,X,X
                                      X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,-,-,-,-,X
                                      X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X,X
                                      """;

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
                grid[row, col] = cells[col] == 'X' ? WorldCellType.Filled : WorldCellType.Empty;
        }

        return grid;
    }

    public static WorldCellType[,] GetOizusGrid()
    {
        return ParseWorld(OizusWorld);
    }
}