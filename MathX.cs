namespace HPEngine;

public static class MathX
{
    public static float Lerp(float a, float b, float t)
    {
        return (b - a) * t + a;
    }

    public static float DtLerp(float a, float b, float t, float dt)
    {
        return Lerp(a, b, 1f - MathF.Pow(0.5f, dt * t));
    }

    public static double Lerp(double a, double b, double t)
    {
        return (b - a) * t + a;
    }

    public static double DtLerp(double a, double b, double t, double dt)
    {
        return Lerp(a, b, 1f - Math.Pow(0.5, dt * t));
    }
}
