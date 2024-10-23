using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using HPEngine.Graphics;

namespace HPEngine;

public struct WindowSettings
{
    public static WindowSettings Default = new() {};

    public string Title = "HPEngine";
    public Vec2i Size = new(800, 600);

    public WindowSettings()
    {
    }
}

public class Window : IRenderTarget
{
    internal NativeWindow Handle { get; private set; }
    private Framebuffer _framebuffer;

    public Vec2i Size
    {
        get => new(Handle.ClientSize.X, Handle.ClientSize.Y);
        set => Handle.ClientSize = new Vector2i(value.X, value.Y);
    }

    public string Title
    {
        get => Handle.Title;
        set => Handle.Title = value;
    }

    public bool ShouldClose
    {
        get => Handle.IsExiting;
    }

    public delegate void FramebufferResizedHandler(Vec2i size);
    public delegate void MouseScrolledHandler(Vec2 by);
    public delegate void TextInputHandler(char c);

    public event FramebufferResizedHandler? FramebufferResized;
    public event MouseScrolledHandler? MouseScrolled;
    public event TextInputHandler? TextInput;

    internal Window(WindowSettings settings)
    {
        var nativeSettings = new NativeWindowSettings() {
            Title = settings.Title,
            ClientSize = (settings.Size.W, settings.Size.H),
        };
        Handle = new NativeWindow(nativeSettings);
        Handle.MakeCurrent();

        Handle.FramebufferResize += EmitFramebufferResized;
        Handle.MouseWheel += EmitMouseScrolled;
        Handle.TextInput += EmitTextInput;

        _framebuffer = new Framebuffer() {
            Handle = 0,
            Size = Size,
        };
    }

    private void EmitFramebufferResized(FramebufferResizeEventArgs args)
    {
        var size = new Vec2i(args.Width, args.Height);
        _framebuffer.Size = size;
        FramebufferResized?.Invoke(size);
    }

    private void EmitMouseScrolled(MouseWheelEventArgs args)
    {
        MouseScrolled?.Invoke(new Vec2(args.OffsetX, args.OffsetY));
    }

    private void EmitTextInput(TextInputEventArgs args)
    {
        TextInput?.Invoke((char)args.Unicode);
    }

    public void SwapBuffers()
    {
        Handle.Context.SwapBuffers();
    }

    public void PollEvents()
    {
        Handle.ProcessEvents(0.001f);
    }

    public void Close()
    {
        Handle.Close();
    }

    public Framebuffer BindFramebuffer(Renderer renderer)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return _framebuffer;
    }
}
