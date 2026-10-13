namespace HCDE.Client;

// Borrows the renderer; MidiSession owns this adapter and borrows its source.
public sealed class VmMusicWildMidiSessionDevice : IVmMusicAudioSessionDevice, IVmMusicIntegerSettingsDevice,
    IVmMusicStatisticsDevice
{
    private readonly VmMusicWildMidiManagedRenderer renderer;
    private readonly VmMusicFmSessionDevice session;
    private bool disposed;
    private bool ownsRenderer;

    internal void OwnRenderer() => ownsRenderer = true;

    public VmMusicWildMidiSessionDevice(VmMusicWildMidiManagedRenderer renderer)
        : this(renderer, renderer?.SampleRate ?? throw new ArgumentNullException(nameof(renderer))) { }

    public VmMusicWildMidiSessionDevice(VmMusicWildMidiManagedRenderer renderer, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        if (sampleRate is < 11025 or > 65535) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        if (sampleRate != renderer.SampleRate)
            throw new ArgumentException("Session sample rate must match the renderer.", nameof(sampleRate));
        this.renderer = renderer;
        var adapter = new VmMusicWildMidiEventQueue.Adapter(renderer);
        session = new(adapter, adapter, sampleRate, 1f);
    }

    public bool IsOpen => session.IsOpen;
    public int Technology => session.Technology;
    public bool CanHandleSysex => true;
    public bool FakeVolume => true;
    public VmMusicStreamFormat GetStreamInfo() => session.GetStreamInfo();
    public bool ChangeInteger(string setting, int value)
    {
        RequireOpen();
        return VmMusicWildMidiSettings.ChangeInteger(renderer, setting, value);
    }
    public string GetStatistics()
    {
        RequireOpen();
        return VmMusicWildMidiOutput.GetStatistics(renderer);
    }
    private void RequireOpen()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!IsOpen) throw new InvalidOperationException("WildMIDI session device is closed.");
    }
    public int Open()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (IsOpen) throw new InvalidOperationException("WildMIDI session device is already open.");
        renderer.ResetPlayback();
        return session.Open();
    }
    public void PrecacheInstruments(IReadOnlyList<ushort> instruments)
    {
        if (!IsOpen) throw new InvalidOperationException("WildMIDI session device is closed.");
        ArgumentNullException.ThrowIfNull(instruments);
        VmMusicWildMidiSettings.PrecacheInstruments(renderer, instruments.ToArray());
    }
    public void SetCompletionCallback(Func<int>? callback) => session.SetCompletionCallback(callback);
    public int SetTimeDivision(ushort division)
    {
        if ((division & 0x8000) != 0) throw new NotSupportedException("WildMIDI sessions require PPQN time division.");
        return session.SetTimeDivision(division);
    }
    public int SetTempo(uint tempo) => session.SetTempo(tempo);
    public void InitializePlayback() => session.InitializePlayback();
    public int Prepare(int number, ReadOnlyMemory<uint> events) => session.Prepare(number, events);
    public int Unprepare(int number) => session.Unprepare(number);
    public int StreamOut(int number) => session.StreamOut(number);
    public int StreamOutSync(int number) => session.StreamOutSync(number);
    public int StartStream() => session.StartStream();
    public bool Pause(bool paused) => session.Pause(paused);
    public bool Update() => session.Update();
    public bool ServiceStream(Span<float> output) => session.ServiceStream(output);
    public bool ServiceStreamBytes(Span<byte> output) => session.ServiceStreamBytes(output);
    public void Stop() => session.Stop();
    public void Close() => session.Close();
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        try { session.Dispose(); }
        finally { if (ownsRenderer) renderer.Dispose(); }
    }
}
