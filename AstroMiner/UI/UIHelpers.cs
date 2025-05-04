#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace AstroMiner.UI;

public static class UIHelpers
{
    public static List<UIElement?> FilterNull(List<UIElement?> children)
    {
        return children.Where(child => child != null).ToList();
    }
}