using System;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.AsteroidWorld;

public class MinerRenderer(
    RendererShared shared)
{
    private const int MinerBoxOffsetX = -13;
    private const int MinerBoxOffsetY = -20;
    private const int MinerTextureSize = 64;

    public void RenderMiner(SpriteBatch spriteBatch, int entityId)
    {
        var positionComponent = shared.GameState.EcsWorld.GetComponent<PositionComponent>(entityId);
        var movementComponent = shared.GameState.EcsWorld.GetComponent<MovementComponent>(entityId);
        
        if (positionComponent == null || movementComponent == null)
            return;

        // TODO: Implement grapple rendering when we add grapple component
        // RenderGrapple(spriteBatch);
        
        var sourceRectangle = new Rectangle(
            movementComponent.Direction is Direction.Bottom or Direction.Left
                ? 0
                : MinerTextureSize,
            movementComponent.Direction is Direction.Top or Direction.Left
                ? 0
                : MinerTextureSize,
            MinerTextureSize,
            MinerTextureSize);
            
        var destinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(positionComponent.Position,
            MinerTextureSize, MinerTextureSize, MinerBoxOffsetX, MinerBoxOffsetY);

        // TODO: Add health component for proper tint color
        var tintColor = Color.White;

        spriteBatch.Draw(GetTracksTexture(positionComponent.Position, movementComponent.Direction), 
            destinationRectangle, sourceRectangle, tintColor);
        spriteBatch.Draw(shared.Textures["miner-no-tracks"], destinationRectangle, sourceRectangle, tintColor);
    }

    private Texture2D GetTracksTexture(Vector2 position, Direction direction)
    {
        var (gridX, gridY) = ViewHelpers.GridPosToTexturePx(position);
        switch (direction)
        {
            case Direction.Top: return shared.Textures["tracks-" + (2 - gridY % 3)];
            case Direction.Right: return shared.Textures["tracks-" + gridX % 3];
            case Direction.Bottom: return shared.Textures["tracks-" + gridY % 3];
            case Direction.Left: return shared.Textures["tracks-" + (2 - gridX % 3)];
        }

        return shared.Textures["tracks-1"];
    }

    // TODO: Re-implement when we add grapple component
    /*
    private void RenderGrapple(SpriteBatch spriteBatch)
    {
        if (Miner.GrappleTarget.HasValue)
        {
            var grappleVisibleGridLength = Miner.DistanceToTarget * Miner.GrapplePercentToTarget;
            var grappleVisibleGridWidth = 0.03f;

            // Calculate perpendicular offset for the two grapples
            var perpendicularDir = Miner.Direction switch
            {
                Direction.Top or Direction.Bottom => new Vector2(1, 0),
                Direction.Left or Direction.Right => new Vector2(0, 1),
                _ => throw new ArgumentOutOfRangeException()
            };
            var leftGrappleOffset = -perpendicularDir * (MinerEntity.GrapplesWidth / 2);
            var rightGrappleOffset = perpendicularDir * (MinerEntity.GrapplesWidth / 2);

            // Render left grapple
            var leftDestRect = Miner.Direction switch
            {
                Direction.Top => shared.ViewHelpers.GetVisibleRectForObject(
                    Miner.FrontPosition + leftGrappleOffset +
                    new Vector2(-(grappleVisibleGridWidth / 2), -grappleVisibleGridLength),
                    grappleVisibleGridWidth,
                    grappleVisibleGridLength),
                Direction.Right => shared.ViewHelpers.GetVisibleRectForObject(
                    Miner.FrontPosition + leftGrappleOffset + new Vector2(0, -(grappleVisibleGridWidth / 2)),
                    grappleVisibleGridLength,
                    grappleVisibleGridWidth),
                Direction.Bottom => shared.ViewHelpers.GetVisibleRectForObject(
                    Miner.FrontPosition + leftGrappleOffset + new Vector2(-(grappleVisibleGridWidth / 2), 0),
                    grappleVisibleGridWidth,
                    grappleVisibleGridLength),
                Direction.Left => shared.ViewHelpers.GetVisibleRectForObject(
                    Miner.FrontPosition + leftGrappleOffset +
                    new Vector2(-grappleVisibleGridLength, -(grappleVisibleGridWidth / 2)),
                    grappleVisibleGridLength,
                    grappleVisibleGridWidth),
                _ => throw new ArgumentOutOfRangeException()
            };
            spriteBatch.Draw(shared.Textures["white"], leftDestRect, Color.Black);

            // Render right grapple
            var rightDestRect = Miner.Direction switch
            {
                Direction.Top => shared.ViewHelpers.GetVisibleRectForObject(
                    Miner.FrontPosition + rightGrappleOffset +
                    new Vector2(-(grappleVisibleGridWidth / 2), -grappleVisibleGridLength),
                    grappleVisibleGridWidth,
                    grappleVisibleGridLength),
                Direction.Right => shared.ViewHelpers.GetVisibleRectForObject(
                    Miner.FrontPosition + rightGrappleOffset + new Vector2(0, -(grappleVisibleGridWidth / 2)),
                    grappleVisibleGridLength,
                    grappleVisibleGridWidth),
                Direction.Bottom => shared.ViewHelpers.GetVisibleRectForObject(
                    Miner.FrontPosition + rightGrappleOffset + new Vector2(-(grappleVisibleGridWidth / 2), 0),
                    grappleVisibleGridWidth,
                    grappleVisibleGridLength),
                Direction.Left => shared.ViewHelpers.GetVisibleRectForObject(
                    Miner.FrontPosition + rightGrappleOffset +
                    new Vector2(-grappleVisibleGridLength, -(grappleVisibleGridWidth / 2)),
                    grappleVisibleGridLength,
                    grappleVisibleGridWidth),
                _ => throw new ArgumentOutOfRangeException()
            };
            spriteBatch.Draw(shared.Textures["white"], rightDestRect, Color.Black);
        }
    }
    */
}