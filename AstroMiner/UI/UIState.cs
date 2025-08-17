using AstroMiner.Effects;
using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public class UIState(BaseGame game)
{
    public bool IsInMainMenu { get; set; } = true;
    public bool IsInPauseMenu { get; set; }
    public bool IsDebugMenuOpen { get; set; } = false;
    public bool IsInventoryOpen { get; set; }
    public bool IsLaunchConsoleOpen { get; set; }
    public bool IsInShopMenu { get; set; }
    public int sellConfirmationItemIndex { get; set; } = -1;

    // TODO all very temporary. Need a proper way of tracking dialog
    public bool IsInDialog { get; set; } = false;
    public int DialogIndex { get; set; } = 0;

    public ScrollingEffectManager StarBackground { get; init; } = new();

    public void Update(GameTime gameTime, ActiveControls activeControls)
    {
        if (IsInMainMenu)
        {
            if (StarBackground.Layers.Count == 0)
            {
                StarBackground.AddLayer(new ScrollingEffectLayer
                {
                    TextureName = "star",
                    TextureSize = 170,
                    Speed = 8f,
                    Density = 9f,
                    MinOpacity = 0.1f,
                    MaxOpacity = 0.2f
                });
                StarBackground.AddLayer(new ScrollingEffectLayer
                {
                    TextureName = "star",
                    TextureSize = 170,
                    Speed = 16f,
                    Density = 7f,
                    MinOpacity = 0.4f,
                    MaxOpacity = 0.6f
                });
                // StarBackground.AddLayer(new ScrollingEffectLayer
                // {
                //     TextureName = "star",
                //     TextureSize = 170,
                //     Speed = 30f,
                //     Density = 2f,
                //     MinOpacity = 0.8f,
                //     MaxOpacity = 1.0f
                // });
            }
            else
            {
                StarBackground.Update(gameTime, game.Graphics.GraphicsDevice.Viewport.Width,
                    game.Graphics.GraphicsDevice.Viewport.Height);
            }
        }
        else if (StarBackground.Layers.Count > 0)
        {
            StarBackground.Layers.Clear();
        }
    }
}