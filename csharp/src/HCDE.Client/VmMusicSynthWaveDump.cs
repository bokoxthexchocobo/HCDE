namespace HCDE.Client;

public static class VmMusicSynthWaveDump
{
    public static Func<IVmMusicSmfSource, VmMusicWaveRequest, bool> CreateRouter(
        VmMusicMidiDeviceKind defaultDevice,
        Func<VmMusicWaveRequest, VmMusicFmSessionDevice>? createFm,
        Func<VmMusicWaveRequest, VmMusicWildMidiManagedRenderer>? createWildMidi,
        CancellationToken cancellationToken = default)
    {
        if (defaultDevice is not (VmMusicMidiDeviceKind.Adl or VmMusicMidiDeviceKind.Opn or VmMusicMidiDeviceKind.WildMidi))
            throw new ArgumentOutOfRangeException(nameof(defaultDevice));
        return (source, request) =>
        {
            ArgumentNullException.ThrowIfNull(source);
            var kind = request.Device == -1 ? defaultDevice : (VmMusicMidiDeviceKind)request.Device;
            var selected = request with { Device = (int)kind };
            return kind switch
            {
                VmMusicMidiDeviceKind.Adl or VmMusicMidiDeviceKind.Opn => createFm is null
                    ? throw new VmMusicMidiDeviceUnavailableException("FM WAVE backend is unavailable.")
                    : VmMusicFmWaveDump.Write(source, selected, createFm, cancellationToken),
                VmMusicMidiDeviceKind.WildMidi => createWildMidi is null
                    ? throw new VmMusicMidiDeviceUnavailableException("WildMIDI WAVE backend is unavailable.")
                    : VmMusicWildMidiWaveDump.Write(source, selected, createWildMidi, cancellationToken),
                _ => throw new NotSupportedException($"WAVE backend {kind} is not configured by this router.")
            };
        };
    }
}
