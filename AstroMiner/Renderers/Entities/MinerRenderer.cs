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
        var positionComponent = shared.GameState.Ecs.GetComponent<PositionComponent>(entityId);
        var directionComponent = shared.GameState.Ecs.GetComponent<DirectionComponent>(entityId);

        if (positionComponent == null)
            return;

        RenderGrapple(spriteBatch, entityId);

        var sourceRectangle = new Rectangle(
            directionComponent.Direction is Direction.Bottom or Direction.Left
                ? 0
                : MinerTextureSize,
            directionComponent.Direction is Direction.Top or Direction.Left
                ? 0
                : MinerTextureSize,
            MinerTextureSize,
            MinerTextureSize);

        var destinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(positionComponent.Position,
            MinerTextureSize, MinerTextureSize, MinerBoxOffsetX, MinerBoxOffsetY);

        var tintColor = ViewHelpers.GetEntityTintColor(shared.GameState.Ecs, entityId);

        spriteBatch.Draw(GetTracksTexture(positionComponent.Position, directionComponent.Direction, entityId),
            destinationRectangle, sourceRectangle, tintColor);
        spriteBatch.Draw(shared.Textures["miner-no-tracks"], destinationRectangle, sourceRectangle, tintColor);
    }

    private Texture2D GetTracksTexture(Vector2 position, Direction direction, int entityId)
    {
        // Movement removed when off asteroid
        var movementComponent = shared.GameState.Ecs.GetComponent<MovementComponent>(entityId);
        if (movementComponent == null) return shared.Textures["tracks-1"];

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

    private void RenderGrapple(SpriteBatch spriteBatch, int entityId)
    {
        var grappleComponent = shared.GameState.Ecs.GetComponent<GrappleComponent>(entityId);
        var movementComponent = shared.GameState.Ecs.GetComponent<MovementComponent>(entityId);
        var positionComponent = shared.GameState.Ecs.GetComponent<PositionComponent>(entityId);
        var directionComponent = shared.GameState.Ecs.GetComponent<DirectionComponent>(entityId);

        if (grappleComponent == null || movementComponent == null || positionComponent == null ||
            !grappleComponent.GrappleTarget.HasValue)
            return;

        var frontPosition = shared.GameState.Ecs.MovementSystem.GetFrontPosition(entityId);
        var grappleVisibleGridLength = shared.GameState.Ecs.GrappleSystem.GetDistanceToTarget(grappleComponent) *
                                       grappleComponent.GrapplePercentToTarget;
        var grappleVisibleGridWidth = 0.03f;

        // Calculate perpendicular offset for the two grapples
        var perpendicularDir = directionComponent.Direction switch
        {
            Direction.Top or Direction.Bottom => new Vector2(1, 0),
            Direction.Left or Direction.Right => new Vector2(0, 1),
            _ => throw new ArgumentOutOfRangeException()
        };
        var leftGrappleOffset = -perpendicularDir * (GrappleComponent.GrapplesWidth / 2);
        var rightGrappleOffset = perpendicularDir * (GrappleComponent.GrapplesWidth / 2);

        // Render left grapple
        var leftDestRect = directionComponent.Direction switch
        {
            Direction.Top => shared.ViewHelpers.GetVisibleRectForObject(
                frontPosition + leftGrappleOffset +
                new Vector2(-(grappleVisibleGridWidth / 2), -grappleVisibleGridLength),
                grappleVisibleGridWidth,
                grappleVisibleGridLength),
            Direction.Right => shared.ViewHelpers.GetVisibleRectForObject(
                frontPosition + leftGrappleOffset + new Vector2(0, -(grappleVisibleGridWidth / 2)),
                grappleVisibleGridLength,
                grappleVisibleGridWidth),
            Direction.Bottom => shared.ViewHelpers.GetVisibleRectForObject(
                frontPosition + leftGrappleOffset + new Vector2(-(grappleVisibleGridWidth / 2), 0),
                grappleVisibleGridWidth,
                grappleVisibleGridLength),
            Direction.Left => shared.ViewHelpers.GetVisibleRectForObject(
                frontPosition + leftGrappleOffset +
                new Vector2(-grappleVisibleGridLength, -(grappleVisibleGridWidth / 2)),
                grappleVisibleGridLength,
                grappleVisibleGridWidth),
            _ => throw new ArgumentOutOfRangeException()
        };
        spriteBatch.Draw(shared.Textures["white"], leftDestRect, Color.Black);

        // Render right grapple
        var rightDestRect = directionComponent.Direction switch
        {
            Direction.Top => shared.ViewHelpers.GetVisibleRectForObject(
                frontPosition + rightGrappleOffset +
                new Vector2(-(grappleVisibleGridWidth / 2), -grappleVisibleGridLength),
                grappleVisibleGridWidth,
                grappleVisibleGridLength),
            Direction.Right => shared.ViewHelpers.GetVisibleRectForObject(
                frontPosition + rightGrappleOffset + new Vector2(0, -(grappleVisibleGridWidth / 2)),
                grappleVisibleGridLength,
                grappleVisibleGridWidth),
            Direction.Bottom => shared.ViewHelpers.GetVisibleRectForObject(
                frontPosition + rightGrappleOffset + new Vector2(-(grappleVisibleGridWidth / 2), 0),
                grappleVisibleGridWidth,
                grappleVisibleGridLength),
            Direction.Left => shared.ViewHelpers.GetVisibleRectForObject(
                frontPosition + rightGrappleOffset +
                new Vector2(-grappleVisibleGridLength, -(grappleVisibleGridWidth / 2)),
                grappleVisibleGridLength,
                grappleVisibleGridWidth),
            _ => throw new ArgumentOutOfRangeException()
        };
        spriteBatch.Draw(shared.Textures["white"], rightDestRect, Color.Black);
    }
}