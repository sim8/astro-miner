using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS;

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
    public Dictionary<Type, HashSet<Component>> ComponentsByType { get; set; }
    public Dictionary<int, HashSet<Component>> EntityComponents { get; set; }
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
                MinerEntityId = null,
                ComponentsByType = new Dictionary<Type, HashSet<Component>>(),
                EntityComponents = new Dictionary<int, HashSet<Component>>()
            }
        };
    }
}