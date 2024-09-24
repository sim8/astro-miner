using Microsoft.Xna.Framework;

namespace AstroMiner;

public class Entity
{
    public Vector2 Position { get; protected set; }
    public virtual int BoxSizePx { get; } = 1;

    public float GridBoxSize => (float)BoxSizePx / GameConfig.CellTextureSizePx;

    public Vector2 CenterPosition => Position + new Vector2(GridBoxSize / 2f, GridBoxSize / 2f);

    public float GetDistanceTo(Entity entity)
    {
        return Vector2.Distance(CenterPosition, entity.CenterPosition);
    }
}