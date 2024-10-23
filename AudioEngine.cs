using OpenTK.Audio.OpenAL;

namespace HPEngine;

public class AudioEngine
{
    internal static ALDevice CurrentDevice;
    internal ALContext AlContext;
    internal ALDevice Device;

    public AudioEngine()
    {
        Device = ALC.OpenDevice(null);
        CurrentDevice = Device;

        AlContext = ALC.CreateContext(Device, new ALContextAttributes {});
        ALC.MakeContextCurrent(AlContext);
    }

    ~AudioEngine()
    {
        ALC.DestroyContext(AlContext);
        ALC.CloseDevice(Device);
    }
}
