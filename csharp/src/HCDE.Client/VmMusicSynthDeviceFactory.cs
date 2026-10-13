namespace HCDE.Client;

public static class VmMusicSynthDeviceFactory
{
    public static Func<VmMusicMidiDeviceKind, IVmMusicMidiSessionDevice> CreateConfiguredRouter(
        Func<VmMusicMidiDeviceKind, string, VmMusicFmSessionDevice>? createFm,
        VmMusicWildMidiResourceCache wildMidiCache, string defaultConfiguration, string arguments,
        int requestedSampleRate, bool enhancedResampling, bool reverb)
    {
        ArgumentNullException.ThrowIfNull(wildMidiCache);
        ArgumentException.ThrowIfNullOrEmpty(defaultConfiguration);
        ArgumentNullException.ThrowIfNull(arguments);
        var configuration = arguments.Length == 0 ? defaultConfiguration : arguments;
        return CreateRouter(createFm is null ? null : kind => createFm(kind, arguments),
            () => wildMidiCache.CreateRenderer(configuration, requestedSampleRate, enhancedResampling, reverb));
    }

    public static Func<VmMusicMidiDeviceKind, IVmMusicMidiSessionDevice> CreateRouter(
        Func<VmMusicMidiDeviceKind, VmMusicFmSessionDevice>? createFm,
        Func<VmMusicWildMidiManagedRenderer>? createWildMidi)
    {
        return kind => kind switch
        {
            VmMusicMidiDeviceKind.Adl or VmMusicMidiDeviceKind.Opn => createFm is null
                ? throw new VmMusicMidiDeviceUnavailableException("FM MIDI backend is unavailable.")
                : createFm(kind) ?? throw new InvalidOperationException("FM factory returned no device."),
            VmMusicMidiDeviceKind.WildMidi => createWildMidi is null
                ? throw new VmMusicMidiDeviceUnavailableException("WildMIDI backend is unavailable.")
                : CreateWildMidi(createWildMidi),
            _ when !Enum.IsDefined(kind) || kind == VmMusicMidiDeviceKind.Default =>
                throw new ArgumentOutOfRangeException(nameof(kind)),
            _ => throw new VmMusicMidiDeviceUnavailableException($"MIDI backend {kind} is unavailable.")
        };
    }

    private static VmMusicWildMidiSessionDevice CreateWildMidi(Func<VmMusicWildMidiManagedRenderer> create)
    {
        var renderer = create() ?? throw new InvalidOperationException("WildMIDI factory returned no renderer.");
        try
        {
            var device = new VmMusicWildMidiSessionDevice(renderer);
            device.OwnRenderer();
            return device;
        }
        catch (Exception initializationError)
        {
            try { renderer.Dispose(); }
            catch (Exception cleanupError)
            {
                throw new AggregateException("WildMIDI session initialization and cleanup failed.", initializationError, cleanupError);
            }
            throw;
        }
    }
}
