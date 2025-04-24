using System.Collections.Generic;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.UI;

public sealed class UIInventoryFooter : UIElement
{
    public UIInventoryFooter(BaseGame game) : base(game)
    {
        ChildrenDirection = ChildrenDirection.Row;
        ChildrenAlign = ChildrenAlign.Center;
        ChildrenJustify = ChildrenJustify.Start;
        Children = CreateInventoryItems(game);
    }

    private List<UIElement> CreateInventoryItems(BaseGame game)
    {
        var items = new List<UIElement>();
        var inventory = game.StateManager.Inventory.resources;

        for (var i = 0; i < 10; i++)
            if (i < inventory.Count && inventory[i] != null)
            {
                var resource = inventory[i];
                var resourceConfig = ResourceTypes.GetConfig(resource.Type);
                items.Add(new UIInventoryFooterItem(game, resourceConfig.Name.ToUpper(), resource.Count));
            }
            else
            {
                items.Add(new UIInventoryFooterItem(game, "", 0));
            }

        return items;
    }
}

public sealed class UIInventoryFooterItem : UIElement
{
    public UIInventoryFooterItem(BaseGame game, string resourceName, int count) : base(game)
    {
        FixedWidth = 32;
        FixedHeight = 32;
        BackgroundColor = Color.DarkGray;
        Children =
        [
            new UITextElement(game)
            {
                Text = resourceName != "" ? resourceName : "EMPTY",
                Scale = 1,
                Padding = 2
            },
            .. count > 0
                ? new UIElement[]
                {
                    new UITextElement(game)
                    {
                        Text = count.ToString(),
                        Scale = 1,
                        Padding = 2
                    }
                }
                : [],
            
        ];
    }
}