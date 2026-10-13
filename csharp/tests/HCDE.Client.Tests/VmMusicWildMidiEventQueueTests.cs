using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiEventQueueTests
{
    private sealed class Renderer : IVmMusicWildMidiRenderer
    {
        public readonly List<(int, int, int)> Events = [];
        public byte[] Long = [];
        private float value;
        public void ShortEvent(int status, int first, int second) { Events.Add((status, first, second)); if (status == 0x90) value = 2; if (status == 0x80) value = 0; }
        public void LongEvent(ReadOnlySpan<byte> data) => Long = data.ToArray();
        public void ComputeOutput(Span<float> output, int frames) => output[..(frames * 2)].Fill(value);
        public int GetVoiceCount() => 0;
        public void LoadInstrument(int bank, int drumSet, int instrument) { }
        public void SetOption(int mask, int value) { }
    }

    [Fact]
    public void TickBoundariesDispatchNotesWithoutApplyingExtraGain()
    {
        var renderer = new Renderer();
        var queue = new VmMusicWildMidiEventQueue(renderer, 11025);
        queue.SetTimeDivision(11025); queue.SetTempo(1000000);
        queue.Submit(new uint[] { 2, 0, 0x643c90, 3, 0, 0x3c80 });
        var output = new float[12];
        queue.Render(output);
        Assert.Equal(new float[] { 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 0, 0 }, output);
        Assert.Equal(new[] { (0x90, 60, 100), (0x80, 60, 0) }, renderer.Events);
    }

    [Fact]
    public void PitchSysexCompletionAndPostQueueAudioAreForwarded()
    {
        var renderer = new Renderer();
        var queue = new VmMusicWildMidiEventQueue(renderer, 44100);
        var completed = 0; queue.BufferCompleted = () => completed++;
        queue.Submit(new uint[] { 0, 0, 0x4021e2, 0, 0, 0x80000003, 0x00f701f0, 0, 0, 0x643c90 });
        queue.Render(new float[2]);
        Assert.Contains((0xe2, 33, 64), renderer.Events);
        Assert.Equal(new byte[] { 240, 1, 247 }, renderer.Long);
        Assert.Equal(1, completed);
        var tail = new float[4]; queue.Render(tail);
        Assert.Equal(new float[] { 2, 2, 2, 2 }, tail);
        Assert.Throws<NotSupportedException>(() => queue.SetTimeDivision(0x8001));
        Assert.Throws<ArgumentException>(() => queue.Render(new float[1]));
    }
}
