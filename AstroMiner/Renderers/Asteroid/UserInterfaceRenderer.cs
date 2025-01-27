using System;
using System.Linq;
using AstroMiner.Definitions;
using AstroMiner.Entities;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Renderers.Asteroid;

public class UserInterfaceRenderer(
    RendererShared shared)
{
    private readonly Color _gridColor = new(70, 125, 149);

    public void RenderUserInterface(SpriteBatch spriteBatch)
    {
        var timeLeft = Math.Max(GameConfig.AsteroidExplodeTimeMs - shared.GameState.MsSinceStart, 0);
        var minutes = timeLeft / 60000;
        var seconds = timeLeft % 60000 / 1000;
        shared.RenderString(spriteBatch, 0, 0, minutes.ToString("D2") + " " + seconds.ToString("D2"), 6);

        RenderInventory(spriteBatch, 5, 225);


        RenderMinimap(spriteBatch);

        RenderGrappleIcon(spriteBatch);

        RenderHealthBar(spriteBatch, shared.GameState.Asteroid.Miner, 50, 192);
        RenderHealthBar(spriteBatch, shared.GameState.Asteroid.Player, 50, 207);

        if (shared.GameState.ActiveControllableEntity.IsDead ||
            shared.GameState.ActiveControllableEntity.IsOffAsteroid)
            RenderNewGameScreen(spriteBatch, shared.GameState.ActiveControllableEntity.IsDead);
    }

    private void RenderInventory(SpriteBatch spriteBatch, int xOffset, int yoffset)
    {
        var resourceLineHeight = 40;

        shared.RenderString(spriteBatch, xOffset, yoffset,
            "DYNAMITE " + shared.GameState.Inventory.numDynamite);

        foreach (var (inventoryResource, index) in shared.GameState.Inventory.resources.Select((r, i) => (r, i)))
        {
            var lineYOffset = yoffset + resourceLineHeight * (index + 1);
            var resourceConfig = ResourceTypes.GetConfig(inventoryResource.Type);
            shared.RenderString(spriteBatch, xOffset, lineYOffset,
                resourceConfig.Name.ToUpper() + " " + inventoryResource.Count);
        }
    }

    private void RenderHealthBar(SpriteBatch spriteBatch, MiningControllableEntity entity, int xOffset, int yOffset)
    {
        spriteBatch.Draw(shared.Textures["white"],
            new Rectangle(xOffset, yOffset, (int)entity.Health, 3),
            Color.LimeGreen);
    }

    private void RenderGrappleIcon(SpriteBatch spriteBatch)
    {
        var color = shared.GameState.Asteroid.Miner.GrappleAvailable
            ? Color.LimeGreen
            : _gridColor;
        spriteBatch.Draw(shared.Textures["grapple-icon"],
            new Rectangle(10, 185, 32, 32),
            color);
    }

    private void RenderMinimap(SpriteBatch spriteBatch)
    {
        var xOffset = 10;
        var yOffset = 70;
        var playerSize = 26;
        var scale = 0.5f;
        var asteroidLineThickness = 1;

        var dividingLinesThickness = 1;
        var dividingLines = 4;

        var borderThickness = 2;

        var minimapGridSizePx = (int)(GameConfig.GridSize * scale);
        var minimapTotalSizePx = minimapGridSizePx + borderThickness * 2;


        // Top border
        spriteBatch.Draw(shared.Textures["white"], new Rectangle(xOffset, yOffset, minimapTotalSizePx, borderThickness),
            _gridColor);
        // Bottom border
        spriteBatch.Draw(shared.Textures["white"],
            new Rectangle(xOffset, yOffset + minimapGridSizePx + borderThickness, minimapTotalSizePx, borderThickness),
            _gridColor);
        // Left border
        spriteBatch.Draw(shared.Textures["white"],
            new Rectangle(xOffset, yOffset, borderThickness, minimapTotalSizePx),
            _gridColor);
        // Right border
        spriteBatch.Draw(shared.Textures["white"],
            new Rectangle(xOffset + minimapGridSizePx + borderThickness, yOffset, borderThickness, minimapTotalSizePx),
            _gridColor);

        // Draw dividing lines
        var cellWidth = minimapGridSizePx / (dividingLines + 1);
        var cellHeight = minimapGridSizePx / (dividingLines + 1);

        // Vertical dividing lines
        for (var i = 1; i <= dividingLines; i++)
        {
            var x = xOffset + borderThickness + cellWidth * i;
            spriteBatch.Draw(shared.Textures["white"],
                new Rectangle(x, yOffset, dividingLinesThickness, minimapTotalSizePx),
                _gridColor);
        }

        // Horizontal dividing lines
        for (var i = 1; i <= dividingLines; i++)
        {
            var y = yOffset + borderThickness + cellHeight * i;
            spriteBatch.Draw(shared.Textures["white"],
                new Rectangle(xOffset, y, minimapTotalSizePx, dividingLinesThickness),
                _gridColor);
        }

        foreach (var gameStateEdgeCell in shared.GameState.Asteroid.EdgeCells)
        {
            var x = xOffset + gameStateEdgeCell.x * scale;
            var y = yOffset + gameStateEdgeCell.y * scale;
            var edgeCellDestRect = new Rectangle((int)x, (int)y,
                asteroidLineThickness, asteroidLineThickness);
            spriteBatch.Draw(shared.Textures["white"], edgeCellDestRect, Color.White);
        }

        var playerGridPos =
            ViewHelpers.ToGridPosition(shared.GameState.ActiveControllableEntity.CenterPosition);
        var playerX = xOffset + playerGridPos.x * scale - playerSize / 2;
        var playerY = yOffset + playerGridPos.y * scale - playerSize / 2;
        var playerDestRect = new Rectangle((int)playerX, (int)playerY, playerSize, playerSize);
        spriteBatch.Draw(shared.Textures["radial-light"], playerDestRect, Color.Red);
    }

    private void RenderNewGameScreen(SpriteBatch spriteBatch, bool isDead)
    {
        var (viewportWidth, viewportHeight) = shared.ViewHelpers.GetViewportSize();
        spriteBatch.Draw(shared.Textures["white"], new Rectangle(0, 0, viewportWidth, viewportHeight),
            isDead ? new Color(107, 7, 0) * 0.7f : Color.DarkGreen);

        if (isDead)
        {
            shared.RenderString(spriteBatch, 300, 400, "YOU WERE INJURED", 5);
            shared.RenderString(spriteBatch, 300, 500, "PRESS N TO RESTART");
        }
        else
        {
            shared.RenderString(spriteBatch, 300, 400, "OFF THE ASTEROID", 5);
            shared.RenderString(spriteBatch, 300, 500, "PRESS N TO RESTART");


            RenderInventory(spriteBatch, 300, 600);
        }
    }
}