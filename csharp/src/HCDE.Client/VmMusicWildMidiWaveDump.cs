namespace HCDE.Client;

public static class VmMusicWildMidiWaveDump
{
    // Borrows the source; owns the factory-created renderer and output file.
    public static bool Write(IVmMusicSmfSource source, VmMusicWaveRequest request,
        Func<VmMusicWaveRequest, VmMusicWildMidiManagedRenderer> createRenderer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(createRenderer);
        if (request.Device is not (-1 or 6))
            throw new NotSupportedException("This WAVE exporter supports only WildMIDI.");
        VmMusicWildMidiManagedRenderer? renderer = null;
        try
        {
            return VmMusicFmWaveDump.WriteAudio(source, request, value =>
            {
                renderer = createRenderer(value)
                    ?? throw new InvalidOperationException("WAVE factory returned no WildMIDI renderer.");
                return new VmMusicWildMidiSessionDevice(renderer);
            }, cancellationToken);
        }
        finally { renderer?.Dispose(); }
    }
}
