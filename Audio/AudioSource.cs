using NVorbis;
using OpenTK.Audio.OpenAL;

namespace HPEngine.Audio;

internal class SoundData
{
    public int ChannelCount;
    public int BitDepth;
    public int SampleRate;
    public byte[] Samples;

    public SoundData(int channelCount, int bitDepth, int sampleRate, byte[] samples)
    {
        ChannelCount = channelCount;
        BitDepth = bitDepth;
        SampleRate = sampleRate;
        Samples = samples;
    }

    public ALFormat GetALFormat()
    {
        if (ChannelCount == 1 && BitDepth == 8)
            return ALFormat.Mono8;
        else if (ChannelCount == 1 && BitDepth == 16)
            return ALFormat.Mono16;
        else if (ChannelCount == 2 && BitDepth == 8)
            return ALFormat.Stereo8;
        else if (ChannelCount == 2 && BitDepth == 16)
            return ALFormat.Stereo16;

        throw new NotImplementedException();
    }
}

public abstract class AudioSource
{
    private static SoundData LoadWave(Stream stream)
    {
        using (var reader = new BinaryReader(stream))
        {
            var magicNumber = new string(reader.ReadChars(4));
            if (magicNumber != "RIFF")
                throw new InvalidDataException("Not a wave file (Did not find 'RIFF')");

            var fileSize = reader.ReadInt32();

            var format = new string(reader.ReadChars(4));
            if (format != "WAVE")
                throw new InvalidDataException("Not a wave file (Did not find 'WAVE')");

            var formatSignature = new string(reader.ReadChars(4));
            if (formatSignature != "fmt ")
                throw new InvalidDataException("Unsupported wave format");

            var formatChunkSize = reader.ReadInt32();
            var audioFormat = reader.ReadInt16();
            var channelCount = reader.ReadInt16();
            var sampleRate = reader.ReadInt32();
            var byteRate = reader.ReadInt32();
            var blockAlign = reader.ReadInt16();
            var bitDepth = reader.ReadInt16();
            
            var dataSignature = new string(reader.ReadChars(4));
            if (dataSignature != "data")
                throw new InvalidDataException("Corrupted data");

            var dataChunkSize = reader.ReadInt32();

            return new SoundData(
                    channelCount, bitDepth, sampleRate,
                    reader.ReadBytes((int)reader.BaseStream.Length));
        }
    }

    private static SoundData LoadOgg(Stream stream)
    {
        var vorbis = new VorbisReader(stream);

        var channelCount = vorbis.Channels;
        var sampleRate = vorbis.SampleRate;
        var sampleSize = (int)(sampleRate * (vorbis.TotalTime.Seconds + 0.5f));
        var samples = new float[sampleSize];

        var samplesRead = vorbis.ReadSamples(samples, 0, sampleSize);

        var bytes = new byte[sampleSize];
        for (var i = 0; i < samples.Length; i++)
        {
            var sample = (samples[i] + 1f) / 2f;
            var temp = (int)(255f * sample);
            if (temp > byte.MaxValue) temp = byte.MaxValue;
            if (temp < byte.MinValue) temp = byte.MinValue;
            bytes[i] = (byte)temp;
        }

        return new SoundData(channelCount, 8, sampleRate, bytes);
    }

    internal static SoundData LoadData(string path)
    {
        if (Path.GetExtension(path) == ".wav")
        {
            return LoadWave(File.OpenRead(path));
        }
        else if (Path.GetExtension(path) == ".ogg")
        {
            return LoadOgg(File.OpenRead(path));
        }

        Console.Error.WriteLine($"Unsupported file format for '{path}'");
        return new SoundData(1, 8, 10000, new byte[] {});
    }

    public abstract void Play();
}
