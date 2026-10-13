namespace HCDE.Client.Tests;

public class VmMusicOutputStreamTests
{
    private sealed class Output : IVmMusicOutputStream
    {
        public List<string> Calls { get; } = [];
        public void Play(bool loop, float volume) { Assert.True(loop); Assert.Equal(1, volume); Calls.Add("play"); }
        public void SetPaused(bool paused) => Calls.Add(paused ? "pause" : "resume");
        public void Stop() => Calls.Add("stop");
        public void Dispose() => Calls.Add("dispose");
    }

    [Theory]
    [InlineData(true, 256)]
    [InlineData(false, 512)]
    public void CreateNegotiatesFloatOutputAndPreservesInputFormat(bool inputFloat, int outputBytes)
    {
        var state = new VmMusicOutputStream();
        var output = new Output();
        state.Create(true, () => new(256, 44100, 2, inputFloat), format =>
        {
            Assert.Equal(new VmMusicStreamFormat(outputBytes, 44100, 2, true), format);
            return output;
        });
        Assert.Equal(inputFloat, state.IsFloat);
        Assert.Same(output, state.Stream);
        state.Pause(true);
        state.Pause(false);
        state.Stop();
        state.Stop();
        Assert.Null(state.Stream);
        Assert.Equal(new[] { "play", "pause", "resume", "stop", "dispose" }, output.Calls);
    }

    [Fact]
    public void MissingSongOrDeviceOwnedOutputDoesNotCreateStream()
    {
        var state = new VmMusicOutputStream();
        state.Create(false, () => throw new InvalidOperationException(), _ => throw new InvalidOperationException());
        state.Create(true, () => new(0, 44100, 2, false), _ => throw new InvalidOperationException());
        state.Pause(true);
        state.Stop();
        Assert.Null(state.Stream);
    }

    [Fact]
    public void ReplacementDisposesPreviousStreamEvenWhenCreationReturnsNull()
    {
        var state = new VmMusicOutputStream();
        var previous = new Output();
        state.Create(true, () => new(256, 44100, 2, true), _ => previous);
        state.Create(true, () => new(256, 44100, 2, true), _ => null);
        Assert.Null(state.Stream);
        Assert.Equal(new[] { "play", "dispose" }, previous.Calls);
    }

    [Fact]
    public void FailedCreationLeavesNoStreamAndIntegerSizeOverflowIsReported()
    {
        var state = new VmMusicOutputStream();
        state.Create(true, () => new(256, 44100, 2, true), _ => null);
        Assert.Null(state.Stream);
        Assert.Throws<OverflowException>(() => state.Create(true, () => new(int.MaxValue, 44100, 2, false),
            _ => throw new InvalidOperationException()));
    }
}
