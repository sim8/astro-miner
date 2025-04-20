using System;
using AstroMiner.Definitions;

namespace AstroMiner.Model;

[Serializable]
public class GameModel
{
    public World ActiveWorld { get; set; }
    public long TotalPlaytimeMs { get; set; }
}