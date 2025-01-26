using System;
using AstroMiner.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.Asteroid;

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

    private (bool shouldShow, int index) GetSparksIndex(DynamiteEntity dynamiteEntity)
    {
        var sparksIndexFloat = dynamiteEntity.FusePercentLeft * SparksSpeed % SparksFrames;
        var sparksIndex = (int)Math.Floor(sparksIndexFloat);
        var decimalValue = sparksIndexFloat - (int)sparksIndexFloat;

        return (decimalValue > 0.5, sparksIndex);
    }

    public void RenderDynamite(SpriteBatch spriteBatch, DynamiteEntity dynamiteEntity)
    {
        if (dynamiteEntity.FusePercentLeft < 0) return;
        var sourceRectangle = new Rectangle(
            0, 0,
            6,
            11);
        var destinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(dynamiteEntity.Position,
            6, 11, DynamiteBoxOffsetX, DynamiteBoxOffsetY);

        spriteBatch.Draw(shared.Textures["dynamite"], destinationRectangle, sourceRectangle, Color.White);

        var fuseLeftPx = (int)Math.Floor(dynamiteEntity.FusePercentLeft * FuseLengthPx);

        var (shouldShow, sparksIndex) = GetSparksIndex(dynamiteEntity);

        if (shouldShow)
        {
            var sparksSourceRectangle = new Rectangle(
                7, SparksBorderSize + (SparksBoxSize + SparksBorderSize) * sparksIndex,
                SparksBoxSize,
                SparksBoxSize);
            var sparksDestinationRectangle = shared.ViewHelpers.GetVisibleRectForObject(dynamiteEntity.Position,
                SparksBoxSize, SparksBoxSize, 0, -(5 + fuseLeftPx));

            spriteBatch.Draw(shared.Textures["dynamite"], sparksDestinationRectangle, sparksSourceRectangle,
                Color.White);
        }
    }

    public void RenderLightSource(SpriteBatch spriteBatch, DynamiteEntity dynamiteEntity)
    {
        var (shouldShow, _) = GetSparksIndex(dynamiteEntity);

        var lightSourcePos = dynamiteEntity.CenterPosition;
        shared.RenderRadialLightSource(spriteBatch, lightSourcePos, 128, shouldShow ? 0.3f : 0.1f);
    }
}