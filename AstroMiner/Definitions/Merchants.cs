using System;
using System.Collections.Generic;

namespace AstroMiner.Definitions;

public enum MerchantType
{
    Explosives,
    Medic
}

public class MerchantConfig(string name, List<ItemType> items)
{
    public string Name { get; } = name;
    public List<ItemType> Items { get; } = items;
}

public static class Merchants
{
    private static readonly IReadOnlyDictionary<MerchantType, MerchantConfig> AllMerchantsConfig =
        new Dictionary<MerchantType, MerchantConfig>
        {
            { MerchantType.Explosives, new MerchantConfig("Explosives", [ItemType.Dynamite]) },
            { MerchantType.Medic, new MerchantConfig("Medic", [ItemType.HealthJuice]) }
        };

    public static MerchantConfig GetConfig(MerchantType merchantType)
    {
        if (!AllMerchantsConfig.TryGetValue(merchantType, out var config))
            throw new ArgumentException($"No configuration found for MerchantType: {merchantType}");

        return config;
    }
}