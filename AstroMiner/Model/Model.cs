using System;
using AstroMiner.Definitions;

namespace AstroMiner.Model;

[Serializable]
public class Model
{
    public World ActiveWorld { get; set; }
    public long TotalPlaytimeMs { get; set; }
}