using AstroMiner.ECS;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.Utilities;

public static class EntityHelpers
{
    public static float GetDistanceBetween(Ecs ecs, int entityId1, int entityId2)
    {
        var pos1 = ecs.GetComponent<PositionComponent>(entityId1);
        var pos2 = ecs.GetComponent<PositionComponent>(entityId2);

        if (pos1 == null || pos2 == null)
            return float.MaxValue;

        return Vector2.Distance(pos1.CenterPosition, pos2.CenterPosition);
    }
}