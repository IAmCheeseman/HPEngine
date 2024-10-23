using OpenTK.Audio.OpenAL;

namespace HPEngine;

public class PooledAudioSource : AudioSource, IDisposable
{
    private bool _disposed = false;
    private int[] _sourceHandles;
    private int _currentSource = 0;

    internal int BufferHandle;

    public PooledAudioSource(string path, int poolSize = 4)
    {
        BufferHandle = AL.GenBuffer();

        var soundData = AudioSource.LoadData(path);

        var size = soundData.Samples.Length * sizeof(byte);
        var format = soundData.GetALFormat();

        AL.BufferData(
                BufferHandle,
                format, soundData.Samples, soundData.SampleRate);
        
        _sourceHandles = new int[poolSize];
        for (var i = 0; i < poolSize; i++)
        {
            _sourceHandles[i] = CreateSource();
        }
    }

    ~PooledAudioSource()
    {
        Dispose(false);
    }

    private int CreateSource()
    {
        int source = AL.GenSource();

        AL.Source(source, ALSourcef.Pitch, 1f);
        AL.Source(source, ALSourcef.Gain, 1f);
        AL.Source(source, ALSource3f.Position, 0f, 0f, 0f);
        AL.Source(source, ALSource3f.Velocity, 0f, 0f, 0f);
        AL.Source(source, ALSourceb.Looping, false);
        AL.Source(source, ALSourcei.Buffer, BufferHandle);

        return source;
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        AL.DeleteBuffer(BufferHandle);

        foreach (var sourceHandle in _sourceHandles)
            AL.DeleteSource(sourceHandle);

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public override void Play()
    {
        AL.SourcePlay(_sourceHandles[_currentSource]);

        _currentSource++;
        if (_currentSource >= _sourceHandles.Length)
            _currentSource = 0;
    }
}
