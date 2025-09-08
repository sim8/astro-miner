using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

// Top-level screens. Only one can be open at once
public enum Screen
{
    InGameMenu,
    LaunchConsole,
    SaleMenu,
    MerchantMenu,
}

public enum InGameMenuSubScreen
{
    Inventory,
    Map
}

public class UIState(BaseGame game)
{
    public Screen? ActiveScreen { get; set; } = null;
    public InGameMenuSubScreen ActiveInGameMenuSubScreen { get; set; } = InGameMenuSubScreen.Inventory;
    public bool IsInMainMenu { get; set; } = true;
    public bool IsInPauseMenu { get; set; }
    public bool IsDebugMenuOpen { get; set; } = false;
    public int SellConfirmationItemIndex { get; set; } = -1;
    public ItemType? PurchaseConfirmationItemType { get; set; } = null;
    public MerchantType? ActiveMerchantType { get; set; } = null;

    // TODO all very temporary. Need a proper way of tracking dialog
    public bool IsInDialog { get; set; } = false;
    public int DialogIndex { get; set; } = 0;

    public void Update(GameTime gameTime, ActiveControls activeControls)
    {

    }
}