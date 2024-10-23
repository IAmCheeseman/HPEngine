using OpenTK.Graphics.OpenGL4;

namespace HPEngine;

public enum WrapMode
{
    None,
    Repeat,
    RepeatMirrored,
    Clamp,
}

public enum FilterMode
{
    Linear,
    Nearest,
}

public abstract class Texture
{
    public static WrapMode DefaultWrapMode = WrapMode.None;
    public static FilterMode DefaultFilterMode = FilterMode.Linear;

    public Vec2i Size
    {
        get => GetSize();
    }

    public int Width
    {
        get => GetWidth();
    }

    public int Height
    {
        get => GetHeight();
    }

    public abstract void Use();
    public abstract void SetWrapMode(WrapMode mode);
    public abstract void SetFilterMode(FilterMode mode);
    public abstract int GetWidth();
    public abstract int GetHeight();

    public virtual Texture GetTexture() => this;

    public Vec2i GetSize() => new(GetWidth(), GetHeight());

    internal static void ApplyDefaultWrapMode(TextureTarget target)
    {
        var glMode = GetOpenGLWrapMode(DefaultWrapMode);
        GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)glMode);
        GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)glMode);
    }

    internal static void ApplyDefaultFilterMode(TextureTarget target)
    {
        GL.TexParameter(
                target,
                TextureParameterName.TextureMinFilter,
                (int)GetOpenGLMinFilter(DefaultFilterMode));
        GL.TexParameter(
                target,
                TextureParameterName.TextureMagFilter,
                (int)GetOpenGLMagFilter(DefaultFilterMode));
    }

    internal static TextureMinFilter GetOpenGLMinFilter(FilterMode mode)
    {
        switch (mode)
        {
            case FilterMode.Linear:
                return TextureMinFilter.Linear;
            case FilterMode.Nearest:
                return TextureMinFilter.Nearest;
        }
        return TextureMinFilter.Linear;
    }

    internal static TextureMagFilter GetOpenGLMagFilter(FilterMode mode)
    {
        switch (mode)
        {
            case FilterMode.Linear:
                return TextureMagFilter.Linear;
            case FilterMode.Nearest:
                return TextureMagFilter.Nearest;
        }
        return TextureMagFilter.Linear;
    }

    internal static TextureWrapMode GetOpenGLWrapMode(WrapMode mode)
    {
        switch (mode)
        {
            case WrapMode.None:
                return TextureWrapMode.ClampToBorder;
            case WrapMode.Repeat:
                return TextureWrapMode.Repeat;
            case WrapMode.RepeatMirrored:
                return TextureWrapMode.MirroredRepeat;
            case WrapMode.Clamp:
                return TextureWrapMode.ClampToEdge;
        }

        return TextureWrapMode.ClampToBorder;
    }
}
