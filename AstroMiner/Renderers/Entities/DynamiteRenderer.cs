using System;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.AsteroidWorld;

public class DynamiteRenderer(
    RendererShared shared)
{
    private const int DynamiteBoxOffsetX = -1;
    private const int DynamiteBoxOffsetY = -6;
    private const int SparksBoxSize = 4;
    private const int SparksBorderSize = 1;
    private const int SparksFrames = 4;
    private const int FuseLengthPx = 6;
    private const int SparksSpeed = 20;

    private (bool shouldShow, int index) GetSparksIndex(float fusePercentLeft)
    {
        var sparksIndexFloat = fusePercentLeft * SparksSpeed % SparksFrames;
        var sparksIndex = (int)Math.Floor(sparksIndexFloat);
        var decimalValue = sparksIndexFloat - (int)sparksIndexFloat;

        return (decimalValue > 0.5, sparksIndex);
    }

    public void RenderDynamite(SpriteBatch spriteBatch, int entityId)
    {
        var positionComponent = shared.GameStateManager.Ecs.GetComponent<PositionComponent>(entityId);
        var fuseComponent = shared.GameStateManager.Ecs.GetComponent<FuseComponent>(entityId);

        if (positionComponent == null || fuseComponent == null)
            return;

        if (fuseComponent.FusePercentLeft < 0) return;

        var sourceRectangle = new Rectangle(
            0, 0,
            6,
            11);
        var destinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(positionComponent.Position,
            6, 11, DynamiteBoxOffsetX, DynamiteBoxOffsetY);

        spriteBatch.Draw(shared.Textures[Tx.Dynamite], destinationRectangle, sourceRectangle, Color.White);

        var fuseLeftPx = (int)Math.Floor(fuseComponent.FusePercentLeft * FuseLengthPx);

        var (shouldShow, sparksIndex) = GetSparksIndex(fuseComponent.FusePercentLeft);

        if (shouldShow)
        {
            var sparksSourceRectangle = new Rectangle(
                7, SparksBorderSize + (SparksBoxSize + SparksBorderSize) * sparksIndex,
                SparksBoxSize,
                SparksBoxSize);
            var sparksDestinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(positionComponent.Position,
                SparksBoxSize, SparksBoxSize, 0, -(5 + fuseLeftPx));

            spriteBatch.Draw(shared.Textures[Tx.Dynamite], sparksDestinationRectangle, sparksSourceRectangle,
                Color.White);
        }
    }

    public void RenderLightSource(SpriteBatch spriteBatch, int entityId)
    {
        var positionComponent = shared.GameStateManager.Ecs.GetComponent<PositionComponent>(entityId);
        var fuseComponent = shared.GameStateManager.Ecs.GetComponent<FuseComponent>(entityId);

        if (positionComponent == null || fuseComponent == null)
            return;

        var (shouldShow, _) = GetSparksIndex(fuseComponent.FusePercentLeft);

        var lightSourcePos = positionComponent.CenterPosition;
        shared.RenderRadialLightSource(spriteBatch, lightSourcePos, 128, shouldShow ? 0.3f : 0.1f);
    }
}