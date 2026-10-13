namespace HCDE.Client;

public interface IVmMusicOutputStream : IDisposable
{
    void Play(bool loop, float volume);
    void SetPaused(bool paused);
    void Stop();
}

public readonly record struct VmMusicStreamFormat(int BufferBytes, int SampleRate, int Channels, bool IsFloat);

public sealed class VmMusicOutputStream
{
    public IVmMusicOutputStream? Stream { get; private set; }
    public bool IsFloat { get; private set; }

    public void Create(bool hasSong, Func<VmMusicStreamFormat> getFormat,
        Func<VmMusicStreamFormat, IVmMusicOutputStream?> createFloatStream)
    {
        ArgumentNullException.ThrowIfNull(getFormat);
        ArgumentNullException.ThrowIfNull(createFloatStream);
        if (!hasSong) return;
        var format = getFormat();
        IsFloat = format.IsFloat;
        var outputBytes = format.IsFloat ? format.BufferBytes : checked(format.BufferBytes * 2);
        if (outputBytes <= 0) return;
        var created = createFloatStream(format with { BufferBytes = outputBytes, IsFloat = true });
        Stream?.Dispose();
        Stream = created;
        Stream?.Play(true, 1);
    }

    public void Pause(bool paused) => Stream?.SetPaused(paused);

    public void Stop()
    {
        if (Stream is null) return;
        var stream = Stream;
        Stream = null;
        try { stream.Stop(); }
        finally { stream.Dispose(); }
    }
}
