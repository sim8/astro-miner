using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.AsteroidWorld;

public class PlayerRenderer(
    RendererShared shared)
{
    private const int PlayerBoxOffsetX = -16;
    private const int PlayerBoxOffsetY = -22;
    private const int PlayerTextureSizePx = 50;

    public void RenderPlayer(SpriteBatch spriteBatch, int entityId)
    {
        var positionComponent = shared.GameState.EcsWorld.GetComponent<PositionComponent>(entityId);
        var movementComponent = shared.GameState.EcsWorld.GetComponent<MovementComponent>(entityId);
        
        if (positionComponent == null || movementComponent == null)
            return;

        var textureFrameOffsetX = movementComponent.Direction switch
        {
            Direction.Bottom => 0,
            Direction.Left => PlayerTextureSizePx * 1,
            Direction.Top => PlayerTextureSizePx * 2,
            Direction.Right => PlayerTextureSizePx * 3,
            _ => 0
        };
        var sourceRectangle = new Rectangle(
            textureFrameOffsetX, 0,
            PlayerTextureSizePx,
            PlayerTextureSizePx);

        var destinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(positionComponent.Position,
            PlayerTextureSizePx, PlayerTextureSizePx, PlayerBoxOffsetX, PlayerBoxOffsetY);

        // TODO: Add health component to handle tint color
        var tintColor = Color.Green;

        spriteBatch.Draw(shared.Textures["player"], destinationRectangle, sourceRectangle, tintColor);
    }
}