using System.Buffers;

namespace HCDE.Client;

public sealed class VmMusicWildMidiDeviceResources : IDisposable
{
    private readonly List<VmMusicWildMidiInstrumentPatch> patches = [];
    private bool disposed;
    public VmMusicWildMidiNoteTable Notes { get; } = new();

    public VmMusicWildMidiPatchLoadResult Load(ushort id,
        Func<ushort, VmMusicWildMidiInstrumentPatch?> findExact,
        Func<VmMusicWildMidiInstrumentPatch, int> loadSamples)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return VmMusicWildMidiPatchLoading.Load(id, patches, findExact, loadSamples);
    }

    public void RenderDry(Span<float> output, int frames, bool enhancedResampling = false)
        => Render(output, frames, enhancedResampling, null);

    public void Render(Span<float> output, int frames, bool enhancedResampling, VmMusicWildMidiReverbDelay? reverb)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(frames);
        var count = checked(frames * 2);
        if (output.Length < count) throw new ArgumentException("Output must hold the requested stereo frames.", nameof(output));
        if (count == 0) return;
        var buffer = ArrayPool<int>.Shared.Rent(count);
        try
        {
            var mixed = buffer.AsSpan(0, count);
            VmMusicWildMidiLinearMixer.Mix(Notes.Voices, mixed, enhancedResampling);
            reverb?.Process(mixed);
            VmMusicWildMidiMixerOutput.Convert(mixed, output[..count]);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer);
        }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        Notes.Clear();
        VmMusicWildMidiPatchLoading.Release(patches, patch => patch.FreeSamples());
    }
}
