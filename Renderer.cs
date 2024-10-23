using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace HPEngine;

public enum VertexFormat
{
    PosUvColor, // vec2 vec2 vec4
    PosUv, // vec2 vec2
    PosColor, // vec2 vec4
    Pos, // vec2
}

public class Renderer : IDisposable
{
    private class BatchRequest
    {
        public bool Invalid;
        public PrimitiveType PrimitiveType;
        public VertexFormat Format;
        public Shader Shader;
        public Texture Texture;
        public List<float> Vertices;
        public List<uint> Indices;

        private uint _topIndex = 0;

        public BatchRequest(Renderer renderer)
        {
            Shader = renderer._defaultShader;
            Texture = renderer._colorTexture;
            PrimitiveType = PrimitiveType.Triangles;
            Vertices = new List<float>();
            Indices = new List<uint>();
        }

        public uint GetTopIndex() => _topIndex;

        public void AddIndex(uint start, uint index)
        {
            if (start + index > _topIndex)
                _topIndex = start + index + 1;
            Indices.Add(start + index);
        }

        public void AddVertex(float[] vertex)
        {
            Vertices.EnsureCapacity(Vertices.Count + vertex.Length);
            foreach (var e in vertex)
                Vertices.Add(e);
        }
    }

    private bool _disposed = false;
    private Color _currentColor = Color.White;
    private Matrix4 _projection = Matrix4.Identity;
    private Shader _defaultShader = new("default.vert", "default.frag");
    private Texture2D _colorTexture = new(Color.White);
    private Shader _currentShader;
    private BatchRequest _batch;
    private IRenderTarget _currentRenderTarget;
    private Framebuffer _currentFramebuffer;
    private int _vbo = GL.GenBuffer();
    private int _ebo = GL.GenBuffer();
    private int _vao = GL.GenVertexArray();

    private int _drawCalls = 0;
    private int _prevDrawCalls = 0;

    private Transform _view = new();

    public Renderer(Context context)
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        _colorTexture.SetWrapMode(WrapMode.Repeat);
        _colorTexture.SetFilterMode(FilterMode.Nearest);

        _batch = new BatchRequest(this) {
            Invalid = true,
        };
        context.Window.FramebufferResized += OnFramebufferResized;

        _currentRenderTarget = context.Window;
        _currentFramebuffer = context.Window.BindFramebuffer(this);
        _currentShader = _defaultShader;
    }

    ~Renderer()
    {
        Dispose(false);
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        
        if (disposing)
        {
            _defaultShader.Dispose();
            _colorTexture.Dispose();
        }

        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        GL.DeleteVertexArray(_vao);
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public int GetDrawCalls() => _prevDrawCalls;

    private void OnFramebufferResized(Vec2i size)
    {
        // I think this technically only needs to be run when the window is the
        // active render target.
        GL.Viewport(0, 0, _currentFramebuffer.Size.W, _currentFramebuffer.Size.H);
        _projection = Matrix4.CreateOrthographicOffCenter(
                0, _currentFramebuffer.Size.W,
                _currentFramebuffer.Size.H, 0,
                -1, 1);
    }

    public void PrepareNextFrame()
    {
        FlushBatch();
        _prevDrawCalls = _drawCalls;
        _drawCalls = 0;
    }

    public void FlushBatch()
    {
        if (_batch.Invalid)
            return;
        _batch.Invalid = true;

        _batch.Shader.Use();
        _batch.Shader.Send("view", _view.GetMatrix());
        _batch.Shader.Send("projection", _projection);

        var data = _batch.Vertices.ToArray();
        SetVertexData(data);
        SetElementData(_batch.Indices.ToArray());
        SetUpVertexFormat(VertexFormat.PosUvColor);
        _batch.Texture.Use();

        GL.DrawElements(
            _batch.PrimitiveType,
            _batch.Indices.Count, DrawElementsType.UnsignedInt, 0);
        _drawCalls++;
    }

    private BatchRequest RequestBatch(
            VertexFormat format, PrimitiveType primitiveType,
            Texture? textureOrNull)
    {
        Texture texture =
            textureOrNull == null ? _colorTexture : textureOrNull!;

        bool flush = false;
        bool newBatch = false;

        if (_batch.Format != format ||
            _batch.Texture != texture ||
            _batch.Shader != _currentShader)
        {
            flush = true;
            newBatch = true;
        }

        if (_batch.Invalid)
        {
            flush = false;
            newBatch = true;
        }

        if (flush)
            FlushBatch();

        if (newBatch)
        {
            _batch = new BatchRequest(this) {
                Format = format,
                Shader = _currentShader,
                Texture = texture,
                PrimitiveType = primitiveType,
            };
        }

        return _batch;
    }

    public void Clear(Color color)
    {
        GL.ClearColor(color.R, color.G, color.B, color.A);
        GL.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void SetRenderTarget(IRenderTarget target)
    {
        _currentRenderTarget.UnbindFramebuffer(this);
        FlushBatch();
        _currentRenderTarget = target;
        _currentFramebuffer = target.BindFramebuffer(this);

        GL.Viewport(0, 0, _currentFramebuffer.Size.W, _currentFramebuffer.Size.H);
        _projection = Matrix4.CreateOrthographicOffCenter(
                0, _currentFramebuffer.Size.W,
                _currentFramebuffer.Size.H, 0,
                -1, 1);
    }

    private int GetVertexFormatLength(VertexFormat format)
    {
        switch (format)
        {
            case VertexFormat.PosUvColor: return 8;
            default: throw new NotImplementedException();
        }
    }

    private void SetUpVertexFormat(VertexFormat format)
    {
        switch (format)
        {
            case VertexFormat.PosUvColor:
                GL.BindVertexArray(_vao);

                var positionAttribute =
                    _currentShader.GetAttributeLocation("in_Position");
                GL.VertexAttribPointer(
                        positionAttribute,
                        2, VertexAttribPointerType.Float, false,
                        8 * sizeof(float), 0);
                GL.EnableVertexAttribArray(positionAttribute);

                var uvAttribute = _currentShader.GetAttributeLocation("in_Uv");
                GL.VertexAttribPointer(
                        uvAttribute,
                        2, VertexAttribPointerType.Float, false,
                        8 * sizeof(float), 2 * sizeof(float));
                GL.EnableVertexAttribArray(uvAttribute);

                var colorAttribute =
                    _currentShader.GetAttributeLocation("in_Color");
                GL.VertexAttribPointer(
                        colorAttribute,
                        4, VertexAttribPointerType.Float, false,
                        8 * sizeof(float), 4 * sizeof(float));
                GL.EnableVertexAttribArray(colorAttribute);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void SetVertexData(float[] data)
    {
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(
                BufferTarget.ArrayBuffer,
                data.Length * sizeof(float), data, BufferUsageHint.DynamicDraw);
    }

    private void SetElementData(uint[] data)
    {
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                data.Length * sizeof(uint), data, BufferUsageHint.DynamicDraw);
    }

    public void SetColor(Color color) => _currentColor = color;
    public Color GetColor() => _currentColor;

    public void Rectangle(Vec2 pos, Vec2 size)
    {
        var batch = RequestBatch(VertexFormat.PosUvColor, PrimitiveType.Triangles, null);
        uint indexStart = batch.GetTopIndex();

        batch.AddIndex(indexStart, 0);
        batch.AddIndex(indexStart, 1);
        batch.AddIndex(indexStart, 2);
        batch.AddIndex(indexStart, 0);
        batch.AddIndex(indexStart, 2);
        batch.AddIndex(indexStart, 3);

        batch.AddVertex(
            new float[] {
                pos.X, pos.Y,
                0f, 0f,
                _currentColor.R, _currentColor.G, _currentColor.B, _currentColor.A
            });
        batch.AddVertex(
            new float[] {
                pos.X+size.W, pos.Y,
                1f, 0f,
                _currentColor.R, _currentColor.G, _currentColor.B, _currentColor.A
            });
        batch.AddVertex(
            new float[] {
                pos.X+size.W, pos.Y+size.H,
                1f, 1f,
                _currentColor.R, _currentColor.G, _currentColor.B, _currentColor.A
            });
        batch.AddVertex(
            new float[] {
                pos.X, pos.Y+size.H,
                0f, 1f,
                _currentColor.R, _currentColor.G, _currentColor.B, _currentColor.A
            });
    }

    public void Line(Vec2 start, Vec2 end)
    {
        var batch = RequestBatch(VertexFormat.PosUvColor, PrimitiveType.Lines, null);
        uint indexStart = batch.GetTopIndex();

        batch.AddIndex(indexStart, 0);
        batch.AddIndex(indexStart, 1);

        batch.AddVertex(
            new float[] {
                start.X, start.Y,
                0f, 0f,
                _currentColor.R, _currentColor.G, _currentColor.B, _currentColor.A
            });
        batch.AddVertex(
            new float[] {
                end.X, end.Y,
                0f, 0f,
                _currentColor.R, _currentColor.G, _currentColor.B, _currentColor.A
            });
    }

    public void ApplyTransform(Matrix4 m, float bx, float by, out float x, out float y)
    {
        x = (m.M11 * bx) + (m.M21 * by) + (m.M41);
        y = (m.M12 * bx) + (m.M22 * by) + (m.M42);
    }

    public void DrawTexture(Texture texture, Rect rect, Transform transform)
    {
        // There's some texture types that just wrap around another texture
        // We want the underlying texture, so that we can better batch textures
        var underlying = texture.GetTexture();
        var batch = RequestBatch(VertexFormat.PosUvColor, PrimitiveType.Triangles, underlying);
        uint indexStart = batch.GetTopIndex();

        batch.AddIndex(indexStart, 0);
        batch.AddIndex(indexStart, 1);
        batch.AddIndex(indexStart, 2);
        batch.AddIndex(indexStart, 0);
        batch.AddIndex(indexStart, 2);
        batch.AddIndex(indexStart, 3);

        var m = transform.GetMatrix();

        ApplyTransform(m, 0f, 0f, out float tlx, out float tly);
        ApplyTransform(m, rect.Size.W, 0f, out float trx, out float _try);
        ApplyTransform(m, rect.Size.W, rect.Size.H, out float brx, out float bry);
        ApplyTransform(m, 0, rect.Size.H, out float blx, out float bly);

        rect.Start.Y = underlying.Size.Y - rect.Size.Y - rect.Start.Y;

        var UvStart = rect.Start / underlying.Size;
        var UvEnd = rect.End / underlying.Size;

        batch.AddVertex(
            new float[] {
                tlx, tly,
                UvStart.X, UvEnd.Y,
                _currentColor.R, _currentColor.G, _currentColor.B, _currentColor.A
            });
        batch.AddVertex(
            new float[] {
                trx, _try,
                UvEnd.X, UvEnd.Y,
                _currentColor.R, _currentColor.G, _currentColor.B, _currentColor.A
            });
        batch.AddVertex(
            new float[] {
                brx, bry,
                UvEnd.X, UvStart.Y,
                _currentColor.R, _currentColor.G, _currentColor.B, _currentColor.A
            });
        batch.AddVertex(
            new float[] {
                blx, bly,
                UvStart.X, UvStart.Y,
                _currentColor.R, _currentColor.G, _currentColor.B, _currentColor.A
            });
    }

    public void DrawTexture(Texture texture, Transform transform)
    {
        DrawTexture(
            texture, new Rect(Vec2.Zero, texture.Size), transform);
    }

    public void DrawTexture(Texture texture, Vec2 position)
    {
        DrawTexture(texture, new Transform() { Position = position });
    }

    public void DrawTexture(Texture texture, Rect rect, Vec2 position)
    {
        DrawTexture(texture, rect, new Transform() { Position = position });
    }

    public void SetViewPosition(Vec2 position)
    {
        FlushBatch();
        _view.Position = position;
    }

    public void SetShader(Shader? shaderOrNull)
    {
        _currentShader = shaderOrNull == null ? _defaultShader : shaderOrNull!;
    }
}
