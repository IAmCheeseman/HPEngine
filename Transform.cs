using OpenTK.Mathematics; 

namespace HPEngine;

public class Transform
{
    public Vec2 Position = new(0f);
    public float Rotation = 0f;
    public Vec2 Scale = new(1f);
    public Vec2 Offset = new(0f);
    public Vec2 Shear = new(0f);

    public Transform()
    {
    }

    internal Matrix4 GetMatrix()
    {
        var trans = Matrix4.CreateTranslation(Position.X, Position.Y, 0f);
        var rotation = Matrix4.CreateRotationZ(Rotation);
        var scale = Matrix4.CreateScale(Scale.X, Scale.Y, 0f);
        var offset = Matrix4.CreateTranslation(Offset.X, Offset.Y, 0f);
        var skew = Matrix4.Identity;
        skew.M12 = Shear.X;
        skew.M21 = Shear.Y;
        return offset * skew * rotation * scale * trans;
    }
}
