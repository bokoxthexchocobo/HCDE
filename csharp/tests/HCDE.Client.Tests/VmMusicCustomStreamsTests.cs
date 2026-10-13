namespace HCDE.Client.Tests;

public class VmMusicCustomStreamsTests
{
    private sealed class Stream : IVmMusicOutputStream
    {
        public List<string> Calls { get; } = [];
        public bool FailPlay { get; init; }
        public bool FailStop { get; init; }
        public bool FailDispose { get; init; }
        public void Play(bool loop, float volume)
        {
            Assert.True(loop);
            Assert.Equal(1, volume);
            Calls.Add("play");
            if (FailPlay) throw new InvalidOperationException("play failed");
        }
        public void SetPaused(bool paused) => Calls.Add(paused ? "pause" : "resume");
        public void Stop()
        {
            Calls.Add("stop");
            if (FailStop) throw new InvalidOperationException("stop failed");
        }
        public void Dispose()
        {
            Calls.Add("dispose");
            if (FailDispose) throw new InvalidOperationException("dispose failed");
        }
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void CreatesOriginalFormatStartsLoopingAndStopsBeforeDisposal(int channels, bool isFloat)
    {
        var manager = new VmMusicCustomStreams();
        var stream = new Stream();
        var format = new VmMusicStreamFormat(256, 22050, channels, isFloat);
        Assert.Same(stream, manager.Create(format, received => { Assert.Equal(format, received); return stream; }));
        manager.Stop(stream);
        manager.PauseAll(true);
        Assert.Equal(new[] { "play", "stop", "dispose" }, stream.Calls);
    }

    [Fact]
    public void FailedPlayDisposesWithoutRegisteringAndPropagatesFailure()
    {
        var manager = new VmMusicCustomStreams();
        var stream = new Stream { FailPlay = true };
        var error = Assert.Throws<InvalidOperationException>(() => manager.Create(new(256, 22050, 2, true), _ => stream));
        Assert.Equal("play failed", error.Message);
        manager.PauseAll(true);
        Assert.Equal(new[] { "play", "dispose" }, stream.Calls);
    }

    [Theory]
    [InlineData(true, false, "stop failed")]
    [InlineData(false, true, "dispose failed")]
    public void StopFailuresRemoveRegistrationAndAttemptDisposal(bool failStop, bool failDispose, string message)
    {
        var manager = new VmMusicCustomStreams();
        var stream = new Stream { FailStop = failStop, FailDispose = failDispose };
        manager.Create(new(256, 22050, 2, true), _ => stream);
        Assert.Equal(message, Assert.Throws<InvalidOperationException>(() => manager.Stop(stream)).Message);
        manager.PauseAll(true);
        Assert.Equal(new[] { "play", "stop", "dispose" }, stream.Calls);
    }

    [Fact]
    public void InvalidChannelsAndFailedCreationDoNotRegisterStreams()
    {
        var manager = new VmMusicCustomStreams();
        Assert.Null(manager.Create(new(256, 22050, 3, true), _ => throw new InvalidOperationException()));
        Assert.Null(manager.Create(new(256, 22050, 1, true), _ => null));
        manager.Stop(null);
        manager.PauseAll(true);
    }

    [Fact]
    public void PauseTransitionsSuppressDuplicatesAndNewStreamsDoNotInheritPause()
    {
        var manager = new VmMusicCustomStreams();
        var first = new Stream();
        var second = new Stream();
        var format = new VmMusicStreamFormat(256, 22050, 2, false);
        manager.Create(format, _ => first);
        manager.PauseAll(false);
        manager.PauseAll(true);
        manager.Create(format, _ => second);
        manager.PauseAll(true);
        manager.PauseAll(false);
        Assert.Equal(new[] { "play", "pause", "resume" }, first.Calls);
        Assert.Equal(new[] { "play", "resume" }, second.Calls);
        manager.Stop(first);
        manager.Stop(second);
    }
}
