using HPEngine.Graphics;

namespace HPEngine;

public class Content : IDisposable
{
    private bool _disposed;
    private Dictionary<string, Texture2D> _textures = new();
    private string _assetsPath;

    public Content(string assetsPath)
    {
        _assetsPath = assetsPath;
    }

    ~Content()
    {
        Dispose(false);
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            foreach (var item in _textures)
                item.Value.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Texture GetTexture(string path)
    {
        var fullPath = _assetsPath + path;
        if (_textures.ContainsKey(fullPath))
            return _textures[fullPath];

        _textures.Add(fullPath, new Texture2D(path));
        return _textures[fullPath];
    }
}
