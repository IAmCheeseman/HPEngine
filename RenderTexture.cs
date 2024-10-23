using OpenTK.Graphics.OpenGL4;

namespace HPEngine;

public class RenderTexture : Texture, IRenderTarget, IDisposable
{
    private bool _disposed = false;
    internal Texture2D Texture;
    private Framebuffer _framebuffer;

    private Vec2i _size;

    public RenderTexture(Vec2i size)
    {
        _size = size;

        int textureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);
        GL.TexImage2D(
                TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgb, size.W, size.H,
                0, PixelFormat.Rgb, PixelType.UnsignedByte, 0);
        ApplyDefaultFilterMode(TextureTarget.Texture2D);
        ApplyDefaultWrapMode(TextureTarget.Texture2D);

        Texture = new Texture2D(textureHandle, size);

        _framebuffer = new Framebuffer() {
            Size = size,
        };

        _framebuffer.Handle = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer.Handle);
        GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, textureHandle, 0);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    ~RenderTexture()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        GL.DeleteFramebuffer(_framebuffer.Handle);
        if (disposing) 
            Texture.Dispose();
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Framebuffer BindFramebuffer(Renderer renderer)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer.Handle);
        return _framebuffer;
    }

    public override void Use()
    {
        Texture.Use();
    }

    public override void SetWrapMode(WrapMode mode)
    {
        Texture.SetWrapMode(mode);
    }

    public override void SetFilterMode(FilterMode mode)
    {
        Texture.SetFilterMode(mode);
    }

    public override int GetWidth() => _size.W;
    public override int GetHeight() => _size.H;
}
