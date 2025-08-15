using AstroMiner.Definitions;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public class UIAsteroidHUD : UIElement
{
    public UIAsteroidHUD(BaseGame game) : base(game)
    {
        Padding = 10;
        ChildrenDirection = ChildrenDirection.Column;
        Children =
        [
            new UIMinimap(game),
            new UIElement(game)
            {
                FixedHeight = 40
            },
            new UITextElement(game)
            {
                Text = GetTimeRemaining(game),
                Color = Color.White,
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
    private const int GRID_DISTANCE_TO_EDGE_OF_MINIMAP = 30;

    private const int
        MINIMAP_GRID_SIZE = GRID_DISTANCE_TO_EDGE_OF_MINIMAP * 2 + 1; // both sides + cell containing player

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
        if (cell.WallType != WallType.Empty) return Colors.DarkBlue;
        if (cell.FloorType != FloorType.Empty)
            return cell.DistanceToExploredFloor == 0 ? Colors.LightBlue : Colors.DarkBlue;

        return Colors.VeryDarkBlue;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        var (playerX, playerY) =
            ViewHelpers.ToGridPosition(_game.StateManager.Ecs.ActiveControllableEntityCenterPosition);
        var minimapGridSizePx = ComputedWidth / MINIMAP_GRID_SIZE;
        for (var minimapX = 0; minimapX < MINIMAP_GRID_SIZE; minimapX++)
        for (var minimapY = 0; minimapY < MINIMAP_GRID_SIZE; minimapY++)
        {
            var destX = X + minimapX * minimapGridSizePx;
            var destY = Y + minimapY * minimapGridSizePx;
            var destRect = new Rectangle(destX, destY, minimapGridSizePx, minimapGridSizePx);

            var gridX = playerX - GRID_DISTANCE_TO_EDGE_OF_MINIMAP + minimapX;
            var gridY = playerY - GRID_DISTANCE_TO_EDGE_OF_MINIMAP + minimapY;

            var color = GetCellColorForMinimap(gridX, gridY);


            spriteBatch.Draw(_game.Textures["white"], destRect, color);

            base.Render(spriteBatch);
        }

        base.Render(spriteBatch);
    }
}