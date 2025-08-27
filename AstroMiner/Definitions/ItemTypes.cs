using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner.Definitions;

public enum ItemType
{
    Ruby,
    Diamond,
    Gold,
    Nickel,
    Dynamite,
    Drill,
    HealthJuice
}

public class ItemTypeConfig(string name, int colIndex, int rowIndex, int salePrice, int buyPrice = -1)
{
    private readonly int colIndex = colIndex;
    private readonly int rowIndex = rowIndex;
    public string Name { get; } = name;
    public int SalePrice { get; } = salePrice;
    public int BuyPrice { get; } = buyPrice;

    public Rectangle GetSourceRect()
    {
        return new Rectangle(colIndex * 16, rowIndex * 16, 16, 16);
    }
}

public static class ItemTypes
{
    private static readonly IReadOnlyDictionary<ItemType, ItemTypeConfig> AllItemTypeConfig =
        new Dictionary<ItemType, ItemTypeConfig>
        {
            { ItemType.Ruby, new ItemTypeConfig("Ruby", 0, 0, 4) },
            { ItemType.Diamond, new ItemTypeConfig("Diamond", 1, 0, 16) },
            { ItemType.Gold, new ItemTypeConfig("Gold", 2, 0, 6) },
            { ItemType.Nickel, new ItemTypeConfig("Nickel", 3, 0, 4) },
            { ItemType.Dynamite, new ItemTypeConfig("Dynamite", 4, 0, -1, 5) },
            { ItemType.Drill, new ItemTypeConfig("Drill", 5, 0, -1) },
            { ItemType.HealthJuice, new ItemTypeConfig("Health Juice", 6, 0, -1, 10) }
        };

    public static ItemTypeConfig GetConfig(ItemType itemType)
    {
        if (!AllItemTypeConfig.TryGetValue(itemType, out var config))
            throw new ArgumentException($"No configuration found for ItemType: {itemType}");

        return config;
    }
}