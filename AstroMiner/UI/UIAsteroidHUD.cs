using System;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public class UIAsteroidHUD : UIElement
{
    public UIAsteroidHUD(BaseGame game) : base(game)
    {
        Padding = 10;
        ChildrenDirection = ChildrenDirection.Column;
        ChildrenAlign = ChildrenAlign.Center;
        Children =
        [
            new UIMinimap(game),
            new UIElement(game)
            {
                FixedHeight = 10
            },
            new UITextElement(game)
            {
                Text = GetTimeRemaining(game),
                Color = Color.LimeGreen,
                Padding = 6,
                Scale = 4
            }
        ];
    }

    private string GetTimeRemaining(BaseGame game)
    {
        var timeLeft = game.StateManager.AsteroidWorld.MsTilExplosion;
        var minutes = timeLeft / 60000;
        var seconds = timeLeft % 60000 / 1000;
        return minutes.ToString("D2") + " " + seconds.ToString("D2");
    }
}

public class UIMinimap : UIElement
{
    private const int GRID_DISTANCE_TO_EDGE_OF_MINIMAP = 25;

    private const int
        MINIMAP_GRID_SIZE = GRID_DISTANCE_TO_EDGE_OF_MINIMAP * 2;

    private readonly BaseGame _game;

    public UIMinimap(BaseGame game) : base(game)
    {
        FixedWidth = MINIMAP_GRID_SIZE * 2 * game.StateManager.Ui.UIScale;
        FixedHeight = MINIMAP_GRID_SIZE * 2 * game.StateManager.Ui.UIScale;
        _game = game;
    }

    private Color GetCellColorForMinimap(int x, int y)
    {
        var cell = _game.StateManager.AsteroidWorld.Grid.GetCellState(x, y);
        if (cell.isEmpty) return Colors.VeryDarkBlue;

        if (cell.WallType == WallType.Empty && cell.DistanceToExploredFloor == 0)
            return cell.FloorType == FloorType.Lava ? Color.Orange : Colors.LightBlue;

        return Colors.DarkBlue;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        var playerPos = _game.StateManager.Ecs.ActiveControllableEntityCenterPosition;
        var playerXFloat = playerPos.X;
        var playerYFloat = playerPos.Y;
        var minimapGridSizePx = ComputedWidth / MINIMAP_GRID_SIZE;

        // Calculate the floating-point offset for smooth movement
        var playerGridOffsetX = playerXFloat - (float)Math.Floor(playerXFloat);
        var playerGridOffsetY = playerYFloat - (float)Math.Floor(playerYFloat);

        // We need to render one extra cell in each direction to handle cropping
        var extendedGridSize = MINIMAP_GRID_SIZE + 1;

        for (var minimapX = 0; minimapX < extendedGridSize; minimapX++)
        for (var minimapY = 0; minimapY < extendedGridSize; minimapY++)
        {
            // Calculate position with floating-point offset
            var cellPosX = (minimapX - playerGridOffsetX) * minimapGridSizePx;
            var cellPosY = (minimapY - playerGridOffsetY) * minimapGridSizePx;

            var destX = X + cellPosX;
            var destY = Y + cellPosY;

            // Calculate the grid coordinates for this cell
            var gridX = (int)Math.Floor(playerXFloat) - GRID_DISTANCE_TO_EDGE_OF_MINIMAP + minimapX;
            var gridY = (int)Math.Floor(playerYFloat) - GRID_DISTANCE_TO_EDGE_OF_MINIMAP + minimapY;

            var color = GetCellColorForMinimap(gridX, gridY) * 0.5f;

            // Create rectangle for this cell
            var cellRect = new Rectangle((int)destX, (int)destY, minimapGridSizePx, minimapGridSizePx);

            // Crop the cell rectangle to fit within the minimap bounds
            var minimapBounds = new Rectangle(X, Y, ComputedWidth, ComputedHeight);
            var intersection = Rectangle.Intersect(cellRect, minimapBounds);

            // Only draw if there's a visible intersection
            if (intersection.Width > 0 && intersection.Height > 0)
                spriteBatch.Draw(_game.Textures["white"], intersection, color);
        }

        base.Render(spriteBatch);
    }
}