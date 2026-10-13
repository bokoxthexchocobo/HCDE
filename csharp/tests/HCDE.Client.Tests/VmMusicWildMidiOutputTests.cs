namespace HCDE.Client.Tests;

public class VmMusicWildMidiOutputTests
{
    private sealed class Renderer : IVmMusicWildMidiRenderer
    {
        public (int, int, int) Event;
        public byte[] Long = [];
        public int Frames = -1;
        public int Voices;
        public void ShortEvent(int status, int first, int second) => Event = (status, first, second);
        public void LongEvent(ReadOnlySpan<byte> data) => Long = data.ToArray();
        public void ComputeOutput(Span<float> samples, int frames) { Frames = frames; samples.Fill(2.5f); }
        public int GetVoiceCount() => Voices;
        public void LoadInstrument(int bank, int drum, int instrument) { }
        public void SetOption(int mask, int value) { }
    }

    [Fact]
    public void EventsForwardRawValuesAndExactSysexBytes()
    {
        var renderer = new Renderer();
        VmMusicWildMidiOutput.HandleEvent(renderer, 0x91, 255, -1);
        Assert.Equal((0x91, 255, -1), renderer.Event);
        VmMusicWildMidiOutput.HandleLongEvent(renderer, [0xf0, 0, 127, 0xf7]);
        Assert.Equal(new byte[] { 0xf0, 0, 127, 0xf7 }, renderer.Long);
    }

    [Fact]
    public void OutputForwardsFrameCountAndRestrictsWritableBufferWithoutClipping()
    {
        var renderer = new Renderer();
        var samples = new float[] { 9, 9, 9, 9, 9 };
        VmMusicWildMidiOutput.ComputeOutput(renderer, samples, 2);
        Assert.Equal(2, renderer.Frames);
        Assert.Equal(new float[] { 2.5f, 2.5f, 2.5f, 2.5f, 9 }, samples);
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiOutput.ComputeOutput(renderer, samples, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiOutput.ComputeOutput(renderer, samples, -1));
        Assert.Equal(2, renderer.Frames);
    }

    [Theory]
    [InlineData(0, "  0 voices")]
    [InlineData(1234, "1234 voices")]
    [InlineData(-1, " -1 voices")]
    public void StatisticsMatchNativeMinimumWidth(int voices, string expected)
    {
        Assert.Equal(expected, VmMusicWildMidiOutput.GetStatistics(new Renderer { Voices = voices }));
    }
}
