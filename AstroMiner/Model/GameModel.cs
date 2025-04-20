using System;
using AstroMiner.Definitions;

namespace AstroMiner.Model;

[Serializable]
public class GameModel
{
    public World ActiveWorld { get; set; }
    public long TotalPlaytimeMs { get; set; }
    public EcsModel Ecs { get; set; }
}

[Serializable]
public class EcsModel
{
    public int? ActiveControllableEntityId { get; set; }
    public int NextEntityId { get; set; }
    public int? PlayerEntityId { get; set; }

    public int? MinerEntityId { get; set; }
}

public static class GameModelHelpers
{
    public static GameModel CreateNewGameModel()
    {
        return new GameModel
        {
            ActiveWorld = World.Home,
            TotalPlaytimeMs = 0,
            Ecs = new EcsModel
            {
                ActiveControllableEntityId = null,
                NextEntityId = 1,
                PlayerEntityId = null,
                MinerEntityId = null
            }
        };
    }
}