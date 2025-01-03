using System;
using AstroMiner;
using AstroMiner.ProceduralGenViewer;

if (Environment.GetEnvironmentVariable("PROC_GEN_VIEWER") == "true")
{
    using var game = new ProceduralGenViewerGame();
    game.Run();
}
else
{
    using var game = new AstroMinerGame();
    game.Run();
}