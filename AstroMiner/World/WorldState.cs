using AstroMiner.Definitions;
using AstroMiner.Entities;

namespace AstroMiner.World;

public class WorldState(GameState gameState)
{
    public WorldCellType[,] Grid;
    public PlayerEntity Player;

    public void Initialize()
    {
        Grid = WorldGrid.GetOizusGrid();
        Player = new PlayerEntity(gameState);
    }
}