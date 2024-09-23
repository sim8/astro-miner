using Microsoft.Xna.Framework;

namespace AstroMiner;

public class Entity
{
    public Vector2 Position { get; protected set; }
    protected virtual int TextureSize { get; } = 1;
}