namespace HCDE.Client;

public sealed class VmMusicCustomStreams
{
    private readonly List<IVmMusicOutputStream> streams = [];
    private bool paused;

    public IVmMusicOutputStream? Create(VmMusicStreamFormat format,
        Func<VmMusicStreamFormat, IVmMusicOutputStream?> createStream)
    {
        ArgumentNullException.ThrowIfNull(createStream);
        if (format.Channels is not (1 or 2)) return null;
        var stream = createStream(format);
        if (stream is null) return null;
        var registered = false;
        try
        {
            stream.Play(true, 1);
            streams.Add(stream);
            registered = true;
            return stream;
        }
        finally
        {
            if (!registered) stream.Dispose();
        }
    }

    public void Stop(IVmMusicOutputStream? stream)
    {
        if (stream is null) return;
        try
        {
            stream.Stop();
        }
        finally
        {
            var index = streams.FindIndex(candidate => ReferenceEquals(candidate, stream));
            if (index >= 0) streams.RemoveAt(index);
            stream.Dispose();
        }
    }

    public void PauseAll(bool on)
    {
        if (paused == on) return;
        paused = on;
        foreach (var stream in streams) stream.SetPaused(on);
    }
}
