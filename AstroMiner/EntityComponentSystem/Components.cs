using System.Drawing;
using AstroMiner.Definitions;
using Microsoft.Xna.Framework;

namespace AstroMiner.EntityComponentSystem;

public struct PositionAndSize
{
    public int BoxSizePx;
    public Vector2 Position;

    public float GridBoxSize => (float)BoxSizePx / GameConfig.CellTextureSizePx;

    public Vector2 CenterPosition => Position + new Vector2(GridBoxSize / 2f, GridBoxSize / 2f);

    public float FrontY => Position.Y + GridBoxSize;

    public RectangleF Rectangle => new(Position.X, Position.Y, GridBoxSize, GridBoxSize);
}

public struct Explodable
{
    public int TotalTimeMs;
    public int TimeRemainingMs;
}