namespace HCDE.Client;

// Source remains borrowed. The host serializes callbacks, updates and disposal.
public sealed class VmMusicSynthStreamPlayback : IDisposable
{
    private readonly VmMusicMidiSession session;
    private readonly IVmMusicAudioSessionDevice device;
    private readonly VmMusicStreamPlayback output = new();
    private bool ended;
    private bool disposed;

    public float OutputVolume
    {
        get => output.MusicVolume;
        set
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            output.MusicVolume = value;
        }
    }

    public bool ChangeInteger(string setting, int value)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(setting);
        return device is IVmMusicIntegerSettingsDevice settings && settings.ChangeInteger(setting, value);
    }

    public bool ChangeNumber(string setting, double value)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(setting);
        return device is IVmMusicNumberSettingsDevice settings && settings.ChangeNumber(setting, value);
    }

    public bool ChangeString(string setting, string value)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(setting);
        ArgumentNullException.ThrowIfNull(value);
        return device is IVmMusicStringSettingsDevice settings && settings.ChangeString(setting, value);
    }

    public string GetStatistics()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!device.IsOpen) return "No MIDI device in use.";
        return device is IVmMusicStatisticsDevice statistics ? statistics.GetStatistics()
            : "This MIDI device does not have any stats.";
    }

    private VmMusicSynthStreamPlayback(VmMusicMidiSession session, IVmMusicAudioSessionDevice device)
    {
        this.session = session;
        this.device = device;
    }

    public static VmMusicSynthStreamPlayback StartConfigured(IVmMusicSmfSource source,
        VmMusicResolvedSettings settings, int deviceSetting, bool systemMidi,
        Func<VmMusicMidiDeviceKind, string, IVmMusicAudioSessionDevice> createDevice,
        VmMusicMidiPlayOptions options, VmMusicOutputFactory createOutput,
        Func<int, string, float> getReplayGain)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings.Arguments);
        ArgumentNullException.ThrowIfNull(createDevice);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(createOutput);
        ArgumentNullException.ThrowIfNull(getReplayGain);
        var kind = (VmMusicMidiDeviceKind)settings.Device;
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(settings));
        var gain = settings.Volume ?? getReplayGain(settings.Device, settings.Arguments);
        return Start(source, kind, deviceSetting, systemMidi,
            selected => createDevice(selected, settings.Arguments), options, createOutput, gain);
    }

    public static VmMusicSynthStreamPlayback Start(IVmMusicSmfSource source, VmMusicMidiDeviceKind kind,
        int setting, bool systemMidi, Func<VmMusicMidiDeviceKind, IVmMusicAudioSessionDevice> createDevice,
        VmMusicMidiPlayOptions options, VmMusicOutputFactory createOutput, float outputVolume = 1)
    {
        ArgumentNullException.ThrowIfNull(createDevice);
        ArgumentNullException.ThrowIfNull(createOutput);
        if (!float.IsFinite(outputVolume) || outputVolume < 0)
            throw new ArgumentOutOfRangeException(nameof(outputVolume));
        IVmMusicAudioSessionDevice? selected = null;
        var playing = VmMusicMidiPlay.Start(source, kind, setting, systemMidi,
            value => selected = createDevice(value), options);
        var playback = new VmMusicSynthStreamPlayback(playing.Session, selected!);
        try
        {
            if (!playing.Started) throw new InvalidOperationException("MIDI audio session did not start.");
            playback.OutputVolume = outputVolume;
            playback.output.Create(true, playback.device.GetStreamInfo,
                (format, fill) => createOutput(format, fill)
                    ?? throw new VmMusicMidiDeviceUnavailableException("Music output stream is unavailable."),
                playback.Fill, _ => throw new NotSupportedException("Synth sessions require float audio output."));
            return playback;
        }
        catch (Exception startupError)
        {
            try { playback.Dispose(); }
            catch (Exception cleanupError)
            {
                throw new AggregateException("Music stream startup and cleanup failed.", startupError, cleanupError);
            }
            throw;
        }
    }

    private bool Fill(Span<float> samples)
    {
        if (disposed || ended) { samples.Clear(); return false; }
        var completed = false;
        try
        {
            var active = device.ServiceStream(samples);
            if (!active) ended = true;
            completed = true;
            return active;
        }
        finally
        {
            if (!completed) { ended = true; samples.Clear(); }
        }
    }

    public void Pause(bool paused)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        device.Pause(paused);
        output.Pause(paused);
    }

    public void Update()
    {
        if (disposed) return;
        if (ended) { Dispose(); return; }
        try { session.Update(); }
        catch (Exception updateError)
        {
            try { Dispose(); }
            catch (Exception cleanupError)
            {
                throw new AggregateException("Music stream update and cleanup failed.", updateError, cleanupError);
            }
            throw;
        }
        if (!device.IsOpen) Dispose();
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        List<Exception> errors = [];
        try { output.Stop(); } catch (Exception error) { errors.Add(error); }
        try { session.Dispose(); } catch (Exception error) { errors.Add(error); }
        if (errors.Count != 0) throw new AggregateException("Music stream cleanup failed.", errors);
    }
}
