namespace HPEngine;

public class Viewport : IRenderTarget, IDisposable
{
    private bool _disposed = false;
    private Context _context;

    public RenderTexture RenderTexture;

    public Vec2i Size { get; private set; }
    public Vec2 CameraPosition;

    public Viewport(Context context, Vec2i size)
    {
        _context = context;
        Size = size;
        RenderTexture = new RenderTexture(size);
    }

    ~Viewport()
    {
        Dispose(false);
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            RenderTexture.Dispose();
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Framebuffer BindFramebuffer(Renderer renderer)
    {
        renderer.SetViewPosition(-CameraPosition);
        return RenderTexture.BindFramebuffer(renderer);
    }
    
    public void UnbindFramebuffer(Renderer renderer)
    {
        renderer.SetViewPosition(Vec2.Zero);
    }

    private Transform GetTransform()
    {
        var transform = new Transform();

        var windowSize = (Vec2)_context.Window.Size;
        var screenSize = (Vec2)Size;

        transform.Scale = new Vec2(MathF.Min(
                windowSize.W / screenSize.W,
                windowSize.H / screenSize.H));
        transform.Position = (windowSize - screenSize * transform.Scale) / 2;

        return transform;
    }

    public Vec2 GetWorldMousePosition()
    {
        var transform = GetTransform();
        var mousePosition = _context.Input.GetWindowMousePosition();

        mousePosition -= transform.Position;
        mousePosition /= transform.Scale;
        mousePosition += CameraPosition;
        return mousePosition.Round();
    }

    public Vec2 GetMousePosition()
    {
        var transform = GetTransform();
        var mousePosition = _context.Input.GetWindowMousePosition();

        mousePosition -= transform.Position;
        mousePosition /= transform.Scale;
        return mousePosition.Round();
    }

    // FIXME: This api is sorta inconsistent with `AnimatedTexture`'s. Maybe
    // make it align more with that?
    public void Draw(Renderer renderer)
    {
        renderer.DrawTexture(RenderTexture, GetTransform());
    }
}
