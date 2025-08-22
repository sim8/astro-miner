using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.AsteroidWorld;

public class SlidingDoorRenderer(
    RendererShared shared)
{
    private const int DoorWidthPx = 32;
    private const int DoorHeightPx = 64;

    public void RenderSlidingDoors(SpriteBatch spriteBatch, int entityId)
    {
        var slidingDoorComponent = shared.GameStateManager.Ecs.GetComponent<SlidingDoorComponent>(entityId);
        var positionComponent = shared.GameStateManager.Ecs.GetComponent<PositionComponent>(entityId);

        if (slidingDoorComponent == null || positionComponent == null)
            return;

        var visiblePx = (int)((1f - slidingDoorComponent.OpenPercent) * DoorWidthPx);
        var yRenderPos = positionComponent.Position.Y + positionComponent.GridHeight / 2 -
                         ViewHelpers.ConvertTexturePxToGridUnits(DoorHeightPx);

        // Left door
        var leftSourceRect = new Rectangle(
            DoorWidthPx - visiblePx, 0,
            visiblePx,
            DoorHeightPx);

        var leftRenderPos = new Vector2(positionComponent.Position.X, yRenderPos);
        var leftDestRect = shared.ViewHelpers.GetVisibleRectForObject(leftRenderPos,
            visiblePx, DoorHeightPx);

        spriteBatch.Draw(shared.Textures["door"], leftDestRect, leftSourceRect, Color.White);

        // Right door
        var rightSourceRect = new Rectangle(
            DoorWidthPx, 0,
            visiblePx,
            DoorHeightPx);
        var rightRenderPos =
            new Vector2(positionComponent.Position.X + ViewHelpers.ConvertTexturePxToGridUnits(DoorWidthPx - visiblePx),
                yRenderPos);
        var rightDestRect = shared.ViewHelpers.GetVisibleRectForObject(rightRenderPos,
            visiblePx, DoorHeightPx);

        spriteBatch.Draw(shared.Textures["door"], rightDestRect, rightSourceRect, Color.White);

        // // Right door (which visually overlays left)
        // var rightSourceRect = new Rectangle(
        //     DoorWidthPx, 0,
        //     DoorWidthPx + visiblePx,
        //     DoorHeightPx);
        // var pos = new Vector2(positionComponent.Position.X + visiblePx, positionComponent.Position.Y);
        // var rightDestRect = shared.ViewHelpers.GetVisibleRectForObject(positionComponent.Position,
        //     DoorWidthPx, DoorWidthPx);

        // spriteBatch.Draw(shared.Textures["door"], rightSourceRect, rightDestRect, Color.White);
    }
}