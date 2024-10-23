namespace HPEngine;

public struct Vec2i
{
    public static readonly Vec2i Zero  = new(0);
    public static readonly Vec2i Right = new(1, 0);
    public static readonly Vec2i Left  = new(-1, 0);
    public static readonly Vec2i Up    = new(0, -1);
    public static readonly Vec2i Down  = new(0, 1);

    public int X;
    public int Y;

    public int W
    {
        get => X;
        set => X = value;
    }

    public int H
    {
        get => Y;
        set => Y = value;
    }

    public Vec2i(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vec2i(int xy = 0)
    {
        X = xy;
        Y = xy;
    }

    public override string ToString() => $"({X}, {Y})";

    public static Vec2i operator +(Vec2i lhs, Vec2i rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y);
    public static Vec2i operator -(Vec2i lhs, Vec2i rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y);
    public static Vec2i operator -(Vec2i op) => new(-op.X, -op.Y);
    public static Vec2i operator *(Vec2i lhs, Vec2i rhs) => new(lhs.X * rhs.X, lhs.Y * rhs.Y);
    public static Vec2i operator /(Vec2i lhs, Vec2i rhs) => new(lhs.X / rhs.X, lhs.Y / rhs.Y);
    public static Vec2i operator *(Vec2i lhs, int rhs) => new(lhs.X * rhs, lhs.Y * rhs);
    public static Vec2i operator /(Vec2i lhs, int rhs) => new(lhs.X / rhs, lhs.Y / rhs);
    public static implicit operator Vec2(Vec2i v) => new Vec2(v);
}

public struct Vec2
{
    public static readonly Vec2 Zero  = new(0f);
    public static readonly Vec2 Right = new(1f, 0f);
    public static readonly Vec2 Left  = new(-1f, 0f);
    public static readonly Vec2 Up    = new(0f, -1f);
    public static readonly Vec2 Down  = new(0f, 1f);

    public float X;
    public float Y;

    public float W
    {
        get => X;
        set => X = value;
    }

    public float H
    {
        get => Y;
        set => Y = value;
    }

    public Vec2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public Vec2(float xy = 0f)
    {
        X = xy;
        Y = xy;
    }

    public Vec2(Vec2i veci)
    {
        X = veci.X;
        Y = veci.Y;
    }

    public float Dot(Vec2 other) => X*other.X + Y*other.Y;
    public float Length() => MathF.Sqrt(X*X + Y*Y);
    public float LengthSquared() => X*X + Y*Y;

    public Vec2 Normalized()
    {
        float length = Length();
        if (length == 0)
            return new();
        return new(X / length, Y / length);
    }

    public void Normalize()
    {
        float length = Length();
        if (length == 0)
            return;
        X /= length;
        Y /= length;
    }

    public float Angle() => MathF.Atan2(Y, X);

    public Vec2 Lerp(Vec2 to, float t)
    {
        return new(MathX.Lerp(X, to.X, t), MathX.Lerp(Y, to.Y, t));
    }

    public Vec2 DtLerp(Vec2 to, float t, float dt)
    {
        return new(MathX.DtLerp(X, to.X, t, dt), MathX.DtLerp(Y, to.Y, t, dt));
    }

    public Vec2 Floor() => new(MathF.Floor(X), MathF.Floor(Y));
    public Vec2 Ceiling() => new(MathF.Ceiling(X), MathF.Ceiling(Y));
    public Vec2 Round() => new(MathF.Round(X), MathF.Round(Y));

    public override string ToString() => $"({X}, {Y})";

    public static Vec2 operator +(Vec2 lhs, Vec2 rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y);
    public static Vec2 operator -(Vec2 lhs, Vec2 rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y);
    public static Vec2 operator -(Vec2 op) => new(-op.X, -op.Y);
    public static Vec2 operator *(Vec2 lhs, Vec2 rhs) => new(lhs.X * rhs.X, lhs.Y * rhs.Y);
    public static Vec2 operator /(Vec2 lhs, Vec2 rhs) => new(lhs.X / rhs.X, lhs.Y / rhs.Y);
    public static Vec2 operator *(Vec2 lhs, float rhs) => new(lhs.X * rhs, lhs.Y * rhs);
    public static Vec2 operator /(Vec2 lhs, float rhs) => new(lhs.X / rhs, lhs.Y / rhs);
    public static explicit operator Vec2i(Vec2 v) => new Vec2i((int)v.X, (int)v.Y);
}
