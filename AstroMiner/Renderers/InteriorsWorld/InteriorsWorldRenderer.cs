using System.Collections.Generic;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.InteriorsWorld;

public class InteriorsRenderConfig(string texureName, int gridWidth, int gridHeight)
{
    public string TexureName { get; } = texureName;
    public int GridWidth { get; } = gridWidth;
    public int GridHeight { get; } = gridHeight;
}

public class InteriorsWorldRenderer(RendererShared shared) : BaseWorldRenderer(shared)
{
    private static readonly IReadOnlyDictionary<World, InteriorsRenderConfig> InteriorsRenderConfigs =
        new Dictionary<World, InteriorsRenderConfig>
        {
            { World.RigRoom, new InteriorsRenderConfig("rig-room", 8, 9) },
            { World.MinEx, new InteriorsRenderConfig("min-ex", 8, 9) }
        };

    private readonly GameStateManager _gameStateManager = shared.GameStateManager;

    public override void RenderWorld(SpriteBatch spriteBatch)
    {
        if (InteriorsRenderConfigs.TryGetValue(shared.Game.Model.ActiveWorld, out var config))
            spriteBatch.Draw(Shared.Textures[config.TexureName],
                Shared.ViewHelpers.GetVisibleRectForGridCell(0, 0, config.GridWidth, config.GridHeight),
                Color.White);
        if (shared.Game.Debug.showGridDebug) RenderGridDebugOverlay(spriteBatch);
    }

    private void RenderGridDebugOverlay(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < _gameStateManager.InteriorsWorld.Grid.GetLength(0); row++)
            for (var col = 0; col < _gameStateManager.InteriorsWorld.Grid.GetLength(1); col++)
            {
                var cellRect = Shared.ViewHelpers.GetVisibleRectForGridCell(col, row);
                if (_gameStateManager.InteriorsWorld.Grid[row, col] == WorldCellType.Filled)
                    spriteBatch.Draw(Shared.Textures["white"], cellRect, Color.Red * 0.5f);
                if (_gameStateManager.InteriorsWorld.Grid[row, col] == WorldCellType.Portal)
                    spriteBatch.Draw(Shared.Textures["white"], cellRect, Color.Green * 0.5f);

                var coordinatesStr = col + " " + row;
                shared.RenderString(spriteBatch, cellRect.X, cellRect.Y, coordinatesStr, 2);
            }
    }
}