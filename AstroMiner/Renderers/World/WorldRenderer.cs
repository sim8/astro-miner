using AstroMiner.Definitions;
using AstroMiner.Entities;
using AstroMiner.Renderers.Asteroid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.World;

public class WorldRenderer
{
    private readonly PlayerRenderer _playerRenderer;
    private readonly RendererShared _shared;

    public WorldRenderer(RendererShared shared)
    {
        _shared = shared;
        _playerRenderer = new PlayerRenderer(_shared);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        RenderScene(spriteBatch);
        spriteBatch.End();
    }

    public void RenderScene(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < _shared.GameState.World.Grid.GetLength(0); row++)
        for (var col = 0; col < _shared.GameState.World.Grid.GetLength(1); col++)
            if (_shared.GameState.World.Grid[row, col] == WorldCellType.Filled)
                spriteBatch.Draw(_shared.Textures["white"],
                    _shared.ViewHelpers.GetVisibleRectForGridCell(col, row),
                    Color.White);

        foreach (var entity in _shared.GameState.World.ActiveEntitiesSortedByDistance)
            if (entity is PlayerEntity)
                _playerRenderer.RenderPlayer(spriteBatch);
    }
}