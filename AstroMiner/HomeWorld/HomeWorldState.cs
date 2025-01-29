using AstroMiner.Definitions;
using AstroMiner.Entities;
using Microsoft.Xna.Framework;

namespace AstroMiner.HomeWorld;

public class HomeWorldState(GameState gameState) : BaseWorldState(gameState)
{
    public WorldCellType[,] Grid;
    public PlayerEntity Player;

    public override void Initialize()
    {
        base.Initialize();
        Grid = WorldGrid.GetOizusGrid();
        Player = new PlayerEntity(gameState);
        Player.Position = new Vector2(1.5f, 1.5f);
        ActiveEntitiesSortedByDistance = [Player];
    }
}