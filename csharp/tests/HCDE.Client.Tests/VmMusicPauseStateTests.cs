namespace HCDE.Client.Tests;

public class VmMusicPauseStateTests
{
    [Fact]
    public void PauseAndResumeNotifySongThenStreamBeforeChangingState()
    {
        var state = new VmMusicPauseState();
        var calls = new List<string>();
        state.Pause(true, () => { Assert.False(state.Paused); calls.Add("pause-song"); }, paused =>
        {
            Assert.True(paused);
            Assert.False(state.Paused);
            calls.Add("pause-stream");
        });
        Assert.True(state.Paused);
        state.Resume(true, () => { Assert.True(state.Paused); calls.Add("resume-song"); }, paused =>
        {
            Assert.False(paused);
            Assert.True(state.Paused);
            calls.Add("resume-stream");
        });
        Assert.False(state.Paused);
        Assert.Equal(new[] { "pause-song", "pause-stream", "resume-song", "resume-stream" }, calls);
    }

    [Fact]
    public void MissingSongsAndRepeatedRequestsDoNotNotifyOrChangeState()
    {
        var state = new VmMusicPauseState();
        Action unexpected = () => throw new InvalidOperationException("unexpected callback");
        Action<bool> unexpectedStream = _ => unexpected();
        state.Pause(false, unexpected, unexpectedStream);
        state.Resume(true, unexpected, unexpectedStream);
        Assert.False(state.Paused);
        state.Pause(true, () => { }, _ => { });
        state.Pause(true, unexpected, unexpectedStream);
        state.Resume(false, unexpected, unexpectedStream);
        Assert.True(state.Paused);
    }

    [Fact]
    public void ResetStopsPausedMusicWithForceAndClearsState()
    {
        var state = new VmMusicPauseState();
        var stops = 0;
        state.Reset(_ => stops++);
        Assert.Equal(0, stops);
        state.Pause(true, () => { }, _ => { });
        state.Reset(force => { Assert.True(force); Assert.True(state.Paused); stops++; });
        Assert.False(state.Paused);
        state.Reset(_ => stops++);
        Assert.Equal(1, stops);
    }

    [Fact]
    public void FailedNotificationsPropagateWithoutCommittingState()
    {
        var state = new VmMusicPauseState();
        Assert.Throws<InvalidOperationException>(() => state.Pause(true, () => { },
            _ => throw new InvalidOperationException("stream failed")));
        Assert.False(state.Paused);
        state.Pause(true, () => { }, _ => { });
        Assert.Throws<InvalidOperationException>(() => state.Resume(true,
            () => throw new InvalidOperationException("song failed"), _ => { }));
        Assert.True(state.Paused);
        Assert.Throws<InvalidOperationException>(() => state.Reset(
            _ => throw new InvalidOperationException("stop failed")));
        Assert.True(state.Paused);
    }
}
