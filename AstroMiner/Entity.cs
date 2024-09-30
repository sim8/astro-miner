using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class Entity
{
    public Vector2 Position { get; set; }
    protected virtual int BoxSizePx { get; } = 1;

    protected float GridBoxSize => (float)BoxSizePx / GameConfig.CellTextureSizePx;

    public Vector2 CenterPosition => Position + new Vector2(GridBoxSize / 2f, GridBoxSize / 2f);

    public float FrontY => Position.Y + GridBoxSize;

    public float GetDistanceTo(Entity entity)
    {
        return Vector2.Distance(CenterPosition, entity.CenterPosition);
    }

    public virtual void Update(int elapsedMs, HashSet<MiningControls> activeMiningControls)
    {
    }
}