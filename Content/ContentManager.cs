using HPEngine.Graphics;
using HPEngine.Audio;

namespace HPEngine.Content;

public class ContentManager : IDisposable
{
    private bool _disposed;
    private Dictionary<string, Texture2D> _textures = new();
    private Dictionary<string, AudioSource> _audioSources = new();
    private string _assetsPath;

    public ContentManager(string assetsPath)
    {
        _assetsPath = assetsPath;
    }

    ~ContentManager()
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

    public Texture LoadTexture(string path)
    {
        var fullPath = _assetsPath + path;
        if (_textures.ContainsKey(fullPath))
            return _textures[fullPath];

        _textures.Add(fullPath, new Texture2D(path));
        return _textures[fullPath];
    }

    public AudioSource LoadAudio(string path, int sourceCount = 1)
    {
        var fullPath = _assetsPath + path;
        if (_audioSources.ContainsKey(fullPath))
            return _audioSources[fullPath];
        
        AudioSource source = sourceCount switch
        {
            <= 1 => new SimpleAudioSource(path),
            _ => new PooledAudioSource(path, sourceCount),
        };

        _audioSources.Add(fullPath, source);
        return _audioSources[fullPath];
    }
}
