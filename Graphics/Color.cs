namespace HPEngine.Graphics;

public class Color
{
    public float R, G, B, A;

    public readonly static Color White = new(1f, 1f, 1f);
    public readonly static Color Red = new(1f, 0f, 0f);
    public readonly static Color Green = new(0f, 1f, 0f);
    public readonly static Color Blue = new(0f, 0f, 1f);
    public readonly static Color Black = new(0f, 0f, 0f);

    public Color(float r, float g, float b, float a = 1f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public byte[] ToBytes()
    {
        return new byte[] {
            (byte)(R * 255),
            (byte)(G * 255),
            (byte)(B * 255),
            (byte)(A * 255),
        };
    }
}
