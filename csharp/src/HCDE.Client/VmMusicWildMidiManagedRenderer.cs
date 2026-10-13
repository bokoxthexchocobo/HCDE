namespace HCDE.Client;

public sealed class VmMusicWildMidiManagedRenderer : IVmMusicWildMidiRenderer, IDisposable
{
    private readonly VmMusicWildMidiDeviceResources resources = new();
    private readonly VmMusicWildMidiChannelState[] channels;
    private readonly Func<ushort, VmMusicWildMidiInstrumentPatch?> findExact;
    private readonly Func<VmMusicWildMidiInstrumentPatch, int> loadSamples;
    private readonly int sampleRate;
    private bool enhanced;
    private bool logarithmic;
    private int masterVolume = 1024;
    private bool disposed;
    private readonly VmMusicWildMidiReverbDelay reverb;
    private bool reverbEnabled;
    private IDisposable? ownedBankReader;

    internal void OwnBankReader(IDisposable reader)
    {
        CheckDisposed();
        ArgumentNullException.ThrowIfNull(reader);
        if (ownedBankReader is not null) throw new InvalidOperationException("Renderer already owns a bank reader.");
        ownedBankReader = reader;
    }

    public int SampleRate => sampleRate;

    public VmMusicWildMidiManagedRenderer(int sampleRate,
        Func<ushort, VmMusicWildMidiInstrumentPatch?> findExact,
        Func<VmMusicWildMidiInstrumentPatch, int> loadSamples, VmMusicWildMidiReverbSettings? reverbSettings = null)
    {
        if (sampleRate is < 11025 or > 65535) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        ArgumentNullException.ThrowIfNull(findExact);
        ArgumentNullException.ThrowIfNull(loadSamples);
        this.sampleRate = sampleRate;
        this.findExact = findExact;
        this.loadSamples = loadSamples;
        reverb = VmMusicWildMidiReverbRoom.Create(sampleRate, reverbSettings ?? new());
        channels = Enumerable.Range(0, 16).Select(_ => new VmMusicWildMidiChannelState()).ToArray();
        try
        {
            resources.Load(0, findExact, loadSamples);
            VmMusicWildMidiPatchLookup.ResetChannels(channels, id => findExact(id), AdjustVolumes);
        }
        catch
        {
            resources.Dispose();
            throw;
        }
    }

    public void ShortEvent(int status, int parameter1, int parameter2)
    {
        CheckDisposed();
        VmMusicWildMidiShortEvents.Dispatch(resources.Notes, channels, id => findExact(id),
            status, parameter1, parameter2, sampleRate, masterVolume, logarithmic);
    }

    public void LongEvent(ReadOnlySpan<byte> data)
    {
        CheckDisposed();
        VmMusicWildMidiSystemExclusive.Handle(data, channels, id => findExact(id), AdjustVolumes);
    }

    public void LoadInstrument(int bank, int drumSet, int instrument)
    {
        CheckDisposed();
        resources.Load(VmMusicWildMidiPatchLookup.InstrumentId(bank, drumSet, instrument), findExact, loadSamples);
    }

    public void SetOption(int mask, int value)
    {
        CheckDisposed();
        if (mask == 0 || (mask & ~7) != 0) throw new ArgumentOutOfRangeException(nameof(mask));
        if ((value & ~7) != 0) throw new ArgumentOutOfRangeException(nameof(value));
        if ((mask & 4) != 0)
        {
            if ((mask & 1) == 0) reverb.Reset();
            reverbEnabled = (value & 4) != 0;
        }
        if ((mask & 2) != 0) enhanced = (value & 2) != 0;
        if ((mask & 1) != 0)
        {
            logarithmic = (value & 1) != 0;
            AdjustVolumes(16);
        }
    }

    public void SetMasterVolume(byte volume)
    {
        CheckDisposed();
        // Native setter changes the scalar; existing voice volumes update on the next volume event.
        masterVolume = VmMusicWildMidiNoteVolume.MasterVolume(volume);
    }

    public void ComputeOutput(Span<float> stereoSamples, int frames)
    {
        CheckDisposed();
        resources.Render(stereoSamples, frames, enhanced, reverbEnabled ? reverb : null);
    }

    public int GetVoiceCount()
    {
        CheckDisposed();
        return resources.Notes.Voices.Count;
    }

    public void ResetPlayback()
    {
        CheckDisposed();
        resources.Notes.Clear();
        reverb.Reset();
        // Playback restart recreates native channel initialization, not just a SysEx reset.
        foreach (var channel in channels)
        {
            channel.PitchAdjust = 0;
            channel.NonRegistered = 0;
        }
        VmMusicWildMidiPatchLookup.ResetChannels(channels, id => findExact(id), AdjustVolumes);
    }

    private void AdjustVolumes(int channel) => VmMusicWildMidiChannelVolume.Adjust(resources.Notes.Voices,
        channel, (ch, voice) => VmMusicWildMidiNoteVolume.Adjust(channels, ch, voice, masterVolume, logarithmic));

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(disposed, this);

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        try { resources.Dispose(); }
        finally
        {
            foreach (var channel in channels) channel.Patch = null;
            ownedBankReader?.Dispose();
        }
    }
}
