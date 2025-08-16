using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.UI;

public class UIMainMenu : UIScreen
{
    private readonly BaseGame _game;
    private const int StarSizePx = 170;

    public UIMainMenu(BaseGame game) : base(game)
    {
        _game = game;
        ChildrenDirection = ChildrenDirection.Column;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Center;
        Children =
        [
            new UITextElement(game)
            {
                Text = "GEM CARAVAN",
                Color = Color.White,
                Padding = 6,
                Scale = 4
            },
            new UIElement(game)
            {
                FixedHeight = 40
            },
            new UITextElement(game)
            {
                Text = "NEW GAME",
                Color = Color.White,
                BackgroundColor = Colors.DarkBlue,
                Padding = 6,
                Scale = 3,
                OnClick = () => { game.StateManager.NewGame(); }
            },
            new UIElement(game)
            {
                FixedHeight = 10
            },
            new UITextElement(game)
            {
                Text = "LOAD GAME",
                Color = Color.White,
                BackgroundColor = Colors.DarkBlue,
                Padding = 6,
                Scale = 3,
                OnClick = () => { game.StateManager.LoadGame(); }
            }
        ];
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        var totalGameTime = _game.StateManager.GameTime.TotalGameTime.TotalMilliseconds;
        var deltaTime = (float)_game.StateManager.GameTime.ElapsedGameTime.TotalSeconds;

        var screenDestRect = new Rectangle(X, Y, ComputedWidth, ComputedHeight);
        spriteBatch.Draw(_game.Textures["white"], screenDestRect, Colors.VeryDarkBlue);

        // Update and render scrolling star background
        var starBackground = _game.StateManager.Ui.State.StarBackground;
        starBackground.Update(deltaTime, ComputedWidth, ComputedHeight);

        for (int i = 0; i < starBackground.StarPositions.Count; i++)
        {
            var starPosition = starBackground.StarPositions[i];
            var starOpacity = starBackground.StarOpacities[i];
            var starRect = new Rectangle((int)starPosition.X, (int)starPosition.Y, StarSizePx, StarSizePx);
            var starColor = Color.White * starOpacity;
            spriteBatch.Draw(_game.Textures["star"], starRect, starColor);
        }

        base.Render(spriteBatch);
    }
}