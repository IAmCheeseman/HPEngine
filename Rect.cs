namespace HPEngine;

public struct Rect
{
    public Vec2 Start;
    public Vec2 Size;

    public float X
    {
        get => Start.X;
        set => Start.X = value;
    }

    public float Y
    {
        get => Start.Y;
        set => Start.Y = value;
    }

    public float W
    {
        get => Size.W;
        set => Size.W = value;
    }

    public float H
    {
        get => Size.H;
        set => Size.H = value;
    }

    public Vec2 End
    {
        get => Start + Size;
        set => Size = value - Start;
    }

    public Rect(Vec2 start, Vec2 size)
    {
        Start = start;
        Size = size;
    }
}
