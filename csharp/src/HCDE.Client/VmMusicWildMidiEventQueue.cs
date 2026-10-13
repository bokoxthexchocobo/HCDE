namespace HCDE.Client;

// Borrows the renderer. The caller serializes MIDI submission and audio processing.
public sealed class VmMusicWildMidiEventQueue
{
    private readonly Adapter adapter;
    private readonly VmMusicFmEventQueue queue;

    public VmMusicWildMidiEventQueue(IVmMusicWildMidiRenderer renderer, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        if (sampleRate is < 11025 or > 65535) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        adapter = new(renderer);
        queue = new(adapter, sampleRate);
    }

    public bool HasEvents => queue.HasEvents;
    public Action? BufferCompleted { get => queue.BufferCompleted; set => queue.BufferCompleted = value; }
    public void SetTempo(uint tempo) => queue.SetTempo(tempo);
    public void SetTimeDivision(ushort division)
    {
        if ((division & 0x8000) != 0) throw new NotSupportedException("WildMIDI event scheduling requires PPQN time division.");
        queue.SetTimeDivision(division);
    }
    public void Submit(ReadOnlySpan<uint> buffer) => queue.Submit(buffer);

    public bool Render(Span<float> stereoSamples)
    {
        if (!queue.HasEvents)
        {
            adapter.Generate(stereoSamples);
            return false;
        }
        return queue.ServiceStream(adapter, stereoSamples, 1f);
    }

    internal sealed class Adapter(IVmMusicWildMidiRenderer renderer) : IVmMusicFmMidiDevice, IVmMusicFmAudioDevice
    {
        public void NoteOn(int channel, int note, int velocity) => renderer.ShortEvent(0x90 | channel, note, velocity);
        public void NoteOff(int channel, int note) => renderer.ShortEvent(0x80 | channel, note, 0);
        public void NoteAfterTouch(int channel, int note, int pressure) => renderer.ShortEvent(0xa0 | channel, note, pressure);
        public void ControllerChange(int channel, int controller, int value) => renderer.ShortEvent(0xb0 | channel, controller, value);
        public void PatchChange(int channel, int program) => renderer.ShortEvent(0xc0 | channel, program, 0);
        public void ChannelAfterTouch(int channel, int pressure) => renderer.ShortEvent(0xd0 | channel, pressure, 0);
        public void PitchBend(int channel, int mostSignificant, int leastSignificant) => renderer.ShortEvent(0xe0 | channel, leastSignificant, mostSignificant);
        public void SystemExclusive(ReadOnlySpan<byte> data) => renderer.LongEvent(data);
        public int Generate(Span<float> stereoSamples)
        {
            if ((stereoSamples.Length & 1) != 0) throw new ArgumentException("WildMIDI requires complete stereo frames.", nameof(stereoSamples));
            renderer.ComputeOutput(stereoSamples, stereoSamples.Length / 2);
            return stereoSamples.Length;
        }
    }
}
