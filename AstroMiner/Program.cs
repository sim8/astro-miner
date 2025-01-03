using System;
using AstroMiner;
using AstroMiner.procGenViewer;

if (Environment.GetEnvironmentVariable("PROC_GEN_VIEWER") == "true")
{
    using var game = new ProcGenViewerGame();
    game.Run();
}
else
{
    using var game = new AstroMinerGame();
    game.Run();
}