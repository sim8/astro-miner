using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public abstract class BaseWorldState(GameState g)
{
    public virtual void Initialize()
    {
    }

    public virtual void Update(HashSet<MiningControls> activeMiningControls, GameTime gameTime)
    {
    }

    public abstract bool CellIsCollideable(int x, int y);
}