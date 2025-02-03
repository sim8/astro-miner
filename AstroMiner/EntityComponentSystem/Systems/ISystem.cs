using Microsoft.Xna.Framework;

namespace AstroMiner.EntityComponentSystem.Systems;

public interface ISystem
{
    void Update(GameTime gameTime);
}