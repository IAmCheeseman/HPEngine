using OpenTK.Audio.OpenAL;

namespace HPEngine.Audio;

public class SimpleAudioSource : AudioSource
{
    private bool _disposed;

    internal int SourceHandle;
    internal int BufferHandle;

    public SimpleAudioSource(string path)
    {
        SourceHandle = AL.GenSource();
        BufferHandle = AL.GenBuffer();

        var soundData = AudioSource.LoadData(path);

        var size = soundData.Samples.Length * sizeof(byte);
        var format = soundData.GetALFormat();

        AL.BufferData(
                BufferHandle,
                format, soundData.Samples, soundData.SampleRate);
        Console.WriteLine(ALC.GetError(AudioEngine.CurrentDevice));

        AL.Source(SourceHandle, ALSourcef.Pitch, 1f);
        AL.Source(SourceHandle, ALSourcef.Gain, 1f);
        AL.Source(SourceHandle, ALSource3f.Position, 0f, 0f, 0f);
        AL.Source(SourceHandle, ALSource3f.Velocity, 0f, 0f, 0f);
        AL.Source(SourceHandle, ALSourceb.Looping, false);
        AL.Source(SourceHandle, ALSourcei.Buffer, BufferHandle);
    }

    ~SimpleAudioSource()
    {
        Dispose(false);
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        AL.DeleteBuffer(BufferHandle);
        AL.DeleteSource(SourceHandle);
        
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public override void Play()
    {
        AL.SourcePlay(SourceHandle);
    }
}
