using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace HPEngine.Graphics;

public class Texture2D : Texture, IDisposable
{
    private bool _disposed = false;
    private Vec2i _size;

    internal int Handle;

    public Texture2D(string path)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult image =
                ImageResult.FromStream(
                    File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);

        PixelInternalFormat iformat;
        PixelFormat format;

        switch (image.Comp)
        {
            case ColorComponents.RedGreenBlueAlpha:
                iformat = PixelInternalFormat.Rgba;
                format = PixelFormat.Rgba;
                break;
            case ColorComponents.RedGreenBlue:
                iformat = PixelInternalFormat.Rgb;
                format = PixelFormat.Rgb;
                break;
            default:
                throw new NotImplementedException("Unsupported pixel format");
        }

        GenerateTexture(image.Data, image.Width, image.Height, iformat, format);
    }

    public Texture2D(Color color)
    {
        GenerateTexture(
                color.ToBytes(), 1, 1,
                PixelInternalFormat.Rgba, PixelFormat.Rgba);
    }

    internal Texture2D(int handle, Vec2i size)
    {
        Handle = handle;
        _size = size;
    }

    ~Texture2D()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        GL.DeleteTexture(Handle);
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void GenerateTexture(
            byte[] data, int width, int height,
            PixelInternalFormat iformat, PixelFormat format)
    {
        Handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GL.TexImage2D(
            TextureTarget.Texture2D,
            0, // LoD
            iformat,
            width, height,
            0, format, PixelType.UnsignedByte, data);

        Texture.ApplyDefaultFilterMode(TextureTarget.Texture2D);
        Texture.ApplyDefaultWrapMode(TextureTarget.Texture2D);

        _size = new Vec2i(width, height);
    }

    public override void Use()
    {
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public override void SetWrapMode(WrapMode mode)
    {
        Use();
        var glMode = Texture.GetOpenGLWrapMode(mode);
        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT, (int)glMode);
        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS, (int)glMode);
    }

    public override void SetFilterMode(FilterMode mode)
    {
        GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int)GetOpenGLMinFilter(mode));
        GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int)GetOpenGLMagFilter(mode));
    }

    public override int GetWidth() => _size.W;
    public override int GetHeight() => _size.H;
}
