using System;

namespace AstroMiner.UI;

public class UIInGameMenu(BaseGame game, Action<int> onItemClick = null) : UIInventory(game, onItemClick);