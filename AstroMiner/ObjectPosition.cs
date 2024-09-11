namespace AstroMiner;

public struct ObjectPosition
{
    public float X { get; set; }
    public float Y { get; set; }

    public ObjectPosition(float x, float y)
    {
        X = x;
        Y = y;
    }

    public void Move(float deltaX, float deltaY)
    {
        X += deltaX;
        Y += deltaY;
    }
}