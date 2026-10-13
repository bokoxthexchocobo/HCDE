using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiReverbDelayTests
{
    private static VmMusicWildMidiReverbDelay Create(int size)
    {
        var coefficients = new int[8, 6, 5];
        coefficients[0, 0, 0] = 1024;
        return new(size, size, new int[8], new int[8], new int[4], new int[4], coefficients);
    }

    [Fact]
    public void InitialReflectionAndOppositeChannelFeedbackUseWetOutput()
    {
        var delay = Create(1);
        int[] frames = [4096, 0, 0, 0, 0, 0];
        delay.Process(frames);
        Assert.Equal(new[] { 4128, 32, 0, 32, 0, 0 }, frames);
    }

    [Fact]
    public void ProcessingChunksAndResetPreserveRingPositions()
    {
        int[] whole = [4096, -2048, 0, 0, 0, 0, 0, 0];
        var chunks = (int[])whole.Clone();
        Create(3).Process(whole);
        var delay = Create(3);
        delay.Process(chunks.AsSpan(0, 2));
        delay.Process(chunks.AsSpan(2));
        Assert.Equal(whole, chunks);
        delay.Reset();
        int[] silence = new int[12];
        delay.Process(silence);
        Assert.All(silence, value => Assert.Equal(0, value));
        int[] restarted = [4096, 0];
        delay.Process(restarted);
        Assert.Equal(new[] { 4128, 32 }, restarted);
    }

    [Fact]
    public void InvalidFramesFailBeforeAdvancingState()
    {
        var delay = Create(1);
        Assert.Throws<ArgumentException>(() => delay.Process(new int[1]));
        int[] frame = [4096, 0];
        delay.Process(frame);
        Assert.Equal(new[] { 4128, 32 }, frame);
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(0));
    }
}
